using UnityEngine;

public class WallTrap : MonoBehaviour
{
    public GameObject wall;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wall.SetActive(false);
        GetComponent<MeshRenderer>().enabled = false;
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            wall.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            wall.SetActive(false);
        }
    }
}
