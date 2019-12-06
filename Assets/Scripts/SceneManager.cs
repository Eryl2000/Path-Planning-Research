using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public ObstacleManager obstacleManager;
    public Actor actor;

    public float BoardWidth;
    public float BoardHeight;

    public GameObject StartSprite;
    public GameObject EndSprite;
    public GameObject LoadingText;

    private enum State { None, RegenerateObstacles, CalculatePath, WaitForPath, ShowPath };
    State state;
    bool startValid;
    bool endValid;
    RRT rrt;

    void Start()
    {
        startValid = endValid = false;
        state = State.RegenerateObstacles;
    }

    void Update()
    {
        switch (state)
        {
            case State.CalculatePath:
                actor.ClearLines();
                rrt = new RRT(StartSprite.transform.position, EndSprite.transform.position, actor, 250, BoardWidth, BoardHeight);
                state = State.WaitForPath;
                break;
            case State.WaitForPath:
                rrt.NextStep();
                if (rrt.Finished)
                {
                    state = State.ShowPath;
                }
                break;
            case State.ShowPath:
                actor.waypoints.Clear();
                actor.transform.position = StartSprite.transform.position;
                if (rrt.Successful)
                {
                    List<Vector3> path = rrt.GetPath();
                    foreach (Vector3 pos in path)
                    {
                        actor.waypoints.Add(new Actor.Waypoint(pos, Quaternion.Euler(0, 0, 0)));
                    }
                }
                state = State.None;
                break;
            case State.RegenerateObstacles:
                obstacleManager.ResetObstacles();
                obstacleManager.CreateObstacles(BoardWidth, BoardHeight);
                state = State.None;
                break;
            case State.None:
            default:
                LoadingText.SetActive(false);
                break;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            startValid = endValid = false;
            actor.SetInvisible();
            actor.waypoints.Clear();
            StartSprite.transform.position = EndSprite.transform.position = new Vector3(10000.0f, 0.0f, 0.0f);
            state = State.RegenerateObstacles;
            LoadingText.SetActive(true);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    startValid = endValid = false;
                    actor.SetInvisible();
                    actor.waypoints.Clear();
                    obstacleManager.CreateObstacle(hit.point);
                }
                else
                {
                    StartSprite.transform.position = hit.point;
                    startValid = true;
                    if (startValid && endValid)
                    {
                        state = State.CalculatePath;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                EndSprite.transform.position = hit.point;
                endValid = true;
                if (startValid && endValid)
                {
                    state = State.CalculatePath;
                }
            }
        }

    }
}
