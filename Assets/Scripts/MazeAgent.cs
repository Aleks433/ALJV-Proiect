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
    [SerializeField]
    public HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private bool completedLastMaze;



    void Start()
    {
        completedLastMaze = false;
        mazeGenerator.Generate();
    }
    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        MaxStep = 5000;
        mazeGenerator.Generate();
    }

    public void TakeDamage(int damage)
    {
       currentHealth -= damage; 
       AddReward(-((float)damage / maxHealth) * 0.5f);

       if(currentHealth <= 0)
        {
            Debug.Log("Agent Died");
            AddReward(-5f);
            EndEpisode();
        }
    }
    public void ApplyMovementModifier(float movement)
    {
        moveSpeed = movement;
        AddReward(-0.01f);
    }

    public void TeleportAgent(Transform where)
    {
        StopAllCoroutines();
        isMoving = true;
        controller.enabled = false;
        transform.SetPositionAndRotation(new Vector3(where.position.x, where.position.y, where.position.z), Quaternion.identity);
        controller.enabled = true;
        isMoving = false;
        RequestDecision();

    }
    // Called at the start of every training episode
    public override void OnEpisodeBegin()
    {
        StopAllCoroutines();

        if(!completedLastMaze)
        {
            Debug.Log("Agent out of moves");
            MaxStep += 10;
        }
        else
        {
            // Generate a fresh maze
            mazeGenerator.Generate();
        }
        completedLastMaze = false;
        visitedCells.Clear();

        currentHealth = maxHealth;
        moveSpeed = baseMoveSpeed;

        isMoving = false;

        // Move agent to start
        controller.enabled = false;
        transform.position = mazeGenerator.startPosition.position;
        transform.rotation = Quaternion.identity;
        controller.enabled = true;

        // Update end reference
        endTransform = mazeGenerator.endPosition;

        distanceToPreviousStep = Vector3.Distance(transform.position, endTransform.position);
        RequestDecision();
    }

    // Define what the agent observes
    public override void CollectObservations(VectorSensor sensor)
    {
        // Position (2)
        sensor.AddObservation(transform.localPosition.x / (mazeGenerator.width * 3));
        sensor.AddObservation(transform.localPosition.z / (mazeGenerator.height * 3));
    
        // End position (2)
        sensor.AddObservation(endTransform.localPosition.x / (mazeGenerator.width * 3));
        sensor.AddObservation(endTransform.localPosition.z / (mazeGenerator.height * 3));
    
        // Direction to end (2)
        Vector3 dirToEnd = (endTransform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(dirToEnd.x);
        sensor.AddObservation(dirToEnd.z);
    
        // Distance to end (1)
        sensor.AddObservation(Vector3.Distance(transform.localPosition, endTransform.localPosition)
            / (mazeGenerator.width * mazeGenerator.height * 3));
    
        // Health (1)
        sensor.AddObservation(currentHealth / maxHealth);
    
        // Visited cell ratio (1)
        int totalCells = (mazeGenerator.width * mazeGenerator.height) / 2;
        sensor.AddObservation((float)visitedCells.Count / totalCells);
    
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(0, 0, !IsWall(transform.position + Vector3.forward * 3));
        actionMask.SetActionEnabled(0, 1, !IsWall(transform.position + Vector3.back * 3));
        actionMask.SetActionEnabled(0, 2, !IsWall(transform.position + Vector3.left * 3));
        actionMask.SetActionEnabled(0, 3, !IsWall(transform.position + Vector3.right * 3));
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


        Vector3 desiredTarget = transform.position + direction * 3f;
        CheckVisited(desiredTarget);

        // Check if target cell is a wall before moving
        if (IsWall(desiredTarget))
        {
            Debug.Log("Agent hit a Wall");
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

        if (StepCount >= MaxStep - 1)
        {
            AddReward(-5f);
        }

        // Reward shaping after completing a move
        float distanceNow = Vector3.Distance(transform.position, endTransform.position);
        float progress = distanceToPreviousStep - distanceNow;
        AddReward(progress * 0.1f);
        distanceToPreviousStep = distanceNow;

        AddReward(-0.1f); // time penalty per step
        RequestDecision();
    }
    void CheckVisited(Vector3 worldPos)
    {
        Vector3 localPos = mazeGenerator.transform.InverseTransformPoint(worldPos);
        // Convert world position to grid coordinates
        int gridX = Mathf.RoundToInt(localPos.x / 3);
        int gridY = Mathf.RoundToInt(localPos.z / 3);

        Vector2Int currentCell = new Vector2Int(gridX, gridY);
        if(visitedCells.Contains(currentCell))
        {
            
            AddReward(-0.7f);
        }
        else
        {
            AddReward(0.7f);
            visitedCells.Add(currentCell);
        }

    }
    bool IsWall(Vector3 worldPos)
    {
        Vector3 localPos = mazeGenerator.transform.InverseTransformPoint(worldPos);
        // Convert world position to grid coordinates
        int gridX = Mathf.RoundToInt(localPos.x / 3);
        int gridY = Mathf.RoundToInt(localPos.z / 3);

        Vector2Int currentCell = new Vector2Int(gridX, gridY);

        // Out of bounds = treat as wall
        if (gridX < 0 || gridX >= mazeGenerator.width ||
            gridY < 0 || gridY >= mazeGenerator.height)
            return true;

        
        return mazeGenerator.grid[gridX, gridY] == 0 && mazeGenerator.trapGrid[gridX, gridY] != 1;
    }

    // Manual control for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
    var discrete = actionsOut.DiscreteActions;

    if (Input.GetKey(KeyCode.W)) discrete[0] = 0;
    if (Input.GetKey(KeyCode.S)) discrete[0] = 1;
    if (Input.GetKey(KeyCode.A)) discrete[0] = 2;
    if (Input.GetKey(KeyCode.D)) discrete[0] = 3;
}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("End"))
        {
            Debug.Log("Completed Maze in " + StepCount.ToString());
            completedLastMaze = true;
            float efficiency  = 1f - ((float)StepCount / MaxStep);
            AddReward(10f + efficiency);
            MaxStep -= 100;
            EndEpisode();
        }

        if (other.CompareTag("Trap"))
        {
            // EndEpisode();         // or just penalize without ending
            Debug.Log("Agent went through a trap");
        }
        if (other.CompareTag("Teleporter"))
        {
            Debug.Log("Agent went through a teleporter");
            AddReward(0.1f);
        }
    }
}