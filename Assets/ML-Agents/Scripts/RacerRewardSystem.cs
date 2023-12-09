using Unity.MLAgents;
using UnityEngine;

public class RacerRewardSystem : MonoBehaviour
{
    [SerializeField]
    RacerAgent racerAgent;

    void OnEnable()
    {
        Academy.Instance.AgentPreStep += AddStepReward;
    }

    void AddStepReward(int stepCount)
    {
        var offCourse = racerAgent.CheckOffCourse();
        if (stepCount % racerAgent.DecisionPeriod == 0 && !offCourse)
        {
            var rewardScalingFactor = (float)racerAgent.DecisionPeriod / racerAgent.MaxStep;
            
            racerAgent.AddReward(rewardScalingFactor * racerAgent.Progress);
        }

        if (offCourse)
        {
            racerAgent.EndEpisode();
        }
    }
}
