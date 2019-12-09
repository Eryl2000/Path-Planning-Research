using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public ObstacleManager obstacleManager;
    public Actor actor;

    public GameObject GroundPlane;

    public GameObject StartSprite;
    public GameObject EndSprite;
    public GameObject LoadingText;

    private enum State { None, RegenerateObstacles, CalculatePath, WaitForPath, ShowPath };
    State state;
    bool startValid;
    bool endValid;
    RRT rrt;
    float boardWidth;
    float boardHeight;

    void Start()
    {
        boardWidth = GroundPlane.transform.localScale.x * 10.0f;
        boardHeight = GroundPlane.transform.localScale.z * 10.0f;
        startValid = endValid = false;
        state = State.RegenerateObstacles;
    }

    void ResetWidgets()
    {
        startValid = endValid = false;
        actor.SetInvisible();
        actor.waypoints.Clear();
        actor.ClearLines();
        StartSprite.transform.position = EndSprite.transform.position = new Vector3(10000.0f, 0.0f, 0.0f);
    }

    void Update()
    {
        switch (state)
        {
            case State.CalculatePath:
                actor.ClearLines();
                rrt = new RRT(StartSprite.transform.position, EndSprite.transform.position, actor, 2500, boardWidth, boardHeight);
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
                obstacleManager.GenerateObstacles(boardWidth, boardHeight);
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
            ResetWidgets();
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
                    ResetWidgets();
                    obstacleManager.CreateObstacle(hit.point);
                    state = State.None;
                }
                else
                {
                    actor.transform.position = hit.point;
                    if (actor.HitsObstacle())
                    {
                        actor.SetInvisible();
                        StartSprite.transform.position = new Vector3(10000.0f, 0.0f, 0.0f);
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
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                actor.transform.position = hit.point;
                if (actor.HitsObstacle())
                {
                    actor.SetInvisible();
                    EndSprite.transform.position = new Vector3(10000.0f, 0.0f, 0.0f);
                }
                else
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
}
