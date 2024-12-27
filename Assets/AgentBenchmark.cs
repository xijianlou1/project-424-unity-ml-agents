using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehiclePhysics.Timing;

public class AgentBenchmark : MonoBehaviour
{
    int m_SectorCount;
    LapTimer m_LapTimer;
    List<List<float>> m_SectorTimeRecords = new ();
    List<float> m_LapTimeRecords = new ();
    float m_CurrentLapTime;
    float m_CurrentSectorTime;

    void OnEnable()
    {
        m_LapTimer = FindObjectOfType<LapTimer>();
        if (m_LapTimer != null)
        {
            m_SectorCount = m_LapTimer.sectors;
            m_LapTimer.onSector += OnSector;
        }
        m_SectorTimeRecords.Add(new List<float>());
    }

    void OnSector(int sector, float sectorTime)
    {
        m_SectorTimeRecords[^1].Add(sectorTime);
        if (m_SectorTimeRecords[^1].Count == m_SectorCount)
        {
            Debug.Log("New lap");
            m_LapTimeRecords.Add(m_SectorTimeRecords[^1].Sum());
            m_SectorTimeRecords.Add(new List<float>());
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
        if (m_LapTimer != null)
        {
            m_CurrentLapTime = m_LapTimer.currentLapTime;
            m_CurrentSectorTime = m_LapTimer.currentSectorTime;
        }
    }
}
