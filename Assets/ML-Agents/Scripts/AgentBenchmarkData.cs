using System.Collections.Generic;
using System.Reflection;

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
        public List<float> BatterySOC  { get; set; } = new();
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
        
        public List<float> EngineRpm { get; set; } = new();
        public List<float> EngineLoad { get; set; } = new();
        public List<float> EngineTorque { get; set; } = new();
        public List<float> EnginePower { get; set; } = new();
        public List<float> AidedSteer { get; set; } = new();
        
    }

    public class ProcessedVehicleData
    {
        public float Time { get; set; }
        public float AverageSpeed { get; set; }
        public float AverageTotalElecPower { get; set; }
        public float AverageBatterySOC { get; set; }
        public float AverageBatteryCapacity { get; set; }
        public float AverageThrottlePosition { get; set; }
        public float AverageBrakePosition { get; set; }
        public float AverageSteeringAngle { get; set; }
        public float AverageEngagedGear { get; set; }
        public float AverageFrontPowertrain { get; set; }
        public float AverageRearPowertrain { get; set; }
        public float AverageGroundTrackerFrontRideHeight { get; set; }
        public float AverageGroundTrackerRearRideHeight { get; set; }
        public float AverageGroundTrackerFrontRollAngle { get; set; }
        public float AverageGroundTrackerRearRollAngle { get; set; }
        public float AverageEngineRpm { get; set; } = new();
        public float AverageEngineLoad { get; set; } = new();
        public float AverageEngineTorque { get; set; } = new();
        public float AverageEnginePower { get; set; } = new();
        public float AverageAidedSteer { get; set; } = new();
    }

    public static class AgentBenchmarkDataUtils
    {
        public static Dictionary<string, float> ConvertToDictionary(ProcessedVehicleData data)
        {
            var dictionary = new Dictionary<string, float>();
            foreach (PropertyInfo property in typeof(ProcessedVehicleData).GetProperties())
            {
                if (property.PropertyType == typeof(float))
                {
                    var value = (float)property.GetValue(data);
                    dictionary.Add(property.Name, value);
                }
            }
            return dictionary;
        }
    }
}