using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    private static ScenarioManager _instance = null;
    public static ScenarioManager Instance { get { return _instance; } }

    public Actor MainActor;
    public Toggle ShowWaypointToggle;
    public Dropdown ScenarioSelectorDropdown;

    public GameObject StaticObjectPrefab;
    public Actor DynamicObjectPrefab;
    public int numStaticObjects = 10;
    public int numDynamicObjects = 5;

    public List<GameObject> Objects = new List<GameObject>();
    public List<GameObject> StaticObjects = new List<GameObject>();
    public List<GameObject> DynamicObjects = new List<GameObject>();

    public enum ObjectType { Static, Dynamic };
    public enum ScenarioType { Random, TwoLanesHeadOn };

    private ScenarioType scenario;
    public ScenarioType Scenario {
        get {
            return scenario;
        }
        set {
            ClearScenario();
            scenario = value;
            switch (value)
            {
                case ScenarioType.Random:
                    GenerateRandom();
                    break;
                case ScenarioType.TwoLanesHeadOn:
                    GenerateTwoLanesHeadOn();
                    break;
                default:
                    break;
            }
            OnShowWaypointsChanged();
        }
    }

    public float boardX = 0.0f;
    public float boardZ = 0.0f;

    public void OnScenarioChanged()
    {
        string text = ScenarioSelectorDropdown.captionText.text;
        switch (text)
        {
            case "Random":
                Scenario = ScenarioType.Random;
                break;
            case "TwoLanesHeadOn":
                Scenario = ScenarioType.TwoLanesHeadOn;
                break;
            default:
                break;
        }
    }

    public void OnShowWaypointsChanged()
    {
        MainActor.ShowWaypoints = ShowWaypointToggle.isOn;
        foreach (GameObject cur in Objects)
        {
            Actor actor = cur.GetComponent<Actor>();
            if (actor != null)
            {
                actor.ShowWaypoints = ShowWaypointToggle.isOn;
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

    public void CreateObject(Vector3 pos, Quaternion orientation, ObjectType type, List<State> waypoints)
    {
        switch (type)
        {
            case ObjectType.Static:
                {
                    GameObject cur = Instantiate(StaticObjectPrefab, pos, orientation, this.transform);
                    cur.gameObject.layer = LayerMask.NameToLayer("Obstacles");
                    cur.transform.localScale = new Vector3(cur.transform.localScale.x * (3 * Random.value + 1), 1, cur.transform.localScale.z * (3 * Random.value + 1));
                    Objects.Add(cur);
                    StaticObjects.Add(cur);
                    break;
                }
            case ObjectType.Dynamic:
                {
                    Actor cur = Instantiate(DynamicObjectPrefab, pos, orientation, this.transform);
                    cur.CurState = new State(pos, orientation, Vector3.zero);
                    foreach (State waypoint in waypoints)
                    {
                        cur.AppendWaypoint(waypoint);
                    }
                    cur.gameObject.layer = LayerMask.NameToLayer("Obstacles");
                    Objects.Add(cur.gameObject);
                    DynamicObjects.Add(cur.gameObject);
                    break;
                }
            default:
                break;
        }
    }

    private void GenerateRandom()
    {
        for (int i = 0; i < numStaticObjects; ++i)
        {
            Vector3 pos = new Vector3(Random.Range(-boardX / 2.0f, boardX / 2.0f), 0.0f, Random.Range(-boardZ / 2.0f, boardZ / 2.0f));
            Quaternion orientation = Quaternion.Euler(0, Random.Range(-180.0f, 180.0f), 0);
            CreateObject(pos, orientation, ObjectType.Static, null);
        }
        for (int i = 0; i < numDynamicObjects; ++i)
        {
            Vector3 pos = new Vector3(Random.Range(-boardX / 2.0f, boardX / 2.0f), 0.0f, Random.Range(-boardZ / 2.0f, boardZ / 2.0f));
            Quaternion orientation = Quaternion.Euler(0, Random.Range(-180.0f, 180.0f), 0);
            List<State> waypoints = new List<State>();
            for (int w = 0; w < 3; ++w)
            {
                Vector3 target = new Vector3(Random.Range(-boardX / 2.0f, boardX / 2.0f), 0.0f, Random.Range(-boardZ / 2.0f, boardZ / 2.0f));
                waypoints.Add(new State(target, new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), 0.0f, DynamicObjectPrefab.CruiseSpeed));
            }
            CreateObject(pos, orientation, ObjectType.Dynamic, waypoints);
        }
    }

    private void GenerateTwoLanesHeadOn()
    {
        Vector3 location1 = new Vector3(-boardX * 0.3f, 0.0f, 0.0f);
        Vector3 location2 = new Vector3(boardX * 0.3f, 0.0f, -boardZ * 0.1f);
        float spawnRadius = 10000.0f;

        Vector2 deviation;
        for (int i = 0; i < numDynamicObjects / 2; ++i)
        {
            Vector3 pos = location1;
            deviation = Random.insideUnitCircle;
            pos.x += spawnRadius * deviation.x;
            pos.z += spawnRadius * deviation.y;

            Quaternion orientation = Quaternion.Euler(0, 90.0f, 0);
            List<State> waypoints = new List<State>();
            for (int w = 0; w < 1; ++w)
            {
                Vector3 target = location2;
                deviation = Random.insideUnitCircle;
                target.x += spawnRadius * deviation.x;
                target.z += spawnRadius * deviation.y;
                waypoints.Add(new State(target, new Vector3(0.0f, 90.0f, 0.0f), 0.0f, DynamicObjectPrefab.CruiseSpeed));
            }
            CreateObject(pos, orientation, ObjectType.Dynamic, waypoints);
        }
        for (int i = 0; i < (numDynamicObjects + 1) / 2; ++i)
        {
            Vector3 pos = location2;
            deviation = Random.insideUnitCircle;
            pos.x += spawnRadius * deviation.x;
            pos.z += spawnRadius * deviation.y;

            Quaternion orientation = Quaternion.Euler(0, -90.0f, 0);
            List<State> waypoints = new List<State>();
            for (int w = 0; w < 1; ++w)
            {
                Vector3 target = location1;
                deviation = Random.insideUnitCircle;
                target.x += spawnRadius * deviation.x;
                target.z += spawnRadius * deviation.y;
                waypoints.Add(new State(target, new Vector3(0.0f, -90.0f, 0.0f), 0.0f, DynamicObjectPrefab.CruiseSpeed));
            }
            CreateObject(pos, orientation, ObjectType.Dynamic, waypoints);
        }
    }

    private void ClearScenario()
    {
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }
        Objects.Clear();
        StaticObjects.Clear();
        DynamicObjects.Clear();
    }
}
