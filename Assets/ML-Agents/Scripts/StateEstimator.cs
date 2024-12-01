using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateEstimator : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 angularVelocity;

    Vector3 m_PreviousVelocity;

    void FixedUpdate()
    {
        position = rb.transform.position;
        rotation = rb.transform.rotation.eulerAngles;
        m_PreviousVelocity = velocity;
        velocity = rb.transform.InverseTransformVector(rb.velocity);
        acceleration = (velocity - m_PreviousVelocity) / Time.fixedDeltaTime;
        angularVelocity = rb.transform.InverseTransformVector(rb.angularVelocity);
    }

    public void Reset()
    {
        position = Vector3.zero;
        rotation = Vector3.zero;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        m_PreviousVelocity = Vector3.zero;
    }
}
