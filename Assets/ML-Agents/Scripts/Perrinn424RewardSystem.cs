using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Perrinn424RewardSystem : MonoBehaviour
{
    [SerializeField]
    Perrinn424Agent perrinn424Agent;

    [SerializeField]
    float wallContactPenaltyScale = 5e-4f;

    [SerializeField]
    float offCoursePenaltyScale = 5e-4f;

    float m_PreviousTimeOffCourse;
    float m_PreviousWallHitTime;
    
    void OnEnable()
    {
        Academy.Instance.AgentPreStep += AddStepReward;
        perrinn424Agent.wallHit += OnWallHit;
    }

    void AddStepReward(int stepCount)
    {
        var offCourse = perrinn424Agent.CheckOffCourse();
        if (stepCount % perrinn424Agent.DecisionPeriod == 0 && !offCourse)
        {
            var rewardScalingFactor = (float)perrinn424Agent.DecisionPeriod / perrinn424Agent.MaxStep;
            
            // delta progress reward
            perrinn424Agent.AddReward(rewardScalingFactor * perrinn424Agent.DeltaProgress);
            
            var velocitySquared = Mathf.Pow(perrinn424Agent.Velocity.magnitude, 2);
            
            // off course penalty
            perrinn424Agent.AddReward(- (perrinn424Agent.CumulativeTimeOffCourse - m_PreviousTimeOffCourse) * velocitySquared * offCoursePenaltyScale);
            
            // wall penalty
            perrinn424Agent.AddReward( - (perrinn424Agent.CumulativeWallHitTime - m_PreviousWallHitTime) * velocitySquared * wallContactPenaltyScale);
            
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
