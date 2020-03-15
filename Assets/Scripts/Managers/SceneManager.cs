using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    private static SceneManager _instance;
    public static SceneManager Instance { get { return _instance; } }

    public GameObject GroundPlane;
    public GameObject LoadingText;
    public float SelectActorDistance;
    public float BoardX;
    public float BoardZ;

    public Toggle ShowWaypointToggle;
    public Dropdown ScenarioSelectorDropdown;
    public Dropdown AISelectorDropdown;
    public GameObject SelectedActorPanel;

    private Actor selectedActor;
    public Actor SelectedActor {
        get {
            return selectedActor;
        }
        set {
            if (selectedActor != null)
            {
                selectedActor.Selected = false;
            }
            if (value == null)
            {
                selectedActor = null;
                SelectedActorPanel.gameObject.SetActive(false);
            }
            else
            {
                selectedActor = value;
                selectedActor.Selected = true;
                SelectedActorPanel.gameObject.SetActive(true);
                AISelectorDropdown.value = AISelectorDropdown.options.FindIndex(option => option.text == selectedActor.AI.GetName());
            }
        }
    }

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
        SelectedActor = null;

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

    void Update()
    {
        LoadingText.SetActive(false);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            LoadingText.SetActive(true);
            ScenarioManager.Instance.Scenario = ScenarioManager.Instance.Scenario;
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                SelectedActor = GetClickedShip(hit.point);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GroundPlane")))
            {
                if (SelectedActor != null)
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        SelectedActor.AppendWaypoint(new State(hit.point, SelectedActor.CurState.rotation, SelectedActor.CurState.velocity));
                    }
                    else
                    {
                        SelectedActor.ClearWaypoints();
                        SelectedActor.AppendWaypoint(new State(hit.point, SelectedActor.CurState.rotation, SelectedActor.CurState.velocity));
                    }
                }
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

    public void OnScenarioChanged()
    {
        string text = ScenarioSelectorDropdown.captionText.text;
        switch (text)
        {
            case "Random":
                ScenarioManager.Instance.Scenario = ScenarioManager.ScenarioType.Random;
                break;
            case "TwoLanesHeadOn":
                ScenarioManager.Instance.Scenario = ScenarioManager.ScenarioType.TwoLanesHeadOn;
                break;
            default:
                break;
        }
    }

    public void OnAIChanged()
    {
        if (SelectedActor != null)
        {
            string text = AISelectorDropdown.captionText.text;
            switch (text)
            {
                case "Potential Field":
                    SelectedActor.AI = new AI_PotentialFieldFollowWaypoints(SelectedActor);
                    break;
                case "RRT":
                    SelectedActor.AI = new AI_RRT(SelectedActor);
                    break;
                default:
                    break;
            }
        }
    }

    public void OnShowWaypointsChanged()
    {
        //MainActor.ShowWaypoints = ShowWaypointToggle.isOn;
        foreach (GameObject cur in ScenarioManager.Instance.Objects)
        {
            Actor actor = cur.GetComponent<Actor>();
            if (actor != null)
            {
                actor.ShowWaypoints = ShowWaypointToggle.isOn;
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
