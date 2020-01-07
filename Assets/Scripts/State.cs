using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class State
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;

    public State(Vector3 _position, Quaternion _rotation, Vector3 _velocity)
    {
        position = _position;
        rotation = _rotation;
        velocity = _velocity;
    }

    public State(Vector3 _position, Quaternion _rotation, float velocityAngle, float speed)
    {
        position = _position;
        rotation = _rotation;
        velocity = speed * new Vector3(Mathf.Sin(Mathf.Deg2Rad * velocityAngle), 0.0f, Mathf.Cos(Mathf.Deg2Rad * velocityAngle));
    }

    public State(Vector3 _position, Vector3 _rotationEuler, Vector3 _velocity)
    {
        position = _position;
        rotation = Quaternion.Euler(_rotationEuler);
        velocity = _velocity;
    }

    public State(Vector3 _position, Vector3 _rotationEuler, float  velocityAngle, float speed)
    {
        position = _position;
        rotation = Quaternion.Euler(_rotationEuler);
        velocity = speed * new Vector3(Mathf.Sin(Mathf.Deg2Rad * velocityAngle), 0.0f, Mathf.Cos(Mathf.Deg2Rad * velocityAngle));
    }

    public static State Undefined { get { return new State(Vector3.positiveInfinity, Quaternion.identity, Vector3.positiveInfinity); } }

    public static State Lerp(State start, State end, float t)
    {
        return new State(Vector3.Lerp(start.position, end.position, t), Quaternion.Lerp(start.rotation, end.rotation, t), Vector3.Lerp(start.velocity, end.velocity, t));
    }

}
