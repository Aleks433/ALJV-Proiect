using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public MazeGenerator maze;
    public static GameManager instance;


    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //player.transform.position = maze.startPosition.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
