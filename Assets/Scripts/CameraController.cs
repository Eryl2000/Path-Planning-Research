using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float Speed = 2000.0f;
    public float ShiftMultiplier = 5.0f;
    public Vector3 velocity;
    void Start()
    {
        velocity = Vector3.zero;
    }


    private void Update()
    {
        float multiplier = 1.0f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            multiplier = ShiftMultiplier;
        }

        velocity.x = (velocity.x < 0 ? -1.0f : (velocity.x > 0 ? 1.0f : 0.0f)) * Speed * multiplier;
        velocity.y = (velocity.y < 0 ? -1.0f : (velocity.y > 0 ? 1.0f : 0.0f)) * Speed * multiplier;
        velocity.z = (velocity.z < 0 ? -1.0f : (velocity.z > 0 ? 1.0f : 0.0f)) * Speed * multiplier;

        velocity.x = Input.GetAxis("Horizontal") * Speed * multiplier;
        velocity.y = Input.GetAxis("Up/Down") * Speed * multiplier;
        velocity.z = Input.GetAxis("Vertical") * Speed * multiplier;
    }


    void FixedUpdate()
    {
        transform.position += velocity * Time.fixedDeltaTime;
    }
}
