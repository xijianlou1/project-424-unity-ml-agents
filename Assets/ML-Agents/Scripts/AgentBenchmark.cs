using System.Collections.Generic;
using System.Linq;
using ML_Agents.Scripts;
using Perrinn424;
using VehiclePhysics.Timing;
using VehiclePhysics;

public class AgentBenchmark : VehicleBehaviour
{
    int m_LapCount;
    Battery m_battery;
    
    int m_SectorCount;
    LapTimer m_LapTimer;
    List<List<float>> m_SectorTimeRecords = new ();
    List<float> m_LapTimeRecords = new ();
    float m_CurrentLapTime;
    float m_CurrentSectorTime;
    
    AgentBenchmarkData m_BenchmarkData = new ();

    void OnEnable()
    {
        m_LapTimer = FindObjectOfType<LapTimer>();
        if (m_LapTimer != null)
        {
            m_SectorCount = m_LapTimer.sectors;
            m_LapTimer.onSector += OnSector;
            m_LapTimer.onBeginLap += LapBeginEventHandler;
        }
        m_SectorTimeRecords.Add(new List<float>());
        
        m_battery = VehicleBase.vehicle.GetComponentInChildren<Battery>();
    }

    void OnSector(int sector, float sectorTime)
    {
        m_SectorTimeRecords[^1].Add(sectorTime);
        if (m_SectorTimeRecords[^1].Count == m_SectorCount)
        {
            m_LapTimeRecords.Add(m_SectorTimeRecords[^1].Sum());
            m_SectorTimeRecords.Add(new List<float>());
            m_LapCount++;
        }
    }

    void OnDisable()
    {
        if (m_LapTimer != null)
        {
            m_LapTimer.onSector -= OnSector;
        }
    }

    void LapBeginEventHandler()
    {
        var lastExtractedLarpData = m_BenchmarkData.ExtractedLarpData[^1];
        var lastLarpData = m_BenchmarkData.LarpData[^1];
        
        // Use reflection to automate the property setting
        var properties = typeof(ExtractedLarpData).GetProperties();

        foreach (var property in properties)
        {
            var sourceProperty = typeof(LarpData).GetProperty(property.Name.Replace("Average", ""));
            if (sourceProperty != null)
            {
                if (sourceProperty.GetValue(lastLarpData) is IEnumerable<float> sourceValue)
                {
                    property.SetValue(lastExtractedLarpData, sourceValue.Average());
                }
            }
        }
        
        m_BenchmarkData.LarpData.Add(new LarpData());
        m_BenchmarkData.ExtractedLarpData.Add(new ExtractedLarpData());
    }
    
    public override void FixedUpdateVehicle ()
    {
        // FixedUpdateVehicle happens right after the vehicle simulation step, with all internal values updated.
        
        var vehicleData = VehicleBase.vehicle.data.Get(Channel.Vehicle);
        var custom = VehicleBase.vehicle.data.Get(Channel.Custom);
        
        if (m_LapTimer != null)
        {
            m_CurrentLapTime = m_LapTimer.currentLapTime;
            m_CurrentSectorTime = m_LapTimer.currentSectorTime;
        }

        // Speed data (km/h)
        float speed = vehicleData[VehicleData.Speed] / 1000.0f;
        m_BenchmarkData.LarpData[^1].Speed.Add(speed * 3.6f);
        
        // Gear data
        var gearId = vehicleData[VehicleData.GearboxGear];
        var switchingGear = vehicleData[VehicleData.GearboxShifting] != 0;
        string gear = gearId switch
        {
            0 => switchingGear ? " " : "N",
            > 0 => "D",
            -1 => "R",
            _ => "R" + (-gearId).ToString()
        };
        
        m_BenchmarkData.LarpData[^1].Gear.Add(gear);
        
        // Battery data
        if (m_battery != null)
        {
            m_BenchmarkData.LarpData[^1].TotalElecPower.Add(m_battery.Power);
            m_BenchmarkData.LarpData[^1].BatterySOC.Add(m_battery.StateOfCharge * 100f);
            m_BenchmarkData.LarpData[^1].BatteryCapacity.Add(m_battery.NetEnergy);
        }
        
        // Controller data
        // TODO: confirm the data bus channel get updated before FixedUpdateVehicle
        m_BenchmarkData.LarpData[^1].ThrottlePosition.Add(custom[Perrinn424Data.ThrottlePosition]);
        m_BenchmarkData.LarpData[^1].BrakePosition.Add(custom[Perrinn424Data.BrakePosition]);
        m_BenchmarkData.LarpData[^1].SteeringAngle.Add(custom[Perrinn424Data.SteeringWheelAngle]);
        m_BenchmarkData.LarpData[^1].EngagedGear.Add(custom[Perrinn424Data.EngagedGear]);
        m_BenchmarkData.LarpData[^1].FrontPowertrain.Add(custom[Perrinn424Data.FrontDiffFriction]);
        m_BenchmarkData.LarpData[^1].RearPowertrain.Add(custom[Perrinn424Data.RearDiffFriction]);
        m_BenchmarkData.LarpData[^1].GroundTrackerFrontRideHeight.Add(custom[Perrinn424Data.FrontRideHeight]);
        m_BenchmarkData.LarpData[^1].GroundTrackerRearRideHeight.Add(custom[Perrinn424Data.RearRideHeight]);
        m_BenchmarkData.LarpData[^1].GroundTrackerFrontRollAngle.Add(custom[Perrinn424Data.FrontRollAngle]);
        m_BenchmarkData.LarpData[^1].GroundTrackerRearRollAngle.Add(custom[Perrinn424Data.RearRollAngle]);
    }
}
