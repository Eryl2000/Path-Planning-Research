using System.Collections;
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

    public bool WouldHitObstacle(State state)
    {
        Vector3 prevPosition = transform.position;
        transform.position = state.position;
        transform.rotation = state.rotation;
        Collider[] colliderHits = Physics.OverlapBox(transform.position, GetComponent<Collider>().bounds.extents, transform.rotation, 1 << LayerMask.NameToLayer("Obstacles"));
        foreach (Collider col in colliderHits)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
            {
                transform.position = prevPosition;
                return true;
            }
        }
        transform.position = prevPosition;
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

    void FixedUpdate()
    {
        if (waypoints.Count == 0)
        {
            return;
        }
        //Reached waypoint
        while (waypoints.Count != 0 && Vector3.Distance(transform.position, waypoints.ElementAt(0).position) <= WaypointThreshold)
        {
            waypoints.RemoveAt(0);
        }
        if (waypoints.Count == 0)
        {
            return;
        }

        State newState = StepTowards(new State(transform.position, transform.rotation, Speed * transform.forward), waypoints.ElementAt(0));
        transform.position = newState.position;
        transform.rotation = newState.rotation;
        Angle = newState.rotation.eulerAngles.y;
        Speed = newState.velocity.magnitude;

        //Angle
        /*Vector3 diff = waypoints.ElementAt(0).position - transform.position;
        float targetAngle = Mathf.Rad2Deg * Mathf.Atan2(diff.x, diff.z);
        if (targetAngle < 0.0f)
        {
            targetAngle += 360.0f;
        }
        float angleDiff = targetAngle - Angle;
        while (angleDiff > 180.0f)
        {
            angleDiff -= 360.0f;
        }
        while (angleDiff < -180.0f)
        {
            angleDiff += 360.0f;
        }
        if (Mathf.Abs(angleDiff) <= MaxTurningRate * Time.fixedDeltaTime)
        {
            Angle = targetAngle;
        }
        else
        {
            Angle += Mathf.Sign(angleDiff) * MaxTurningRate * Time.fixedDeltaTime;
            if (Angle > 360.0f)
            {
                Angle -= 360.0f;
            }
            else if (Angle < 0.0f)
            {
                Angle += 360.0f;
            }
        }

        //Speed
        float accel;
        float targetSpeed = waypoints.ElementAt(0).velocity.magnitude;
        if (Speed >= MaxSpeed || targetSpeed > MaxSpeed)
        {
            accel = 0.0f;
            Speed = MaxSpeed;
        }
        else if (Mathf.Abs(targetSpeed - Speed) < MaxAcceleration * Time.fixedDeltaTime)
        {
            accel = (targetSpeed - Speed) / Time.fixedDeltaTime;
            Speed = targetSpeed;
        }
        else
        {
            accel = Mathf.Sign(targetSpeed - Speed) * MaxAcceleration;
            Speed += accel * Time.fixedDeltaTime;
        }

        transform.localPosition += transform.forward * (0.5f * accel * Time.fixedDeltaTime * Time.fixedDeltaTime + Speed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0.0f, Angle, 0.0f);*/
    }

    public State StepTowards(State start, State target)
    {
        float curAngle = start.rotation.eulerAngles.y;
        float curSpeed = start.velocity.magnitude;
        //Angle
        Vector3 diff = target.position - transform.position;
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
        if (Mathf.Abs(angleDiff) <= MaxTurningRate * Time.fixedDeltaTime)
        {
            curAngle = targetAngle;
        }
        else
        {
            curAngle += Mathf.Sign(angleDiff) * MaxTurningRate * Time.fixedDeltaTime;
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
        float targetSpeed = target.velocity.magnitude;
        if (curSpeed >= MaxSpeed || targetSpeed > MaxSpeed)
        {
            accel = 0.0f;
            curSpeed = MaxSpeed;
        }
        else if (Mathf.Abs(targetSpeed - curSpeed) < MaxAcceleration * Time.fixedDeltaTime)
        {
            accel = (targetSpeed - curSpeed) / Time.fixedDeltaTime;
            curSpeed = targetSpeed;
        }
        else
        {
            accel = Mathf.Sign(targetSpeed - curSpeed) * MaxAcceleration;
            curSpeed += accel * Time.fixedDeltaTime;
        }

        Vector3 newPos = transform.position + transform.forward * (0.5f * accel * Time.fixedDeltaTime * Time.fixedDeltaTime + curSpeed * Time.fixedDeltaTime);
        return new State(newPos, new Vector3(0.0f, curAngle, 0.0f), curAngle, curSpeed);
    }
}
