using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipData
{
    public float MaxAcceleration = 1.0f;
    public float MaxTurningRate = 3.0f;
    public float CruiseSpeed = 7.71667f;
    public float MinSpeed = 0.0f;
    public float MaxSpeed = 12.8611f;
    public float WaypointDistanceThreshold = 150;
}
