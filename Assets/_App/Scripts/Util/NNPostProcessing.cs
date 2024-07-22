using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NNPostProcessing : MonoBehaviour
{
    public static Vector3 roundToCM(Vector3 vec) 
    {
        return new Vector3((float)Math.Round(vec.x, 2), (float)Math.Round(vec.y, 2), (float)Math.Round(vec.z, 2));
    }
    public static Vector3 [] roundToCM(Vector3 [] vecs)
    {
        for(int i = 0; i < vecs.Length; i++)
            vecs[i] = new Vector3((float)Math.Round(vecs[i].x, 2), (float)Math.Round(vecs[i].y, 2), (float)Math.Round(vecs[i].z, 2));

        return vecs;
    }

    public sealed class Vector3ExponentialMovingAverageCalculator
    {
        private readonly Vector3 _alpha;
        private Vector3 _lastAverage = Vector3.zero;

        public Vector3ExponentialMovingAverageCalculator(int lookBack) => _alpha = new Vector3(2f, 2f, 2f) / (lookBack + 1);

        public Vector3 NextValue(Vector3 value) => _lastAverage = _lastAverage == Vector3.zero
            ? value
            : Vector3.Scale((value - _lastAverage), _alpha) + _lastAverage;
    }
}