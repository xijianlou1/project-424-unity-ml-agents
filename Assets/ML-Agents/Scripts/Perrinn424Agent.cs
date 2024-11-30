using System;
using System.Collections;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.InputManagement;
using Perrinn424.AutopilotSystem;

public class Perrinn424Agent : Agent
{
    public LookAheadSensor lookAheadSensor;
    public TerrainSensor terrainSensor;
    public StateEstimator stateEstimator;
    public float steeringRate = 90f;
    public float throttleRate = 100f;
    public float brakeRate = 100f;
    public event Action wallHit;
    public event Action wallStay;

    public int DecisionPeriod { get; private set; }

    public float DeltaProgress => GetDeltaProgress() * Mathf.Sign(m_CenterlineAlignment);

    Vector3 m_InitialPosition;
    Quaternion m_InitialRotation;
    Transform m_InitialTransform;
    Vector3 m_CurrentVelocity;
    Vector3 m_CurrentAcceleration;
    float m_CumulativeTimeOffCourse;
    float m_CumulativeWallHitTime;
    Rigidbody m_Rigidbody;
    VehicleBase m_VehicleBase;
    Steering.Settings m_SteeringSettings;
    AgentPilot m_AgentPilot;
    float m_CenterlineAlignment;
    float m_CurrentProgress;
    float m_PreviousProgress;
    Sample m_CurrentActions;
    Sample m_PreviousActions;
    bool m_WallContact = false;
    
    public Vector3 Velocity => m_CurrentVelocity;
    
    public float CumulativeTimeOffCourse => m_CumulativeTimeOffCourse;

    public float CumulativeWallHitTime => m_CumulativeWallHitTime;
    
    void FixedUpdate()
    {
        m_CurrentVelocity = stateEstimator.velocity;
        m_CurrentAcceleration = stateEstimator.acceleration;
        CheckVertical();
        if (CheckOffCourse())
        {
            m_CumulativeTimeOffCourse += Time.fixedDeltaTime;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            m_WallContact = true;
            wallHit?.Invoke();
        }
    }
    
    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            m_WallContact = true;
            m_CumulativeWallHitTime += Time.fixedDeltaTime;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (m_WallContact)
        {
            m_WallContact = false;
        }
    }

    public override void Initialize()
    {
        var rootTransform = transform;
        m_InitialTransform = rootTransform;
        m_InitialPosition = rootTransform.position;
        m_InitialRotation = rootTransform.rotation;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AgentPilot = GetComponentInChildren<AgentPilot>();
        m_VehicleBase = GetComponentInChildren<VehicleBase>();
        DecisionPeriod = GetComponent<DecisionRequester>().DecisionPeriod;
        m_SteeringSettings = m_VehicleBase.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;
    }
    
    public override void OnEpisodeBegin()
    {
        ResetVariables();
        ResetSensors();
        ResetControlInputs();
        StartCoroutine(ResetPhysicsAndPose());
        m_CumulativeTimeOffCourse = 0f;
        m_CumulativeWallHitTime = 0f;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        var (centerLineAngle, centerLineOffset, newLookahead, progress, centerlineProgress) = lookAheadSensor.Sense();
        m_CenterlineAlignment = lookAheadSensor.CenterlineAlignment;
        m_PreviousProgress = m_CurrentProgress;
        m_CurrentProgress = centerlineProgress;
        var terrainSense = terrainSensor.Sense().Sum();
        
        // total observations : 44
        
        // linear velocity vec3 : 3
        sensor.AddObservation(m_CurrentVelocity);
        
        // linear acceleration vec3 : 3
        sensor.AddObservation(m_CurrentAcceleration);
        
        // centerline angle float : 1
        sensor.AddObservation(centerLineAngle);
        
        // previous steering command float : 1
        sensor.AddObservation(m_PreviousActions.steeringAngle);
        
        // wall contact bool : 1
        sensor.AddObservation(m_WallContact);
        
        // look ahead sensor newLookAhead.length : 10
        foreach (var point in newLookahead)
        {
           sensor.AddObservation(transform.InverseTransformPoint(point));
        }
        
        // off-course check bool : 1
        sensor.AddObservation(CheckOffCourse());
        
        // off-course sense float[] : 4
        sensor.AddObservation(terrainSensor.Sense().ToArray());
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float physicalWheelRange = InputManager.instance.settings.physicalWheelRange;
        var steeringInput = 360f * Mathf.Clamp(actionBuffers.ContinuousActions[0] / m_SteeringSettings.steeringWheelRange * physicalWheelRange, -1.0f, 1.0f);
        steeringInput = Mathf.MoveTowards(m_CurrentActions.steeringAngle, steeringInput, steeringRate * Time.fixedDeltaTime * DecisionPeriod);
        var throttleInput = 100f * Mathf.Clamp01(actionBuffers.ContinuousActions[1]);
        throttleInput = Mathf.MoveTowards(m_CurrentActions.throttle, throttleInput, throttleRate * Time.fixedDeltaTime * DecisionPeriod);
        var brakeInput = 100f * Mathf.Clamp01(-actionBuffers.ContinuousActions[1]);
       brakeInput = Mathf.MoveTowards(m_CurrentActions.brake, brakeInput, brakeRate * Time.fixedDeltaTime * DecisionPeriod); 
        var agentActionSample = new Sample();
        if (Academy.Instance.AutomaticSteppingEnabled)
        {
            agentActionSample = new Sample()
            {
                steeringAngle = steeringInput, 
                throttle = throttleInput, 
                brake = brakeInput,
                drsPosition = 0,
                gear = 1,
            };
        }
        
        m_CurrentActions = agentActionSample;
        
        m_AgentPilot.currentAgentSample = agentActionSample;
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (Academy.Instance.AutomaticSteppingEnabled)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");
        }
    }

    public bool CheckOffCourse()
    {
        var terrainSense = terrainSensor.Sense().Sum();
        return terrainSense >= 0.75f;
    }

    void CheckVertical()
    {
        var agentUp = transform.up;
        var upAngle = Vector3.Angle(Vector3.up, agentUp);
        if (upAngle > 45)
        {
            AddReward(-10f);
            EndEpisode();
        }
    }

    float GetDeltaProgress()
    {
        var deltaProgress = m_CurrentProgress > m_PreviousProgress ? m_CurrentProgress - m_PreviousProgress : 1 + m_CurrentProgress - m_PreviousProgress;
        return deltaProgress;
    }

    void ResetVariables()
    {
        m_CumulativeTimeOffCourse = 0f;
        m_CurrentVelocity = Vector3.zero;
        m_CurrentAcceleration = Vector3.zero;
    }

    void ResetSensors()
    { 
        stateEstimator.Reset();
    }

    void ResetControlInputs()
    {
        m_AgentPilot.currentAgentSample = new Sample()
        {
            steeringAngle = 0f, 
            throttle = 0f, 
            brake = 0f,
            drsPosition = 0,
            gear = 0,
        };
    }

    IEnumerator ResetPhysicsAndPose()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;
        m_VehicleBase.enabled = false;
        m_Rigidbody.isKinematic = false;
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody.isKinematic = true;
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody.Move(m_InitialPosition, m_InitialRotation);
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody.isKinematic = false;
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        Academy.Instance.AutomaticSteppingEnabled = true;
        m_VehicleBase.enabled = true;
    }
}
