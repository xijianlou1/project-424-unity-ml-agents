using System.Collections.Generic;
using System.Linq;
using ML_Agents.Scripts;
using UnityEngine;
using VehiclePhysics.Timing;
using VehiclePhysics;
using VehicleData = ML_Agents.Scripts.VehicleData;

namespace Perrinn424
{
    public class AgentBenchmark : VehicleBehaviour
    {
        [SerializeField]
        LapTimer m_LapTimer;
        
        int m_LapCount;
        Battery m_battery;
        int m_SectorNumPerLap;
        List<List<float>> m_SectorTimeRecords = new();
        List<float> m_LapTimeRecords = new();
        float m_CurrentLapTime;
        float m_CurrentSectorTime;

        AgentBenchmarkData m_BenchmarkData = new();

        public override void OnEnableVehicle()
        {
            if (m_LapTimer != null)
            {
                m_SectorNumPerLap = m_LapTimer.sectors;
                m_LapTimer.onSector += OnSector;
                m_LapTimer.onBeginLap += LapBeginEventHandler;
            }

            m_SectorTimeRecords.Add(new List<float>());
        }

        void OnSector(int sector, float sectorTime)
        {
            // Process the sector data
            var lastProcessedSectorData = m_BenchmarkData.ProcessedSectorData[^1];
            var lastSectorData = m_BenchmarkData.SectorData[^1];
            // Use reflection to automate the properties
            var properties = typeof(ProcessedVehicleData).GetProperties();

            foreach (var property in properties)
            {
                var sourceProperty = typeof(VehicleData).GetProperty(property.Name.Replace("Average", ""));
                if (sourceProperty != null)
                {
                    if (sourceProperty.GetValue(lastSectorData) is IEnumerable<float> sourceValue)
                    {
                        property.SetValue(lastProcessedSectorData, sourceValue.Average());
                    }
                }
            }

            m_BenchmarkData.ProcessedSectorData[^1].Time = sectorTime;

            // Reset the sector data
            m_SectorTimeRecords[^1].Add(sectorTime);
            if (m_SectorTimeRecords[^1].Count == m_SectorNumPerLap)
            {
                m_LapTimeRecords.Add(m_SectorTimeRecords[^1].Sum());
                m_SectorTimeRecords.Add(new List<float>());
                m_LapCount++;
            }

            m_BenchmarkData.ProcessedSectorData.Add(new ProcessedVehicleData());
            m_BenchmarkData.SectorData.Add(new VehicleData());
        }

        public override void OnDisableVehicle()
        {
            if (m_LapTimer != null)
            {
                m_LapTimer.onSector -= OnSector;
            }
        }

        void LapBeginEventHandler()
        {

        }

        public override void FixedUpdateVehicle()
        {
            // FixedUpdateVehicle happens right after the vehicle simulation step, with all internal values updated.
            if (m_battery == null)
            {
                m_battery = vehicle.GetComponentInChildren<Battery>();
            }

            var vehicleData = vehicle.data.Get(Channel.Vehicle);
            var custom = vehicle.data.Get(Channel.Custom);
            if (m_LapTimer != null)
            {
                m_CurrentLapTime = m_LapTimer.currentLapTime;
                m_CurrentSectorTime = m_LapTimer.currentSectorTime;
            }
            
            // Vehicle data
            var speed = vehicleData[VehiclePhysics.VehicleData.Speed] / 1000.0f;
            m_BenchmarkData.SectorData[^1].Speed.Add(speed * 3.6f); // Speed data (km/h)
            m_BenchmarkData.SectorData[^1].EngineRpm.Add(vehicleData[VehiclePhysics.VehicleData.EngineRpm]);
            m_BenchmarkData.SectorData[^1].EngineTorque.Add(vehicleData[VehiclePhysics.VehicleData.EngineTorque]);
            m_BenchmarkData.SectorData[^1].EngineLoad.Add(vehicleData[VehiclePhysics.VehicleData.EngineLoad]);
            m_BenchmarkData.SectorData[^1].EnginePower.Add(vehicleData[VehiclePhysics.VehicleData.EnginePower]);
            m_BenchmarkData.SectorData[^1].AidedSteer.Add(vehicleData[VehiclePhysics.VehicleData.AidedSteer]);
        
            
            // Gear data
            var gearId = vehicleData[VehiclePhysics.VehicleData.GearboxGear];
            var switchingGear = vehicleData[VehiclePhysics.VehicleData.GearboxShifting] != 0;
            var gear = gearId switch
            {
                0 => switchingGear ? " " : "N",
                > 0 => "D",
                -1 => "R",
                _ => "R" + -gearId
            };
            m_BenchmarkData.SectorData[^1].Gear.Add(gear);
            
            // Battery data
            if (m_battery != null)
            {
                m_BenchmarkData.SectorData[^1].TotalElecPower.Add(m_battery.Power);
                m_BenchmarkData.SectorData[^1].BatterySOC.Add(m_battery.StateOfCharge * 100f);
                m_BenchmarkData.SectorData[^1].BatteryCapacity.Add(m_battery.NetEnergy);
            }
            
            // Controller data
            // TODO: confirm the data bus channel get updated before FixedUpdateVehicle
            m_BenchmarkData.SectorData[^1].ThrottlePosition.Add(custom[Perrinn424Data.ThrottlePosition]);
            m_BenchmarkData.SectorData[^1].BrakePosition.Add(custom[Perrinn424Data.BrakePosition]);
            m_BenchmarkData.SectorData[^1].SteeringAngle.Add(custom[Perrinn424Data.SteeringWheelAngle]);
            m_BenchmarkData.SectorData[^1].EngagedGear.Add(custom[Perrinn424Data.EngagedGear]);
            m_BenchmarkData.SectorData[^1].FrontPowertrain.Add(custom[Perrinn424Data.FrontDiffFriction]);
            m_BenchmarkData.SectorData[^1].RearPowertrain.Add(custom[Perrinn424Data.RearDiffFriction]);
            m_BenchmarkData.SectorData[^1].GroundTrackerFrontRideHeight.Add(custom[Perrinn424Data.FrontRideHeight]);
            m_BenchmarkData.SectorData[^1].GroundTrackerRearRideHeight.Add(custom[Perrinn424Data.RearRideHeight]);
            m_BenchmarkData.SectorData[^1].GroundTrackerFrontRollAngle.Add(custom[Perrinn424Data.FrontRollAngle]);
            m_BenchmarkData.SectorData[^1].GroundTrackerRearRollAngle.Add(custom[Perrinn424Data.RearRollAngle]);
        }
    }
}
