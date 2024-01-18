using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    private Material cellWell;
    private Color cellColor;

    [Range(0.0f, 10.0f)] public float pulseFrequency = 4.4f;

    [Range(0f, 1f)] public float min = 0f;

    [Range(0f, 1f)] public float max = 1f;

    private float cosVal;
    // Start is called before the first frame update
    void Start()
    {
        cellWell = this.GetComponent<Renderer>().material;
        cellColor = cellWell.GetColor("_Color");
        cellColor.a = 255;
    }

    void OnEnable()
    {
        cosVal = 0;
    }

    // Update is called once per frame
    void Update()
    {
        cellColor = cellWell.GetColor("_Color");
        cosVal = cosVal + 0.01f;
        cellColor.a = min + (max - min) * (((float)System.Math.Cos(cosVal * pulseFrequency) + 1) / 2);
        cellWell.SetColor("_Color", cellColor);
    }
    void OnDisable()
    {
        cellColor.a = 255;
    }
    void OnDestroy()
    {
        cellColor.a = 255;
    }

    private void setFreq(float freq)
    {
        pulseFrequency = freq > 0 ? freq : 1;
    }
}
