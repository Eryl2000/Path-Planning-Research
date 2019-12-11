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

    private enum SceneState { None, RegenerateObstacles, CalculatePath, WaitForPath, ShowPath };
    SceneState sceneState;
    bool startValid;
    bool endValid;
    State startState;
    State endState;

    RRT rrt;
    float boardWidth;
    float boardHeight;

    void Start()
    {
        boardWidth = GroundPlane.transform.localScale.x * 10.0f;
        boardHeight = GroundPlane.transform.localScale.z * 10.0f;
        startValid = endValid = false;
        sceneState = SceneState.RegenerateObstacles;
    }

    void ResetWidgets()
    {
        startValid = endValid = false;
        actor.SetInvisible();
        actor.ClearWaypoints();
        actor.ClearLines();
        StartSprite.transform.position = EndSprite.transform.position = new Vector3(10000.0f, 0.0f, 0.0f);
    }

    void Update()
    {
        switch (sceneState)
        {
            case SceneState.CalculatePath:
                actor.ClearLines();
                rrt = new RRT(startState, endState, actor, 5000, boardWidth, boardHeight);
                sceneState = SceneState.WaitForPath;
                break;
            case SceneState.WaitForPath:
                rrt.NextStep();
                if (rrt.Finished)
                {
                    sceneState = SceneState.ShowPath;
                }
                break;
            case SceneState.ShowPath:
                actor.ClearWaypoints();
                actor.transform.position = StartSprite.transform.position;
                if (rrt.Successful)
                {
                    List<State> path = rrt.GetPath();
                    foreach (State state in path)
                    {
                        actor.AddWaypoint(state);
                    }
                }
                sceneState = SceneState.None;
                break;
            case SceneState.RegenerateObstacles:
                obstacleManager.GenerateObstacles(boardWidth, boardHeight);
                sceneState = SceneState.None;
                break;
            case SceneState.None:
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
            sceneState = SceneState.RegenerateObstacles;
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
                    sceneState = SceneState.None;
                }
                else
                {
                    State possibleState = new State(hit.point, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), Vector3.zero);
                    if (!actor.HitsObstacle(possibleState))
                    {
                        startState = possibleState;
                        StartSprite.transform.position = possibleState.position;
                        startValid = true;
                        if (startValid && endValid)
                        {
                            sceneState = SceneState.CalculatePath;
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
                State possibleState = new State(hit.point, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), Vector3.zero);
                if (!actor.HitsObstacle(possibleState))
                {
                    endState = possibleState;
                    EndSprite.transform.position = possibleState.position;
                    endValid = true;
                    if (startValid && endValid)
                    {
                        sceneState = SceneState.CalculatePath;
                    }
                }
            }
        }

    }
}
