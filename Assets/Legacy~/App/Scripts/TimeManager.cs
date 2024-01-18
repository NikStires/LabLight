using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private bool started;
    private bool done;
    private int startTime;
    private int msDuration;

    // Start is called before the first frame update
    void Initialize()
    {
        started = false;
        done = false;
    }
    void Update()
    {
    }
    public void startTimer(int duration) //Takes in duration as ms
    {
        msDuration = duration;
        startTime = Environment.TickCount & Int32.MaxValue;     //Environment.TickCount & Int32.MaxValue return time since start up as millisecond
        started = true;
        done = false;
    }

    public float asFloat(int decimalPlaces = 3)  // Returns time left as a float between 0-1 with 0 being not started and 1 being done. Pass in int of how many decimal places are needed for accuracy
    {
        int currTime = (Environment.TickCount & Int32.MaxValue) - startTime;
        return (float)System.Math.Round((double)(started == false ? (done == true ? 1f : 0f) : ((currTime < msDuration) == true ? ((float)currTime / (float)msDuration) : resetTimeFloat())), decimalPlaces);
    }

    public float asFloatInverse(int decimalPlaces = 3) // Returns time left as float between 1-0 with 1 being not started and 0 being done.
    {
        return 1f - asFloat(decimalPlaces);
    }

    public int msPassed()           //Returns milliseconds passed since timer started
    {
        int currTime = (Environment.TickCount & Int32.MaxValue) - startTime;
        return (started == false ? (done == true ? msDuration : 0) : ((currTime < msDuration) == true ? currTime : resetTimeInt()));
    }

    public int msRemaining()        //Returns milliseconds remaining since timer started
    {
        return msDuration - msPassed();
    }
    public bool isDone()        //Returns false if not started or not done, true if done
    {
        return (started == false ? done : (((Environment.TickCount & Int32.MaxValue) - startTime) < msDuration) == true ? false : resetTime());
    }

    private int resetTimeInt()
    {
        started = false;
        done = true;
        return 1;
    }

    private float resetTimeFloat()
    {
        started = false;
        done = true;
        return 1f;
    }

    private bool resetTime()
    {
        started = false;
        done = true;
        return true;
    }

    public void hardReset()
    {
        started = false;
        done = false;
        msDuration = 0;
    }
}