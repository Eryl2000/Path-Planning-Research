using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    private static ScenarioManager _instance = null;
    public static ScenarioManager Instance { get { return _instance; } }

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
            SceneManager.Instance.OnShowWaypointsChanged();
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

    private void Start()
    {
        SceneManager.Instance.OnScenarioChanged();
    }

    public void CreateObject(Vector3 pos, Quaternion orientation, ObjectType type, List<State> waypoints)
    {
        switch (type)
        {
            case ObjectType.Static:
                {
                    GameObject cur = Instantiate(StaticObjectPrefab, pos, orientation, this.transform);
                    cur.gameObject.layer = LayerMask.NameToLayer("Obstacles");
                    cur.transform.localScale = new Vector3(cur.transform.localScale.x * (3 * Random.value + 1), cur.transform.localScale.y, cur.transform.localScale.z * (3 * Random.value + 1));
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
            Vector3 pos = new Vector3(Random.Range(-SceneManager.Instance.BoardX / 2.0f, SceneManager.Instance.BoardX / 2.0f), 0.0f, Random.Range(-SceneManager.Instance.BoardZ / 2.0f, SceneManager.Instance.BoardZ / 2.0f));
            Quaternion orientation = Quaternion.Euler(0, Random.Range(-180.0f, 180.0f), 0);
            CreateObject(pos, orientation, ObjectType.Static, null);
        }
        for (int i = 0; i < numDynamicObjects; ++i)
        {
            State state = GetEmptySpot();
            List<State> waypoints = new List<State>();
            for (int w = 0; w < 3; ++w)
            {
                waypoints.Add(GetEmptySpot());
            }
            CreateObject(state.position, state.rotation, ObjectType.Dynamic, waypoints);
        }
    }

    private State GetEmptySpot()
    {
        bool succesfulPoint;
        Vector3 pos;
        Quaternion orientation;
        do
        {
            succesfulPoint = true;
            pos = new Vector3(Random.Range(-SceneManager.Instance.BoardX / 2.0f, SceneManager.Instance.BoardX / 2.0f), 0.0f, Random.Range(-SceneManager.Instance.BoardZ / 2.0f, SceneManager.Instance.BoardZ / 2.0f));
            orientation = Quaternion.Euler(0, Random.Range(-180.0f, 180.0f), 0);

            Collider[] colliderHits = Physics.OverlapBox(pos, DynamicObjectPrefab.GetComponent<Collider>().bounds.extents, orientation);
            foreach (Collider col in colliderHits)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
                {
                    succesfulPoint = false;
                    break;
                }
            }
        } while (!succesfulPoint);
        return new State(pos, orientation, orientation.eulerAngles.y, DynamicObjectPrefab.ShipData.CruiseSpeed);
    }

    private void GenerateTwoLanesHeadOn()
    {
        Vector3 location1 = new Vector3(-SceneManager.Instance.BoardX * 0.3f, 0.0f, 0.0f);
        Vector3 location2 = new Vector3(SceneManager.Instance.BoardX * 0.3f, 0.0f, -SceneManager.Instance.BoardZ * 0.1f);
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
                waypoints.Add(new State(target, new Vector3(0.0f, 90.0f, 0.0f), 0.0f, DynamicObjectPrefab.ShipData.CruiseSpeed));
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
                waypoints.Add(new State(target, new Vector3(0.0f, -90.0f, 0.0f), 0.0f, DynamicObjectPrefab.ShipData.CruiseSpeed));
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
