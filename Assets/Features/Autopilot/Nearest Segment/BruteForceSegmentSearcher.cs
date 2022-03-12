﻿using Perrinn424.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class BruteForceSegmentSearcher : INearestSegmentSearcher
{

    private readonly IReadOnlyList<Vector3> path;
    public BruteForceSegmentSearcher(IReadOnlyList<Vector3> path)
    {
        this.path = path;
    }
    public int StartIndex { get; private set; }

    public int EndIndex { get; private set; }

    public void Search(Vector3 position)
    {
        float sqrtDistance = Mathf.Infinity;

        CircularIndex closestIndex = new CircularIndex(path.Count);
        for (int i = 0; i < path.Count; i++)
        {
            float tempDistance = (position - path[i]).sqrMagnitude;
            if (tempDistance < sqrtDistance)
            {
                sqrtDistance = tempDistance;
                closestIndex.Assign(i);
            }
        }

        Vector3 segmentVector = path[closestIndex + 1] - path[closestIndex];
        Vector3 pointVector = position - path[closestIndex];

        if(Vector3.Dot(segmentVector, pointVector) < 0)
        {
            closestIndex--;
        }

        StartIndex = closestIndex;
        EndIndex = closestIndex + 1;
    }

}
