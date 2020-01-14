﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Actor : MonoBehaviour
{
    public float Speed = 0.0f;
    public float Angle = 0.0f;
    public float MaxAcceleration = 1.0f;
    public float MaxTurningRate = 3.0f;
    public float CruiseSpeed = 7.71667f;
    public float MinSpeed = 0.0f;
    public float MaxSpeed = 12.8611f;

    public float WaypointThreshold = 10;

    public List<State> waypoints;

    void Awake()
    {
        waypoints = new List<State>();
    }

    public void AddWaypoint(State waypoint)
    {
        waypoints.Add(waypoint);
    }

    public void ClearWaypoints()
    {
        waypoints.Clear();
    }

    public State GetCurState()
    {
        return new State(transform.position, new Vector3(0.0f, Angle, 0.0f), Speed * transform.forward);
    }

    public void SetCurState(State newState)
    {
        transform.position = newState.position;
        transform.rotation = newState.rotation;
        Angle = newState.rotation.eulerAngles.y;
        Speed = newState.velocity.magnitude;
    }

    public bool WouldHitObstacle(State testState)
    {
        Collider[] colliderHits = Physics.OverlapBox(testState.position, GetComponent<Collider>().bounds.extents, testState.rotation/*, 1 << LayerMask.NameToLayer("Obstacles")*/);
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
            float curAngle = curState.rotation.eulerAngles.y;
            Vector3 forward = new Vector3(Mathf.Sin(Mathf.Deg2Rad * curAngle), 0.0f, Mathf.Cos(Mathf.Deg2Rad * curAngle));
            float angleBetween = Mathf.Abs(Vector3.SignedAngle(forward, diff, Vector3.up));
            return dist + 10.0f / (1.0f + Mathf.Exp(-10.0f * angleBetween / 180.0f + 3));
        }
        return dist;
    }

    public bool ReachedWaypoint(State curState, State waypoint)
    {
        return Vector3.Distance(curState.position, waypoint.position) <= WaypointThreshold;
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

        State curState = GetCurState();
        while (waypoints.Count != 0 && ReachedWaypoint(curState, waypoints.ElementAt(0)))
        {
            waypoints.RemoveAt(0);
        }
        if (waypoints.Count == 0)
        {
            return;
        }
        SetCurState(StepTowards(curState, waypoints.ElementAt(0), Time.fixedDeltaTime));
    }

    public State StepTowards(State curState, State targetState, float timeToSimulate)
    {
        if (ReachedWaypoint(curState, targetState))
        {
            return curState;
        }
        float curAngle = curState.rotation.eulerAngles.y;
        float curSpeed = curState.velocity.magnitude;

        //Angle
        Vector3 diff = targetState.position - curState.position;
        float targetAngle = Mathf.Rad2Deg * Mathf.Atan2(diff.x, diff.z);
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
        float targetSpeed = targetState.velocity.magnitude;
        if (curSpeed >= MaxSpeed || targetSpeed > MaxSpeed)
        {
            accel = 0.0f;
            curSpeed = MaxSpeed;
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
