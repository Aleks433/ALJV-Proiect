using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MazeGenerator : MonoBehaviour
{
    public int width = 21, height = 21; // Use odd numbers
    [SerializeField]
    public int[,] grid;
    public int[,] trapGrid;
    private Stack<Vector2Int> stack = new();

    public GameObject wallPrefab;
    public List<GameObject> trapPrefabs;
    public int trapCount = 10;
    public GameObject teleporterPrefab;
    public int teleporterPairCount = 3;
    public GameObject startPrefab, endPrefab;
    public Transform startPosition;
    public Transform endPosition;
    public bool randomSeed;
    public int seed;

    public void ClearMaze()
    {
        stack.Clear();
        // Destroy all children of this GameObject
        for (int i = transform.childCount - 1; i >= 0; i--)
            if(Application.isPlaying) {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

    }
    public void Generate()
    {
        ClearMaze();
        GenerateMaze();
    }
    void PlaceStartAndEnd()
    {
        // Start is always (1,1) — where DFS begins
        grid[1, 1] = 1;
        Vector3 startPos = new Vector3(1 * 3, 1.5f, 1 * 3);
        startPosition = Instantiate(startPrefab, transform.TransformPoint(startPos), Quaternion.identity, transform).transform;

        // End is bottom-right corner — always (width-2, height-2)
        grid[width - 2, height - 2] = 1;
        Vector3 endPos= new Vector3((width - 2) * 3, 1.5f, (height - 2) * 3);
        endPosition = Instantiate(endPrefab, transform.TransformPoint(endPos), Quaternion.identity, transform).transform;

    }
    void GenerateMaze()
    {
        if(!randomSeed)
        {
            Random.InitState(seed);
        } 
        grid = new int[width, height]; // 0 = wall, 1 = path
        trapGrid = new int[width, height];

        // Fill with walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++) 
            {
                grid[x, y] = 0;
                trapGrid[x, y] = 0;
            }

        Vector2Int start = new Vector2Int(1, 1);
        grid[start.x, start.y] = 1;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                // Carve passage between current and chosen
                Vector2Int between = current + (chosen - current) / 2;
                grid[between.x, between.y] = 1;
                grid[chosen.x, chosen.y] = 1;
                stack.Push(chosen);
            }
            else stack.Pop();
        }

        BuildMaze();
        PlaceStartAndEnd();
        PlaceTraps();
        PlaceTeleporters();
    }


    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        var dirs = new[] { Vector2Int.up * 2, Vector2Int.down * 2,
                           Vector2Int.left * 2, Vector2Int.right * 2 };
        List<Vector2Int> result = new();
        foreach (var d in dirs)
        {
            Vector2Int neighbor = cell + d;
            if (neighbor.x > 0 && neighbor.x < width - 1 &&
                neighbor.y > 0 && neighbor.y < height - 1 &&
                grid[neighbor.x, neighbor.y] == 0)
                result.Add(neighbor);
        }
        return result;
    }
    void BuildMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * 3, 1.5f, y * 3);
                if (grid[x, y] == 0)
                    Instantiate(wallPrefab, transform.TransformPoint(pos), Quaternion.identity, transform);
            }
        }
    }
    void PlaceTraps()
    {
        List<Vector2Int> openCells = GetOpenCells();
        openCells = openCells.OrderBy(_ => Random.value).ToList();

        for (int i = 0; i < Mathf.Min(trapCount, openCells.Count); i++)
        {
            grid[openCells[i].x, openCells[i].y] = 0;
            trapGrid[openCells[i].x, openCells[i].y] = 1;
            Vector3 pos = new Vector3(openCells[i].x * 3, 1.5f, openCells[i].y * 3);

            GameObject trapPrefab = trapPrefabs[Random.Range(0, trapPrefabs.Count)];
            Instantiate(trapPrefab, transform.TransformPoint(pos), Quaternion.identity, transform);
        }
    }
    void PlaceTeleporters()
    {
        List<Vector2Int> openCells = GetOpenCells()
            .OrderBy(_ => Random.value).ToList();

        int index = 0;
        for (int i = 0; i < teleporterPairCount && index + 1 < openCells.Count; i++)
        {
            TeleportTimer teleporter = Instantiate(teleporterPrefab, transform).GetComponent<TeleportTimer>();
            Vector3 posA = new Vector3(openCells[index].x * 3, 1.5f, openCells[index].y * 3);
            Vector3 posB = new Vector3(openCells[index + 1].x * 3, 1.5f, openCells[index + 1].y * 3);
            trapGrid[openCells[index].x, openCells[index].y] =  1;
            trapGrid[openCells[index + 1].x, openCells[index + 1].y] =  1;

            teleporter.teleportA.transform.position = transform.TransformPoint(posA);
            teleporter.teleportB.transform.position = transform.TransformPoint(posB);


            index += 2;
        }
    }
    List<Vector2Int> GetOpenCells()
    {
        List<Vector2Int> open = new();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (grid[x, y] == 1)
                    open.Add(new Vector2Int(x, y));
        return open;
    }
}
