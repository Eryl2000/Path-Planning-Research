using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Actor : MonoBehaviour
{
    private State _curState;
    public State CurState {
        get {
            return _curState;
        }
        set {
            transform.position = value.position;
            transform.rotation = value.rotation;
            _curState = value;
        }
    }

    public AI_Base AI;

    public ShipData ShipData = new ShipData();
    public List<State> waypoints;
    public bool ShowWaypoints = true;
    public bool ShowBoundingBox = true;
    public bool Selected = false;

    LineRenderer boundingBoxLine;
    Color boundingBoxLineColor = new Color(0.2f, 0.81f, 0.2f, 1.0f);
    LineRenderer waypointsLine;
    Color waypointsLineColor = new Color(0, 1, 0, 0.1f);
    float lineThickness = 100.0f;

    void Awake()
    {
        waypoints = new List<State>();
        CurState = new State(transform.position, transform.rotation, transform.forward);
        ShowBoundingBox = true;

        boundingBoxLine = new GameObject().AddComponent<LineRenderer>() as LineRenderer;
        boundingBoxLine.transform.parent = transform;
        boundingBoxLine.name = "BoundingBoxLine";
        boundingBoxLine.material = new Material(Shader.Find("UI/Unlit/Transparent"));
        boundingBoxLine.material.color = boundingBoxLineColor;
        boundingBoxLine.startWidth = boundingBoxLine.endWidth = lineThickness;

        waypointsLine = new GameObject().AddComponent<LineRenderer>() as LineRenderer;
        waypointsLine.transform.parent = transform;
        waypointsLine.name = "WaypointsLine";
        waypointsLine.material = new Material(Shader.Find("UI/Unlit/Transparent"));
        waypointsLine.material.color = waypointsLineColor;
        waypointsLine.startWidth = waypointsLine.endWidth = lineThickness;

        AI = new AI_PotentialFieldFollowWaypoints(this);
    }

    public void AppendWaypoint(State waypoint)
    {
        waypoints.Add(waypoint);
    }

    public void InsertWaypoint(State waypoint)
    {
        waypoints.Insert(0, waypoint);
    }

    public void ClearWaypoints()
    {
        waypoints.Clear();
    }

    public bool WouldHitObject(State testState)
    {
        Collider[] colliderHits = Physics.OverlapBox(testState.position, GetComponent<Collider>().bounds.extents, testState.rotation);
        foreach (Collider col in colliderHits)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Obstacles") && col.gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }

    public void SetInvisible()
    {
        transform.position = new Vector3(10000, 0, 0);
    }

    public void ClearLines()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Line")
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    public float ClosenessMeasure(State curState, State targetState)
    {
        const float noiseRange = 50.0f;
        Vector3 diff = targetState.position - curState.position;
        float dist = diff.magnitude + Random.Range(-noiseRange, noiseRange);
        float minTurningRadius = ShipData.CruiseSpeed / (ShipData.MaxTurningRate * Mathf.Deg2Rad);
        if (dist < minTurningRadius)
        {
            float curAngle = curState.Angle;
            Vector3 forward = new Vector3(Mathf.Sin(Mathf.Deg2Rad * curAngle), 0.0f, Mathf.Cos(Mathf.Deg2Rad * curAngle));
            float angleBetween = Mathf.Abs(Vector3.SignedAngle(forward, diff, Vector3.up));
            return dist + 10.0f / (1.0f + Mathf.Exp(-10.0f * angleBetween / 180.0f + 3));
        }
        return dist;
    }

    public bool ReachedWaypoint(State curState, State waypoint)
    {
        return Vector3.Distance(curState.position, waypoint.position) <= ShipData.WaypointDistanceThreshold;
    }

    private void Update()
    {
        if (Selected)
        {
            waypointsLine.material.color = new Color(waypointsLineColor.r, waypointsLineColor.g, waypointsLineColor.b, 0.5f);
            waypointsLine.startWidth = waypointsLine.endWidth = 2 * lineThickness;
            boundingBoxLine.material.color = Color.green;
            boundingBoxLine.startWidth = boundingBoxLine.endWidth = 2 * lineThickness;
        }
        else
        {
            waypointsLine.material.color = waypointsLineColor;
            waypointsLine.startWidth = waypointsLine.endWidth = 2 * lineThickness;
            boundingBoxLine.material.color = boundingBoxLineColor;
            boundingBoxLine.startWidth = boundingBoxLine.endWidth = lineThickness;
        }
        if (ShowBoundingBox)
        {
            const int numPoints = 10;
            boundingBoxLine.positionCount = numPoints + 2;
            boundingBoxLine.loop = true;

            Vector3 halfSize = GetComponent<BoxCollider>().size / 2.0f;
            float angle = CurState.Angle;
            boundingBoxLine.SetPosition(0, transform.position);
            for (int i = 0; i < numPoints + 1; i++)
            {
                float x = transform.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * halfSize.x * 100.0f;
                float z = transform.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * halfSize.x * 100.0f;
                boundingBoxLine.SetPosition(i + 1, new Vector3(x, transform.position.y + 100.0f, z));
                angle += 360.0f / numPoints;
            }
        }
        else
        {
            boundingBoxLine.positionCount = 0;
        }

        if (ShowWaypoints)
        {
            waypointsLine.positionCount = waypoints.Count + 1;
            waypointsLine.SetPosition(0, transform.position + transform.up * 100.0f);
            for (int i = 0; i < waypoints.Count; ++i)
            {
                waypointsLine.SetPosition(i + 1, waypoints.ElementAt(i).position + transform.up * 100.0f);
            }
        }
        else
        {
            waypointsLine.positionCount = 0;
        }
    }

    void FixedUpdate()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 88.7f))
            {
                Vector3 hitPoint = new Vector3(hit.point.x, 0.0f, hit.point.z);
                waypoints.Add(new State(hitPoint, Quaternion.identity, 0.0f, CruiseSpeed));
                Debug.Log("Closeness measure to origin: " + ClosenessMeasure(new State(Vector3.zero, Quaternion.identity, Vector3.zero), new State(hitPoint, Quaternion.identity, Vector3.zero)));
            }
        }*/

        while (waypoints.Count != 0 && ReachedWaypoint(CurState, waypoints.ElementAt(0)))
        {
            waypoints.RemoveAt(0);
        }
        if (waypoints.Count == 0)
        {
            CurState.Speed = 0.0f;
        }
        else
        {
            if(waypoints.Count != 0)
            {
                waypoints = AI.UpdateWaypoints(CurState, waypoints.Last(), waypoints);
            }
            CurState = AI.StepTowards(CurState, waypoints.ElementAt(0), Time.deltaTime);
        }
    }

}
