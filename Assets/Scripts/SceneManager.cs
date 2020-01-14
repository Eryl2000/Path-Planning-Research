using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    private static SceneManager _instance;
    public static SceneManager Instance { get { return _instance; } }

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

    void Start()
    {
        boardWidth = GroundPlane.transform.localScale.x * 10.0f;
        boardHeight = GroundPlane.transform.localScale.z * 10.0f;
        startValid = endValid = false;
        sceneState = SceneState.RegenerateObstacles;

        /*int numPoints = 100;
        for(int i = 0; i < numPoints; ++i)
        {
            for(int j = 0; j < numPoints; ++j)
            {
                Vector3 pos = new Vector3(0.5f * boardWidth * ((float)i / (numPoints - 1) - 0.5f), 0.0f, 0.5f * boardHeight * ((float)j / (numPoints - 1) - 0.5f));
                float closeness = actor.ClosenessMeasure(new State(Vector3.zero, new Vector3(0, 225.0f, 0), Vector3.zero), new State(pos, new Vector3(0, 0.0f, 0), Vector3.zero));
                float t = Mathf.InverseLerp(0, 200, closeness);
                Debug.DrawLine(pos - 4f * Vector3.forward, pos + 4f * Vector3.forward, Color.Lerp(Color.red, Color.blue, t), 1000.0f, false);
                Debug.Log(closeness);
            }
        }*/
    }

    void ResetWidgets()
    {
        startValid = endValid = false;
        actor.ClearWaypoints();
        actor.SetInvisible();
        actor.ClearLines();
        StartSprite.transform.position = EndSprite.transform.position = new Vector3(10000.0f, 0.0f, 0.0f);
    }

    void Update()
    {
        switch (sceneState)
        {
            case SceneState.CalculatePath:
                actor.ClearWaypoints();
                actor.SetInvisible();
                actor.ClearLines();
                rrt = new RRT(startState, endState, actor, 50000, boardWidth, boardHeight);
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
                    actor.SetCurState(path.ElementAt(0));
                }
                rrt = null;
                sceneState = SceneState.None;
                break;
            case SceneState.RegenerateObstacles:
                ObstacleManager.Instance.GenerateObstacles(boardWidth, boardHeight);
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
                    ObstacleManager.Instance.CreateObstacle(hit.point, ObstacleManager.ObstacleType.Static);
                    sceneState = SceneState.None;
                }
                else
                {
                    State possibleState = new State(hit.point, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), Random.value, 0.0f);
                    if (!actor.WouldHitObstacle(possibleState))
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
                State possibleState = new State(hit.point, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), Random.value, actor.CruiseSpeed);
                if (!actor.WouldHitObstacle(possibleState))
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
