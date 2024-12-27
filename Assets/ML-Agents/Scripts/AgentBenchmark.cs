using System;
using System.Collections.Generic;
using System.Linq;
using Perrinn424;
using UnityEngine;
using VehiclePhysics.Timing;
using VehiclePhysics;

public class AgentBenchmark : MonoBehaviour
{
    int m_LapCount;
    Battery m_battery;
    
    int m_SectorCount;
    LapTimer m_LapTimer;
    List<List<float>> m_SectorTimeRecords = new ();
    List<float> m_LapTimeRecords = new ();
    float m_CurrentLapTime;
    float m_CurrentSectorTime;
    
    Dictionary<string,List<Tuple<int, string>>> m_BenchmarkData = new ();

    void OnEnable()
    {
        m_LapTimer = FindObjectOfType<LapTimer>();
        if (m_LapTimer != null)
        {
            m_SectorCount = m_LapTimer.sectors;
            m_LapTimer.onSector += OnSector;
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
    
    void Update()
    {
        var vehicleData = VehicleBase.vehicle.data.Get(Channel.Vehicle);
        var custom = VehicleBase.vehicle.data.Get(Channel.Custom);
        
        if (m_LapTimer != null)
        {
            m_CurrentLapTime = m_LapTimer.currentLapTime;
            m_CurrentSectorTime = m_LapTimer.currentSectorTime;
        }

        // Speed data (km/h)
        float speed = vehicleData[VehicleData.Speed] / 1000.0f;
        m_BenchmarkData["Speed"].Add(new Tuple<int, string>(m_LapCount, (speed * 3.6f).ToString("0")));
        
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
        m_BenchmarkData["Gear"].Add(new Tuple<int, string>(m_LapCount, gear));
        
        // Battery data
        // VehicleBase.vehicle.data.Get(Channel.Custom, );
    }
}
