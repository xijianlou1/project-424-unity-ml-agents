using System.Collections.Generic;

namespace ML_Agents.Scripts
{
    public class AgentBenchmarkData
    {
        public List<VehicleData> SectorData = new () {new VehicleData()};
        public List<ProcessedVehicleData> ProcessedSectorData = new () {new ProcessedVehicleData()};
    }

    public class VehicleData
    {
        public List<float> Speed { get; set; } = new();
        public List<string> Gear { get; set; } = new();
        public List<float> TotalElecPower { get; set; } = new();
        public List<float> BatterySOC { get; set; } = new();
        public List<float> BatteryCapacity { get; set; } = new();
        public List<int> ThrottlePosition { get; set; } = new();
        public List<float> BrakePosition { get; set; } = new();
        public List<float> SteeringAngle { get; set; } = new();
        public List<float> EngagedGear { get; set; } = new();
        public List<float> FrontPowertrain { get; set; } = new();
        public List<float> RearPowertrain { get; set; } = new();
        public List<float> GroundTrackerFrontRideHeight { get; set; } = new();
        public List<float> GroundTrackerRearRideHeight { get; set; } = new();
        public List<float> GroundTrackerFrontRollAngle { get; set; } = new();
        public List<float> GroundTrackerRearRollAngle { get; set; } = new();
    }

    public class ProcessedVehicleData
    {
        public float Time { get; set; }
        public float AverageSpeed { get; set; }
        public float AverageTotalElecPower { get; set; }
        public float AverageBatterySOC { get; set; }
        public float AverageBatteryCapacity { get; set; }
        public float AverageTrottlePosition { get; set; }
        public float AverageBrakePosition { get; set; }
        public float AverageSteeringAngle { get; set; }
        public float AverageEngagedGear { get; set; }
        public float AverageFrontPowertrain { get; set; }
        public float AverageRearPowertrain { get; set; }
        public float AverageGroundTrackerFrontRideHeight { get; set; }
        public float AverageGroundTrackerRearRideHeight { get; set; }
        public float AverageGroundTrackerFrontRollAngle { get; set; }
        public float AverageGroundTrackerRearRollAngle { get; set; }
    }
}