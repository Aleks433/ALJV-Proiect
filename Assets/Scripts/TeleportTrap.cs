using Unity.Mathematics;
using UnityEngine;

public class TeleportTrap : MonoBehaviour
{
    public TeleportTrap teleportLocation;
    public TeleportTimer timer;
    void Start()
    {
        // GetComponent<MeshRenderer>().enabled = false;
    }

    public void TriggerTrap(GameObject toTeleport)
    {
        timer.StartTimer();
        toTeleport.GetComponent<MazeAgent>().TeleportAgent(teleportLocation.transform);
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Agent"))
        {
            if(!timer.triggered)
            {
                TriggerTrap(other.gameObject);
            }
        }
    }
}
