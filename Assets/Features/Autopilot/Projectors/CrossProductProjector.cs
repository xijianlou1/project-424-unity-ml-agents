﻿using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class CrossProductProjector : IProjector
    {
        public (Vector3, float) Project(Transform t, Vector3 start, Vector3 end)
        {
            Vector3 segment = end - start;
            Vector3 pointVector = t.position - start;

            float dotProductPoint = Vector3.Dot(segment, pointVector);
            float ratio = dotProductPoint / segment.sqrMagnitude;
            Vector3 projectedPosition = start + segment * ratio;

            return (projectedPosition, ratio);
        }
    }
}
