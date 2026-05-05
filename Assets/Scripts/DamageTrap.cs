using UnityEngine;

public class DamageTrap : MonoBehaviour
{
    public int damage;


    void Start()
    {
        // GetComponent<MeshRenderer>().enabled = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //activate the trap logic
            other.gameObject.GetComponent<PlayerStats>().TakeDamage(damage);
        }
        if(other.CompareTag("Agent"))
        {
            //activate the trap logic
            other.gameObject.GetComponent<MazeAgent>().TakeDamage(damage);
        }
        
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //logic for deactivating the trap
        }
        
    }
}
