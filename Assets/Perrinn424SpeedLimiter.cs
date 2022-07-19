﻿using VehiclePhysics;
using UnityEngine;
using System;


public class Perrinn424SpeedLimiter : VehicleBehaviour
{

    [System.Serializable]
    public class SpeedLimiterArray
    {
        public float segmentStart = 0.0f;
        public float segmentEnd = 500.0f;
        // set range for minimum limiter -- 0 to 1
        [Range(0, 1)]
        public float minimumLimiter = 1.0f;
    }

    // Emit telemetry checkbox
    public bool emitTelemetry = true;

    // Speed limiter segments configuration
    [Space(5)]
    [SerializeField] private SpeedLimiterArray[] speedLimiterSegment;

    [HideInInspector] public float limiterValue = 1.0f;
    [HideInInspector] public float limiterEnabled = 0.0f;

    float getLimiterValue(float lapDistance)
    {
        for (int i = 0; i < speedLimiterSegment.Length; i++)
        {
            if (lapDistance > speedLimiterSegment[i].segmentStart && lapDistance < speedLimiterSegment[i].segmentEnd)
                return speedLimiterSegment[i].minimumLimiter;
        }
        return 1.0f;
    }

    public override void FixedUpdateVehicle()
    {
        // Getting traveled distance in current lap
        Telemetry.DataRow telemetryDataRow = vehicle.telemetry.latest;
        float distance = (float)telemetryDataRow.distance;

        // check if car is in limited power segment and return the limiter value
        limiterValue = getLimiterValue(distance);

        // check if limiter is enabled
        limiterEnabled = 0.0f;
        if (limiterValue < 1.0f)
            limiterEnabled = 1.0f;

    }


    // Telemetry
    public override bool EmitTelemetry()
    {
        return emitTelemetry;
    }


    public override void RegisterTelemetry()
    {
        vehicle.telemetry.Register<Perrinn424SpeedLimiterTelemetry>(this);
    }


    public override void UnregisterTelemetry()
    {
        vehicle.telemetry.Unregister<Perrinn424SpeedLimiterTelemetry>(this);
    }


    public class Perrinn424SpeedLimiterTelemetry : Telemetry.ChannelGroup
    {
        public override int GetChannelCount()
        {
            return 2;
        }


        public override Telemetry.PollFrequency GetPollFrequency()
        {
            return Telemetry.PollFrequency.Normal;
        }
    }

    
    public void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, UnityEngine.Object instance)
    {

        // Fill-in channel information

        channelInfo[0].SetNameAndSemantic("SpeedLimiterActive", Telemetry.Semantic.Ratio);
        channelInfo[1].SetNameAndSemantic("LimiterValue", Telemetry.Semantic.Ratio);
    }


    public void PollValues(float[] values, int index, UnityEngine.Object instance)
    {
        Perrinn424SpeedLimiter speedLimiter = instance as Perrinn424SpeedLimiter;

        values[index + 0] = speedLimiter.limiterEnabled;
        values[index + 1] = speedLimiter.limiterValue;

    }

}
