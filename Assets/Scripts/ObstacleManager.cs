using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    //public GameObject ObstacleCubePrefab;
    //public GameObject ObstacleCylinderPrefab;
    public Obstacle ObstacleShipPrefab;
    public int numObstacles = 1;

    public List<Obstacle> Obstacles { get => obstacles; }
    List<Obstacle> obstacles;

    private void Awake()
    {
        obstacles = new List<Obstacle>();
    }

    public void ResetObstacles()
    {
        foreach (Obstacle obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }
        obstacles.Clear();
    }

    public void CreateObstacles(float boardWidth, float boardHeight)
    {
        for (int i = 0; i < numObstacles; ++i)
        {
            Vector3 pos = new Vector3(Random.Range(-boardWidth / 2.0f, boardWidth / 2.0f), 0.0f, Random.Range(-boardHeight / 2.0f, boardHeight / 2.0f));
            CreateObstacle(pos);
        }
    }

    public void CreateObstacle(Vector3 pos)
    {
        /*GameObject cur;
        if (Random.value < 0.5f)
        {
            cur = Instantiate(ObstacleCubePrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform);
        }
        else
        {
            cur = Instantiate(ObstacleCylinderPrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform);
        }
        cur.transform.localScale = new Vector3(Random.Range(5.0f, 30.0f), cur.transform.localScale.y, Random.Range(5.0f, 30.0f));
        cur.layer = LayerMask.NameToLayer("Obstacles");
        obstacles.Add(cur);*/

        Obstacle cur = Instantiate(ObstacleShipPrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform) as Obstacle;
        cur.gameObject.layer = LayerMask.NameToLayer("Obstacles");
        obstacles.Add(cur);
    }
}
