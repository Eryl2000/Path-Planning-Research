using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Actor : MonoBehaviour
{
    public float speed = 5.0f;

    List<State> waypoints;
    State prevWaypoint;
    int currentWaypointIndex;

    void Awake()
    {
        waypoints = new List<State>();
        prevWaypoint = null;
        currentWaypointIndex = 0;
    }

    public void AddWaypoint(State waypoint)
    {
        waypoints.Add(waypoint);
        prevWaypoint = waypoints.ElementAt(currentWaypointIndex);
    }

    public void ClearWaypoints()
    {
        waypoints.Clear();
        prevWaypoint = null;
        currentWaypointIndex = 0;
    }

    public bool HitsObstacle(State state)
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
        float distToTravelThisStep = speed * Time.fixedDeltaTime;
        float traveledThisStep = 0f;
        while (traveledThisStep < distToTravelThisStep && waypoints.Count > currentWaypointIndex)
        {
            float distToNextWaypoint = Vector3.Distance(transform.position, waypoints.ElementAt(currentWaypointIndex).position);
            if (traveledThisStep + distToNextWaypoint <= distToTravelThisStep)
            {
                transform.position = waypoints.ElementAt(currentWaypointIndex).position;
                transform.rotation = waypoints.ElementAt(currentWaypointIndex).rotation;
                traveledThisStep += distToNextWaypoint;
                prevWaypoint = waypoints.ElementAt(currentWaypointIndex);
                currentWaypointIndex += 1;
            }
            else
            {
                transform.position += (waypoints.ElementAt(currentWaypointIndex).position - transform.position).normalized * (distToTravelThisStep - traveledThisStep);
                float t = Vector3.Distance(prevWaypoint.position, transform.position) / Vector3.Distance(prevWaypoint.position, waypoints.ElementAt(currentWaypointIndex).position);
                transform.rotation = Quaternion.Lerp(prevWaypoint.rotation, waypoints.ElementAt(currentWaypointIndex).rotation, t);
                traveledThisStep = distToTravelThisStep;
            }
        }
    }
}
