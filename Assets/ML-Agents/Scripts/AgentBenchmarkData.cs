using System.Collections.Generic;

namespace ML_Agents.Scripts
{
    public class AgentBenchmarkData
    {
        public List<LarpData> LarpData;
        public List<ExtractedLarpData> ExtractedLarpData;
    }

    public class LarpData
    {
        public List<float> Speed;
        public List<string> Gear;
        public List<float> TotalElecPower;
        public List<float> BatterySOC;
        public List<float> BatteryCapacity;
        public List<int> ThrottlePosition;
        public List<float> BrakePosition;
        public List<float> SteeringAngle;
        public List<float> EngagedGear;
        public List<float> FrontPowertrain;
        public List<float> RearPowertrain;
        public List<float> GroundTrackerFrontRideHeight;
        public List<float> GroundTrackerRearRideHeight;
        public List<float> GroundTrackerFrontRollAngle;
        public List<float> GroundTrackerRearRollAngle;
    }

    public class ExtractedLarpData
    {
        public float AverageSpeed;
        public float AverageTotalElecPower;
        public float AverageBatterySOC;
        public float AverageBatteryCapacity;
        public float AverageTrottlePosition;
        public float AverageBrakePosition;
        public float AverageSteeringAngle;
        public float AverageEngagedGear;
        public float AverageFrontPowertrain;
        public float AverageRearPowertrain;
        public float AverageGroundTrackerFrontRideHeight;
        public float AverageGroundTrackerRearRideHeight;
        public float AverageGroundTrackerFrontRollAngle;
        public float AverageGroundTrackerRearRollAngle;
    }
}