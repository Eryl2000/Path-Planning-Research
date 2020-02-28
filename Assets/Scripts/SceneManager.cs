using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    private static SceneManager _instance;
    public static SceneManager Instance { get { return _instance; } }

    //public Actor MainActor;
    public GameObject GroundPlane;
    //public GameObject StartSprite;
    //public GameObject EndSprite;
    public GameObject LoadingText;
    public float SelectActorDistance;

    //private enum SceneState { None, RegenerateScenario, CalculatePath, WaitForPath, ShowPath };
    //SceneState sceneState;
    //bool startValid;
    //bool endValid;
    //State startState;
    //State endState;
    Actor selectedActor;

    //RRT rrt;
    public float BoardX;
    public float BoardZ;

    private void Awake()
    {
        //Enforce singleton
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
        BoardX = GroundPlane.transform.localScale.x * 10.0f;
        BoardZ = GroundPlane.transform.localScale.z * 10.0f;
        //startValid = endValid = false;
        //sceneState = SceneState.RegenerateScenario;
        selectedActor = null;

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

    /*void ResetWidgets()
    {
        startValid = endValid = false;
        MainActor.ClearWaypoints();
        MainActor.SetInvisible();
        MainActor.ClearLines();
        StartSprite.transform.position = EndSprite.transform.position = new Vector3(10000.0f, 0.0f, 0.0f);
    }*/

    void Update()
    {
        LoadingText.SetActive(false);
        /*switch (sceneState)
        {
            case SceneState.CalculatePath:
                MainActor.ClearWaypoints();
                MainActor.SetInvisible();
                MainActor.ClearLines();
                rrt = new RRT(startState, endState, MainActor, 50000, boardX, boardZ);
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
                MainActor.ClearWaypoints();
                MainActor.transform.position = StartSprite.transform.position;
                if (rrt.Successful)
                {
                    List<State> path = rrt.GetPath();
                    foreach (State state in path)
                    {
                        MainActor.AppendWaypoint(state);
                    }
                    MainActor.CurState = path.ElementAt(0);
                }
                rrt = null;
                sceneState = SceneState.None;
                break;
            case SceneState.RegenerateScenario:
                ScenarioManager.Instance.Scenario = ScenarioManager.ScenarioType.Random;
                sceneState = SceneState.None;
                break;
            case SceneState.None:
            default:
                LoadingText.SetActive(false);
                break;
        }*/

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            //ResetWidgets();
            //sceneState = SceneState.RegenerateScenario;
            LoadingText.SetActive(true);
            ScenarioManager.Instance.Scenario = ScenarioManager.Instance.Scenario;
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                if (selectedActor != null)
                {
                    selectedActor.Selected = false;
                    selectedActor = null;
                }
                Actor clicked = GetClickedShip(hit.point);
                if (clicked != null)
                {
                    clicked.Selected = true;
                    selectedActor = clicked;
                }
            }
            /*else if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                if (selectedActor != null)
                {
                    selectedActor.Selected = false;
                    selectedActor = null;
                }
                State possibleState = new State(hit.point, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), Random.value, 0.0f);
                if (!MainActor.WouldHitObject(possibleState))
                {
                    startState = possibleState;
                    StartSprite.transform.position = possibleState.position;
                    startValid = true;
                    if (startValid && endValid)
                    {
                        sceneState = SceneState.CalculatePath;
                    }
                }
            }*/
        }
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                if (selectedActor != null)
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        selectedActor.AppendWaypoint(new State(hit.point, selectedActor.CurState.rotation, selectedActor.CurState.velocity));
                    }
                    else
                    {
                        selectedActor.ClearWaypoints();
                        selectedActor.AppendWaypoint(new State(hit.point, selectedActor.CurState.rotation, selectedActor.CurState.velocity));
                    }
                }

                /*State possibleState = new State(hit.point, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), Random.value, MainActor.ShipData.CruiseSpeed);
                if (!MainActor.WouldHitObject(possibleState))
                {
                    endState = possibleState;
                    EndSprite.transform.position = possibleState.position;
                    endValid = true;
                    if (startValid && endValid)
                    {
                        sceneState = SceneState.CalculatePath;
                    }
                }*/
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                ScenarioManager.Instance.CreateObject(hit.point, Quaternion.identity, ScenarioManager.ObjectType.Static, null);
            }
        }
    }


    public static void DrawLine(Vector3 start, Vector3 end, Color color, Transform parent, float duration = -1.0f, float thickness = 5.0f, int z_index = 0)
    {
        start.y += 0.1f;
        end.y += 0.1f;
        GameObject myLine = new GameObject("Line");
        myLine.transform.parent = parent;
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("UI/Unlit/Transparent"));
        lr.material.color = color;
        lr.startWidth = lr.endWidth = thickness;
        lr.SetPosition(0, start + lr.transform.up * z_index);
        lr.SetPosition(1, end + lr.transform.up * z_index);
        if (duration >= 0.0f)
        {
            GameObject.Destroy(myLine, duration);
        }
    }

    public static void DrawCircle(Vector3 center, float radius, int numPoints, Color color, Transform parent, float duration = -1.0f, float thickness = 5.0f, int z_index = 0)
    {
        GameObject myLine = new GameObject("Line");
        myLine.transform.parent = parent;
        myLine.transform.position = center;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("UI/Unlit/Transparent"));
        lr.material.color = color;
        lr.positionCount = numPoints + 1;
        lr.startWidth = lr.endWidth = thickness;

        float angle = 0f;
        for (int i = 0; i < numPoints + 1; i++)
        {
            float x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = center.z + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            lr.SetPosition(i, new Vector3(x, center.y + z_index, z));
            angle += 360f / numPoints;
        }

        if (duration >= 0.0f)
        {
            GameObject.Destroy(myLine, duration);
        }
    }

    private Actor GetClickedShip(Vector3 hitLocation)
    {
        float minDist = Mathf.Infinity;
        Actor closest = null;
        foreach (GameObject ga in ScenarioManager.Instance.DynamicObjects)
        {
            Actor actor = ga.GetComponent<Actor>();
            if (actor != null)
            {
                float dist = Vector3.Distance(hitLocation, actor.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = actor;
                }
            }
        }
        if (minDist <= SelectActorDistance)
        {
            return closest;
        }
        return null;
    }
}
