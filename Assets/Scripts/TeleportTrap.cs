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
        toTeleport.GetComponent<CharacterController>().enabled = false;
        toTeleport.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.z), quaternion.identity);
        toTeleport.GetComponent<CharacterController>().enabled = true;
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(!timer.triggered)
            {
                teleportLocation.TriggerTrap(other.gameObject);
            }
        }
    }
}
