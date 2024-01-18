using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Oscillate : MonoBehaviour
{
    
    [Range(0f, 10f)] public float userOscillationFrequency;

    [Range(1f,20f)] public float maxStepRatio; //can be thought of as the amplitude 

    private float oscillationFrequency;

    enum Axis {X, Y, Z}

    [SerializeField] Axis oscillateMode;

    private Transform objectToOscillate;

    private float startPos;
    private float maxStep;
    private float cosVal;
    
    // Start is called before the first frame update
    void Start()
    {
        cosVal = 0;
        oscillationFrequency = userOscillationFrequency;
        objectToOscillate = this.transform;

        if (oscillateMode == Axis.X)
            startPos = objectToOscillate.localPosition.x;
        else if (oscillateMode == Axis.Y)
            startPos = objectToOscillate.localPosition.y;
        else if (oscillateMode == Axis.Z)
            startPos = objectToOscillate.localPosition.z;

        //maxStep notes the maximum distance a given object can oscillate. 
        //This is done as a ratio to the starting position, such that the maximum step is some fraction of the starting position of an object
        maxStep = startPos / maxStepRatio;
    }

    void OnEnable()
    {
        cosVal = 0;
    }

    // Update is called once per frame
    void Update()
    {
        cosVal = cosVal + 0.01f;
        float currStep = (((float)System.Math.Sin(cosVal * oscillationFrequency) + 1) * maxStep) + (startPos-maxStep);
        objectToOscillate.localPosition = setPosition(oscillateMode, currStep);
    }
    private void setFrequency(float freq)
    {
        oscillationFrequency = freq;
    }
    private Vector3 setPosition(Axis oscillateMode, float currStep)
    {
        Vector3 temp;
        if(oscillateMode == Axis.X)
            temp = new Vector3(currStep, objectToOscillate.localPosition.y, objectToOscillate.localPosition.z);
        else if(oscillateMode == Axis.Y)
            temp = new Vector3(objectToOscillate.localPosition.x, currStep, objectToOscillate.localPosition.z);
        else
            temp = new Vector3(objectToOscillate.localPosition.x, objectToOscillate.localPosition.y, currStep);
        return temp;
    }
}
