using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private static ObstacleManager _instance = null;
    public static ObstacleManager Instance { get { return _instance; } }

    public GameObject StaticObstaclePrefab;
    public Actor DynamicObstaclePrefab;
    public int numStaticObstacles = 10;
    public int numDynamicObstacles = 0;

    public List<GameObject> Obstacles = new List<GameObject>();

    public enum ObstacleType { Static, Dynamic };

    float boardWidth = 0.0f;
    float boardHeight = 0.0f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void GenerateObstacles(float _boardWidth, float _boardHeight)
    {
        boardWidth = _boardWidth;
        boardHeight = _boardHeight;

        ResetObstacles();
        for (int i = 0; i < numStaticObstacles; ++i)
        {
            Vector3 pos = new Vector3(Random.Range(-boardWidth / 2.0f, boardWidth / 2.0f), 0.0f, Random.Range(-boardHeight / 2.0f, boardHeight / 2.0f));
            CreateObstacle(pos, ObstacleType.Static);
        }
        for (int i = 0; i < numDynamicObstacles; ++i)
        {
            Vector3 pos = new Vector3(Random.Range(-boardWidth / 2.0f, boardWidth / 2.0f), 0.0f, Random.Range(-boardHeight / 2.0f, boardHeight / 2.0f));
            CreateObstacle(pos, ObstacleType.Dynamic);
        }
    }

    public void CreateObstacle(Vector3 pos, ObstacleType type)
    {
        if (type == ObstacleType.Static)
        {
            GameObject cur = Instantiate(StaticObstaclePrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform);
            cur.gameObject.layer = LayerMask.NameToLayer("Obstacles");
            Obstacles.Add(cur);
        }
        else if (type == ObstacleType.Dynamic)
        {
            Actor cur = Instantiate(DynamicObstaclePrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform);
            cur.SetCurState(new State(pos, cur.transform.rotation, Vector3.zero));
            for(int i = 0; i < 4; ++i)
            {
                Vector3 target = new Vector3(Random.Range(-boardWidth / 2.0f, boardWidth / 2.0f), 0.0f, Random.Range(-boardHeight / 2.0f, boardHeight / 2.0f));
                cur.AddWaypoint(new State(target, new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), 0.0f, cur.CruiseSpeed));
            }
            cur.gameObject.layer = LayerMask.NameToLayer("Obstacles");
            Obstacles.Add(cur.gameObject);
        }
    }

    private void ResetObstacles()
    {
        foreach (GameObject obstacle in Obstacles)
        {
            Destroy(obstacle);
        }
        Obstacles.Clear();
    }
}
