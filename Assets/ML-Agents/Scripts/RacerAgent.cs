using System;
using System.Collections;
using Perrinn424;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;
using VehiclePhysics;
using VehiclePhysics.InputManagement;

public class RacerAgent : Agent
{
    [SerializeField]
    Perrinn424Input carControllerInput;

    [FormerlySerializedAs("trackCenterline")]
    [SerializeField]
    SplineContainer trackCenterlineSplineContainer;

    [SerializeField]
    int numberOfLookAheadPoints = 10;

    [SerializeField]
    bool debug = true;

    Rigidbody m_RigidBody;
    Vector3 m_InitialPosition;
    Quaternion m_InitialRotation;
    VehicleBase m_Vehicle;
    Steering.Settings m_SteeringSettings;
    Vector3 m_PreviousVelocity;
    float[] m_PreviousActions;
    Spline m_CenterLineSpline;
    Vector3[] m_LookAheadBuffer;
    Vector3 m_SplineRoot;

    public override void Initialize()
    {
        var rootTransform = transform;
        m_InitialPosition = rootTransform.position;
        m_InitialRotation = rootTransform.rotation;
        m_RigidBody = GetComponent<Rigidbody>();
        m_Vehicle = GetComponentInChildren<VehicleBase>();
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.EnableProcessedInput, 1);
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.InputGear, 1);
        m_SteeringSettings = m_Vehicle.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;
        m_PreviousActions = new float[4];
        m_CenterLineSpline = trackCenterlineSplineContainer[0];
        m_LookAheadBuffer = new Vector3[numberOfLookAheadPoints];
        m_SplineRoot = trackCenterlineSplineContainer.transform.position;
    }

    public override void OnEpisodeBegin()
    {
        StartCoroutine(ResetPhysicsAndPose());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // total obs space 46

        // linear velocity vec3

        sensor.AddObservation(transform.InverseTransformDirection(m_RigidBody.velocity));

        // linear acceleration vec3
        sensor.AddObservation(transform.InverseTransformDirection(m_RigidBody.velocity - m_PreviousVelocity));
        m_PreviousVelocity = m_RigidBody.velocity;

        // angular velocity vec3
        sensor.AddObservation(transform.InverseTransformDirection(m_RigidBody.angularVelocity));

        // previous actions float[4]
        sensor.AddObservation(m_PreviousActions);

        // centerline angle float, look ahead float[100], progress along track (sin and cos components) float[2]

        (var centerlineAngle, var lookAhead, var progress) = GetSplineObservations();
        m_LookAheadBuffer = lookAhead;
        sensor.AddObservation(centerlineAngle);
        foreach (var point in lookAhead)
        {
            sensor.AddObservation(point);
        }

        sensor.AddObservation(progress);

        // wall collisions (binary 1 = hit) float - Not sure we need this if we reset on collision - tbd
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float physicalWheelRange = InputManager.instance.settings.physicalWheelRange;
        var steeringInput = 360f * Mathf.Clamp(actionBuffers.ContinuousActions[0] / m_SteeringSettings.steeringWheelRange * physicalWheelRange, -1.0f, 1.0f);
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.InputSteerAngle, (int)(steeringInput * 10000f));
        float throttleInput = Mathf.Clamp01(actionBuffers.ContinuousActions[1]);
        float brakeInput = Mathf.Clamp01(-actionBuffers.ContinuousActions[1]);
        float drsInput = Mathf.Clamp01(actionBuffers.ContinuousActions[2]);
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.InputThrottlePosition, (int)(throttleInput * 10000f));
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.InputBrakePosition, (int)(brakeInput * 10000f));
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.InputDrsPosition, (int)(drsInput * 1000f));
        m_PreviousActions[0] = steeringInput;
        m_PreviousActions[1] = throttleInput;
        m_PreviousActions[2] = brakeInput;
        m_PreviousActions[3] = drsInput;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = 0f;
    }

    IEnumerator ResetPhysicsAndPose()
    {
        carControllerInput.externalBrake = 0f;
        carControllerInput.externalThrottle = 0f;
        carControllerInput.externalSteer = 0f;
        // m_RigidBody.velocity = Vector3.zero;
        // m_RigidBody.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(1f / 500);
        m_RigidBody.isKinematic = true;
        m_RigidBody.Move(m_InitialPosition, m_InitialRotation);
        yield return new WaitForSeconds(1f / 500);
        m_RigidBody.isKinematic = false;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            EndEpisode();
        }
    }

    (float, Vector3[], Vector2) GetSplineObservations()
    {
        SplineUtility.GetNearestPoint(m_CenterLineSpline, transform.position - m_SplineRoot, out float3 nearestPoint, out float t);
        // Debug.Log(nearestPoint + (float3)m_SplineRoot);
        var centerLineDirection = SplineUtility.EvaluateTangent(m_CenterLineSpline, t);
        var centerLineAngle = Vector3.Angle(centerLineDirection, transform.forward);
        var lookahead = new Vector3[numberOfLookAheadPoints];
        for (int i = 0; i < numberOfLookAheadPoints; i++)
        {
            lookahead[i] = SplineUtility.GetPointAtLinearDistance(m_CenterLineSpline, t, i * 10f + 1f, out float resultT) + (float3)m_SplineRoot;
        }

        var progress = new Vector2(Mathf.Sin(2 * Mathf.PI * t), Mathf.Cos(2 * Mathf.PI * t));
        return (centerLineAngle, lookahead, progress);
    }

    void OnDrawGizmos()
    {
        if (debug)
        {
            if (m_LookAheadBuffer?.Length > 0)
            {
                foreach (var point in m_LookAheadBuffer)
                {
                    var oldColor = Gizmos.color;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(point, 1);
                    Gizmos.color = oldColor;
                }
            }
        }
    }
}
