using System;
using UnityEngine;
using VehiclePhysics.Timing;
using VehiclePhysics;

public class VehiclePhysicsEstimator : VehicleBehaviour
{
    [SerializeField] LapTimer m_LapTimer;
    
    public Action OnLapComplete;
    
    internal float m_CurrentSteeringAngle;
    
    int m_SectorNum;
    int m_SectorNumPerLap;
    
    public override void OnEnableVehicle()
    {
        if (m_LapTimer != null)
        {
            m_LapTimer.onSector += OnSector;
            m_SectorNumPerLap = m_LapTimer.sectors;
        }
    }

    void OnSector(int sector, float sectorTime)
    {
        m_SectorNum++;

        if (m_SectorNum == m_SectorNumPerLap)
        {
            OnLapComplete?.Invoke();
        }

    }

    public override void FixedUpdateVehicle()
    {
        // FixedUpdateVehicle happens right after the vehicle simulation step, with all internal values updated.
        var custom = vehicle.data.Get(Channel.Custom);

        m_CurrentSteeringAngle = custom[Perrinn424Data.SteeringWheelAngle] / 1000.0f;
    }
}
