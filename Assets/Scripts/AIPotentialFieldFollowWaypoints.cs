using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPotentialFieldFollowWaypoints : AIBase
{
    public AIPotentialFieldFollowWaypoints(Actor _actor) : base(_actor)
    {

    }

    public override List<State> UpdateWaypoints(List<State> currentWaypoints)
    {
        return currentWaypoints;
    }

    public override State StepTowards(State curState, State targetState, float dt)
    {
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
        if (Mathf.Abs(angleDiff) <= actor.ShipData.MaxTurningRate * dt)
        {
            curAngle = targetAngle;
        }
        else
        {
            curAngle += Mathf.Sign(angleDiff) * actor.ShipData.MaxTurningRate * dt;
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
        targetSpeed = Mathf.Clamp(targetSpeed, 0.0f, actor.ShipData.MaxSpeed);
        if (curSpeed >= actor.ShipData.MaxSpeed && targetSpeed >= actor.ShipData.MaxSpeed)
        {
            accel = 0.0f;
            curSpeed = actor.ShipData.MaxSpeed;
        }
        else if (curSpeed <= 0.0f && targetSpeed <= 0.0f)
        {
            accel = 0.0f;
            curSpeed = 0.0f;
        }
        else if (Mathf.Abs(targetSpeed - curSpeed) < actor.ShipData.MaxAcceleration * dt)
        {
            accel = (targetSpeed - curSpeed) / dt;
            curSpeed = targetSpeed;
        }
        else
        {
            accel = Mathf.Sign(targetSpeed - curSpeed) * actor.ShipData.MaxAcceleration;
            curSpeed += accel * dt;
        }

        //Pack into State object
        Vector3 forward = new Vector3(Mathf.Sin(Mathf.Deg2Rad * curAngle), 0.0f, Mathf.Cos(Mathf.Deg2Rad * curAngle));
        Vector3 newPos = curState.position + forward * (0.5f * accel * dt * dt + curSpeed * dt);
        return new State(newPos, new Vector3(0.0f, curAngle, 0.0f), curAngle, curSpeed);
    }


    private Vector3 GetPotentialVector(Vector3 curPos, Vector3 target)
    {
        Vector3 potential = Vector3.zero;
        foreach (GameObject go in ScenarioManager.Instance.Objects)
        {
            if (go == actor.gameObject)
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
}
