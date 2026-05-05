using UnityEngine;

public class SlowTrap : MonoBehaviour
{
    public float slowDebuff = -2.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().walkSpeed += slowDebuff;
        }
        if(other.CompareTag("Agent"))
        {
            other.GetComponent<MazeAgent>().ApplyMovementModifier(-slowDebuff);
        }
        
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().walkSpeed -= slowDebuff;
        }
        if(other.CompareTag("Agent"))
        {
            other.GetComponent<MazeAgent>().ApplyMovementModifier(slowDebuff);
        }
        
    }
}
