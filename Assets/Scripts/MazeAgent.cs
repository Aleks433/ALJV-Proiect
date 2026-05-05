using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public class MazeAgent : Agent
{
    [Header("References")]
    public MazeGenerator mazeGenerator;
    public Transform endTransform;

    [Header("Movement")]
    public float moveSpeed = 10f;
    public float baseMoveSpeed = 10f;
    public float turnSpeed = 180f;
    private bool isMoving = false;
    [Header("Health")]
    public int currentHealth;
    public int maxHealth;


    private CharacterController controller;
    private float distanceToPreviousStep;
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
    }

    public void TakeDamage(int damage)
    {
       currentHealth -= damage; 
       AddReward(-(damage / maxHealth) * 0.5f);

       if(currentHealth <= 0)
        {
            AddReward(-10f);
            EndEpisode();
        }
    }
    public void ApplyMovementModifier(float movement)
    {
        moveSpeed = movement;
        AddReward(-0.0001f);
    }

    public void TeleportAgent(Transform where)
    {
        StopAllCoroutines();
        isMoving = true;
        controller.enabled = false;
        transform.SetPositionAndRotation(new Vector3(where.position.x, where.position.y, where.position.z), Quaternion.identity);
        controller.enabled = true;
        isMoving = false;

    }
    // Called at the start of every training episode
    public override void OnEpisodeBegin()
    {
        StopAllCoroutines();
        currentHealth = maxHealth;
        moveSpeed = baseMoveSpeed;

        isMoving = false;
        // Generate a fresh maze
        mazeGenerator.Generate();

        // Move agent to start
        controller.enabled = false;
        transform.position = mazeGenerator.startPosition.position;
        transform.rotation = Quaternion.identity;
        controller.enabled = true;

        // Update end reference
        endTransform = mazeGenerator.endPosition;

        distanceToPreviousStep = Vector3.Distance(transform.position, endTransform.position);
    }

    // Define what the agent observes
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position normalized by maze size
        sensor.AddObservation(transform.localPosition.x / (mazeGenerator.width * 3));
        sensor.AddObservation(transform.localPosition.z / (mazeGenerator.height * 3));

        // End position normalized
        sensor.AddObservation(endTransform.position.x / (mazeGenerator.width * 3));
        sensor.AddObservation(endTransform.position.z / (mazeGenerator.height * 3));

        // Direction to end
        Vector3 dirToEnd = (endTransform.position - transform.position).normalized;
        sensor.AddObservation(dirToEnd.x);
        sensor.AddObservation(dirToEnd.z);

        // Distance to end (normalized)
        sensor.AddObservation(Vector3.Distance(transform.position, endTransform.position)
            / (mazeGenerator.width * mazeGenerator.height * 3));
    }

    // Act on decisions from the neural network
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Don't accept new actions mid-move
        if (isMoving) return;

        int action = actions.DiscreteActions[0];

        Vector3 direction = action switch
        {
            0 => Vector3.forward,
            1 => Vector3.back,
            2 => Vector3.left,
            3 => Vector3.right,
            _ => Vector3.zero
        };

        if (direction == Vector3.zero)
        {
            AddReward(-0.001f); // discourage idling
            return;
        }

        Vector3 desiredTarget = transform.position + direction * 3f;

        // Check if target cell is a wall before moving
        if (IsWall(desiredTarget))
        {
            AddReward(-0.5f); // penalty for hitting walls
            return;
        }

        StartCoroutine(MoveToCell(desiredTarget));

    }
    IEnumerator MoveToCell(Vector3 target)
    {
        isMoving = true;
        Vector2 targetXZ = new Vector2(target.x, target.z);

        while (Vector2.Distance( new Vector2(transform.position.x, transform.position.z), targetXZ) > 0.01f)
        {
            Vector3 move = (target - transform.position).normalized 
                * moveSpeed * Time.deltaTime;

            // Clamp so we don't overshoot
            if (move.magnitude > Vector3.Distance(transform.position, target))
                move = target - transform.position;

            controller.Move(move + Physics.gravity * Time.deltaTime);
            yield return null;
        }

        // Snap exactly to cell center to avoid floating point drift
        controller.enabled = false;
        transform.position = target;
        controller.enabled = true;

        isMoving = false;

        // Reward shaping after completing a move
        float distanceNow = Vector3.Distance(transform.position, endTransform.position);
        float progress = distanceToPreviousStep - distanceNow;
        AddReward(progress * 0.01f);
        distanceToPreviousStep = distanceNow;

        // AddReward(-0.001f); // time penalty per step
    }
    bool IsWall(Vector3 worldPos)
    {
        Vector3 localPos = mazeGenerator.transform.InverseTransformPoint(worldPos);
        // Convert world position to grid coordinates
        int gridX = Mathf.RoundToInt(localPos.x / 3);
        int gridY = Mathf.RoundToInt(localPos.z / 3);

        Vector2Int currentCell = new Vector2Int(gridX, gridY);
        if(visitedCells.Contains(currentCell))
        {
            AddReward(-0.07f);
        }
        else
        {
            AddReward(1f);
            visitedCells.Add(currentCell);
        }

        // Out of bounds = treat as wall
        if (gridX < 0 || gridX >= mazeGenerator.width ||
            gridY < 0 || gridY >= mazeGenerator.height)
            return true;

        return mazeGenerator.grid[gridX, gridY] == 0;
    }

    // Manual control for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
    var discrete = actionsOut.DiscreteActions;

    if (Input.GetKeyDown(KeyCode.W)) discrete[0] = 0;
    if (Input.GetKeyDown(KeyCode.S)) discrete[0] = 1;
    if (Input.GetKeyDown(KeyCode.A)) discrete[0] = 2;
    if (Input.GetKeyDown(KeyCode.D)) discrete[0] = 3;
}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("End"))
        {
            Debug.Log("Completed Maze");
            AddReward(10f);       // big reward for finishing
            EndEpisode();
        }

        if (other.CompareTag("Trap"))
        {
            // EndEpisode();         // or just penalize without ending
        }
        if (other.CompareTag("Teleporter"))
        {
            AddReward(0.1f);
        }
    }
}