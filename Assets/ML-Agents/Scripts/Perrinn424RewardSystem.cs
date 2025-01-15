using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;

public class Perrinn424RewardSystem : MonoBehaviour
{
    [SerializeField]
    Perrinn424Agent perrinn424Agent;

    [SerializeField]
    float progressScale = 5e-4f;
    
    [SerializeField]
    float wallContactPenaltyScale = 5e-4f;

    [SerializeField]
    float offCoursePenaltyScale = 5e-4f;
    
    [SerializeField]
    float steeringAccelerationPenaltyScale = 5e-4f;
    
    [SerializeField]
    float velocityRewardScale = 5e-4f;
    
    [SerializeField]
    float lapCompletionBonus = 1f;
    
    [FormerlySerializedAs("vehiclePhysicsEstimator")] [SerializeField]
    VehiclePhysicsEstimator m_VehiclePhysicsEstimator;
    
    public float currentTotalReward;

    float m_PreviousTimeOffCourse;
    float m_PreviousWallHitTime;
    float m_PreviousSteeringAngle;
    bool m_IsLapComplete;
    
    void OnEnable()
    {
        Academy.Instance.AgentPreStep += AddStepReward;
        m_VehiclePhysicsEstimator.OnLapComplete += OnLapComplete;
    }
    
    void OnLapComplete()
    {
        m_IsLapComplete = true;
    }

    void AddStepReward(int stepCount)
    {
        var offCourse = perrinn424Agent.CheckOffCourse();
        if (stepCount % perrinn424Agent.DecisionPeriod == 0)
        {
            var rewardScalingFactor = (float)perrinn424Agent.DecisionPeriod / perrinn424Agent.MaxStep;
            
            var velocitySquared = Mathf.Pow(perrinn424Agent.Velocity.magnitude, 2);

            var offCoursePenalty = - (perrinn424Agent.CumulativeTimeOffCourse - m_PreviousTimeOffCourse) * velocitySquared * offCoursePenaltyScale;

            var wallPenalty = - (perrinn424Agent.CumulativeWallHitTime - m_PreviousWallHitTime) * velocitySquared * wallContactPenaltyScale;

            // var steeringPenalty = -Mathf.Abs(perrinn424Agent.SteeringAcceleration) * steeringAccelerationPenaltyScale;
            
            var steeringAccelerationPenalty = - Mathf.Abs(m_PreviousSteeringAngle - m_VehiclePhysicsEstimator.m_CurrentSteeringAngle) / Time.fixedDeltaTime * steeringAccelerationPenaltyScale;
            m_PreviousSteeringAngle = m_VehiclePhysicsEstimator.m_CurrentSteeringAngle;

            var lapCompleteReward = 0f;
            if (m_IsLapComplete)
            {
                lapCompleteReward = lapCompletionBonus;
                m_IsLapComplete = false;
            }

            var velocityReward = perrinn424Agent.Velocity.magnitude * velocityRewardScale;

            var progressReward = Mathf.Clamp(!offCourse && !(wallPenalty > 0f) ? progressScale * perrinn424Agent.DeltaProgress : 0f, 0f, 10f);

            currentTotalReward = progressReward + offCoursePenalty + wallPenalty + steeringAccelerationPenalty + lapCompleteReward + velocityReward;
            
            perrinn424Agent.AddReward(rewardScalingFactor * currentTotalReward);
            
            m_PreviousTimeOffCourse = perrinn424Agent.CumulativeTimeOffCourse;
            
            m_PreviousWallHitTime = perrinn424Agent.CumulativeWallHitTime;
            
        }
    }

    void OnWallHit()
    {
        var velocity = perrinn424Agent.Velocity;
        
        // wall hit penalty
        perrinn424Agent.AddReward(- Mathf.Pow(velocity.magnitude, 2) * wallContactPenaltyScale);
    }
    
}
