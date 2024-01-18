using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stubbed producer of NetStateFrames. 
/// Loops back the procedure, step etc that are set through the ISharedStateController interface
/// </summary>
public class StubbedNetworkFrameProducer : INetworkFrameProducer, ISharedStateController
{
    public string master;
    private string procedure;
    private int step;
    private int subStep;
    private int checkItem;

    private PositionRotation screen;

    public StubbedNetworkFrameProducer()
    {
        // Positions the UI screen wrt to the Charuco
        screen = new PositionRotation()
        {
            lookForward = Vector3.back,
            position = new Vector3(0, .25f, 0)
        };
    }

    public List<NetStateFrame> GetAndClearFrames()
    {
        var retList = new List<NetStateFrame>();

        retList.Add(new NetStateFrame()
        {
            master = this.master,
            procedure = this.procedure,
            step = this.step,
            screen = this.screen
        });

        return retList;
    }

    public void SetMaster(string deviceId)
    {
        this.master = deviceId;
    }

    public void SetProcedure(string deviceId, string procedureName)
    {
        this.procedure = procedureName;
    }

    public void SetStep(string deviceId, int step)
    {
        this.step = step;
    }

    public void SetSubStep(string deviceId, int subStep)
    {
        this.subStep = subStep;
    }

    public void SetCheckItem(string deviceId, int checkItem)
    {
        this.checkItem = checkItem;
    }
}
