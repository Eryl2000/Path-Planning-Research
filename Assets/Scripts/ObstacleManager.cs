using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject ObstacleShipPrefab;
    public int numObstacles = 10;

    List<GameObject> obstacles = new List<GameObject>();

    public void GenerateObstacles(float boardWidth, float boardHeight)
    {
        ResetObstacles();
        for (int i = 0; i < numObstacles; ++i)
        {
            Vector3 pos = new Vector3(Random.Range(-boardWidth / 2.0f, boardWidth / 2.0f), 0.0f, Random.Range(-boardHeight / 2.0f, boardHeight / 2.0f));
            CreateObstacle(pos);
        }
    }

    public void CreateObstacle(Vector3 pos)
    {
        GameObject cur = Instantiate(ObstacleShipPrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform);
        cur.gameObject.layer = LayerMask.NameToLayer("Obstacles");
        obstacles.Add(cur);
    }

    private void ResetObstacles()
    {
        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
        obstacles.Clear();
    }
}
