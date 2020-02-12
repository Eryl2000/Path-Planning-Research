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

    public float MaxAcceleration = 1.0f;
    public float MaxTurningRate = 3.0f;
    public float CruiseSpeed = 7.71667f;
    public float MinSpeed = 0.0f;
    public float MaxSpeed = 12.8611f;
    public float WaypointDistanceThreshold = 150;
    public List<State> waypoints;
    public bool ShowWaypoints = false;
    public bool ShowBoundingBox = false;

    void Awake()
    {
        waypoints = new List<State>();
        CurState = new State(transform.position, transform.rotation, transform.forward);
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
            if (col.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
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
        float minTurningRadius = CruiseSpeed / (MaxTurningRate * Mathf.Deg2Rad);
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
        return Vector3.Distance(curState.position, waypoint.position) <= WaypointDistanceThreshold;
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
            if (CurState.Speed == 0.0f)
            {
                return;
            }
            State stopped = CurState;
            stopped.position += stopped.velocity * 1000.0f;
            stopped.velocity = Vector3.zero;
            CurState = StepTowards(CurState, stopped, Time.fixedDeltaTime);
        }
        else
        {
            CurState = StepTowards(CurState, waypoints.ElementAt(0), Time.fixedDeltaTime);
        }

        if (ShowWaypoints && waypoints.Count > 0)
        {
            Color waypointColor = new Color(0, 1, 0, 0.1f);
            if (ShowBoundingBox)
            {
                waypointColor.a = 0.5f;
            }
            SceneManager.DrawLine(CurState.position, waypoints.ElementAt(0).position, waypointColor, transform, Time.fixedDeltaTime);
            for (int i = 0; i < waypoints.Count - 1; ++i)
            {
                SceneManager.DrawLine(waypoints.ElementAt(i).position + transform.up, waypoints.ElementAt(i + 1).position + transform.up, waypointColor, transform, Time.fixedDeltaTime);
            }
        }
        if (ShowBoundingBox)
        {
            Vector3 center = GetComponent<BoxCollider>().center;
            Vector3 halfSize = GetComponent<BoxCollider>().size / 2.0f;
            Vector3 frontLeft = transform.TransformPoint(center + new Vector3(-halfSize.x, 1, halfSize.z));
            Vector3 frontRight = transform.TransformPoint(center + new Vector3(halfSize.x, 1, halfSize.z));
            Vector3 backLeft = transform.TransformPoint(center + new Vector3(-halfSize.x, 1, -halfSize.z));
            Vector3 backRight = transform.TransformPoint(center + new Vector3(halfSize.x, 1, -halfSize.z));

            Color outlineColor = new Color(0.2f, 0.81f, 0.2f, 1.0f);
            SceneManager.DrawLine(frontLeft, frontRight, outlineColor, transform, Time.fixedDeltaTime);
            SceneManager.DrawLine(frontRight, backRight, outlineColor, transform, Time.fixedDeltaTime);
            SceneManager.DrawLine(backRight, backLeft, outlineColor, transform, Time.fixedDeltaTime);
            SceneManager.DrawLine(backLeft, frontLeft, outlineColor, transform, Time.fixedDeltaTime);
        }
    }

    private Vector3 GetPotentialVector(Vector3 curPos, Vector3 target)
    {
        Vector3 potential = Vector3.zero;
        foreach (GameObject go in ScenarioManager.Instance.Objects)
        {
            if (go == this.gameObject)
            {
                continue;
            }
            Collider col = go.GetComponent<Collider>();
            if (col == null)
            {
                continue;
            }
            Vector3 diff = col.ClosestPoint(curPos) - curPos;
            float dist = diff.magnitude;
            if (dist < PotentialFieldData.thresholdDist)
            {
                dist += 1; //no divide by 0
                potential -= diff.normalized * PotentialFieldData.repulsionCoeff / Mathf.Pow(dist, PotentialFieldData.repulsionExp);
            }
        }

        Vector3 diffTarget = target - curPos;
        float distTarget = diffTarget.magnitude + 1;
        potential += diffTarget.normalized * PotentialFieldData.targetAttractCoeff / Mathf.Pow(distTarget, PotentialFieldData.targetAttractExp);
        return potential;
    }

    public State StepTowards(State curState, State targetState, float timeToSimulate)
    {
        if (ReachedWaypoint(curState, targetState))
        {
            return curState;
        }
        float curAngle = curState.Angle;
        float curSpeed = curState.Speed;
        Vector3 potentialVector = GetPotentialVector(curState.position, targetState.position);

        //Angle
        float targetAngle = Mathf.Rad2Deg * Mathf.Atan2(potentialVector.x, potentialVector.z);
        if (targetAngle < 0.0f)
        {
            targetAngle += 360.0f;
        }
        float angleDiff = targetAngle - curAngle;
        while (angleDiff > 180.0f)
        {
            angleDiff -= 360.0f;
        }
        while (angleDiff < -180.0f)
        {
            angleDiff += 360.0f;
        }
        if (Mathf.Abs(angleDiff) <= MaxTurningRate * timeToSimulate)
        {
            curAngle = targetAngle;
        }
        else
        {
            curAngle += Mathf.Sign(angleDiff) * MaxTurningRate * timeToSimulate;
            if (curAngle > 360.0f)
            {
                curAngle -= 360.0f;
            }
            else if (curAngle < 0.0f)
            {
                curAngle += 360.0f;
            }
        }

        //Speed
        float accel;
        float targetSpeed = targetState.Speed * 0.5f * (1 + Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(curState.velocity, potentialVector)));
        targetSpeed = Mathf.Clamp(targetSpeed, 0.0f, MaxSpeed);
        if (curSpeed >= MaxSpeed && targetSpeed >= MaxSpeed)
        {
            accel = 0.0f;
            curSpeed = MaxSpeed;
        }
        else if (curSpeed <= 0.0f && targetSpeed <= 0.0f)
        {
            accel = 0.0f;
            curSpeed = 0.0f;
        }
        else if (Mathf.Abs(targetSpeed - curSpeed) < MaxAcceleration * timeToSimulate)
        {
            accel = (targetSpeed - curSpeed) / timeToSimulate;
            curSpeed = targetSpeed;
        }
        else
        {
            accel = Mathf.Sign(targetSpeed - curSpeed) * MaxAcceleration;
            curSpeed += accel * timeToSimulate;
        }

        //Pack into State object
        Vector3 forward = new Vector3(Mathf.Sin(Mathf.Deg2Rad * curAngle), 0.0f, Mathf.Cos(Mathf.Deg2Rad * curAngle));
        Vector3 newPos = curState.position + forward * (0.5f * accel * timeToSimulate * timeToSimulate + curSpeed * timeToSimulate);
        return new State(newPos, new Vector3(0.0f, curAngle, 0.0f), curAngle, curSpeed);
    }
}
