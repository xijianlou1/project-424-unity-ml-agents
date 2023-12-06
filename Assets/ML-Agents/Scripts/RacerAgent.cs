using System.Collections;
using Perrinn424;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.InputManagement;

public class RacerAgent : Agent
{
    [SerializeField]
    Perrinn424Input carControllerInput;


    Rigidbody m_RigidBody;
    Vector3 m_InitialPosition;
    Quaternion m_InitialRotation;
    VehicleBase m_Vehicle;
    Steering.Settings m_SteeringSettings;

    public override void Initialize()
    {
        var rootTransform = transform;
        m_InitialPosition = rootTransform.position;
        m_InitialRotation = rootTransform.rotation;
        m_RigidBody = GetComponent<Rigidbody>();
        m_Vehicle = GetComponentInChildren<VehicleBase>();
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.EnableProcessedInput, 1);
        m_Vehicle.data.Set(Channel.Custom,Perrinn424Data.InputGear, 1);
        m_SteeringSettings = m_Vehicle.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;
    }

    public override void OnEpisodeBegin()
    {
        StartCoroutine(ResetPhysicsAndPose());
    }
    

    public override void CollectObservations(VectorSensor sensor)
    {
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float physicalWheelRange = InputManager.instance.settings.physicalWheelRange;
        var steeringInput = 360f * Mathf.Clamp(actionBuffers.ContinuousActions[0] / m_SteeringSettings.steeringWheelRange * physicalWheelRange, -1.0f, 1.0f);
        Debug.Log(steeringInput);
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.InputSteerAngle, (int)(steeringInput * 10000f));
        
        float throttleInput = Mathf.Clamp01(actionBuffers.ContinuousActions[1]);
        float brakeInput = Mathf.Clamp01(-actionBuffers.ContinuousActions[1]);

        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.InputThrottlePosition, (int)(throttleInput * 10000f));
        m_Vehicle.data.Set(Channel.Custom, Perrinn424Data.InputBrakePosition, (int)(brakeInput * 10000f));
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    IEnumerator ResetPhysicsAndPose()
    {
        carControllerInput.externalBrake = 0f;
        carControllerInput.externalThrottle = 0f;
        carControllerInput.externalSteer = 0f;
        m_RigidBody.velocity = Vector3.zero;
        m_RigidBody.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(1f / 500);
        m_RigidBody.isKinematic = true;
        m_RigidBody.Move(m_InitialPosition, m_InitialRotation);
        yield return new WaitForSeconds(1f / 500);
        m_RigidBody.isKinematic = false;
    }
}
