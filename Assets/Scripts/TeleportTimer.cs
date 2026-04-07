using UnityEngine;

public class TeleportTimer : MonoBehaviour
{
    public float timer;
    public bool triggered;
    public GameObject teleportA;
    public GameObject teleportB;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0;
        triggered = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 0)
        {
            triggered = false;
            return;
        }
        timer -= Time.deltaTime;
        
    }
    public void StartTimer()
    {
        timer = 5.0f;
        triggered = true;
    }
}
