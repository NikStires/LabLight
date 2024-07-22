using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for 
/// -media services
/// -calibration 
/// </summary>
public interface ILighthouseControl
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetBuildVersion();

    /// <summary>
    /// Base hostname for performing webrequests
    /// </summary>
    /// <returns></returns>
    public string GetFileServerUri();

    /// <summary>
    /// Was the Lighthouse server found?
    /// </summary>
    /// <returns></returns>
    public bool IsInitialized();

    /// <summary>Request start recording on lighthouse server.</summary>
    public void StartRecordingVideo();

    /// <summary>Request stop recording on lighthouse server.</summary>
    public void StopRecordingVideo();

    /// <summary>Request start timer on lighthouse server.</summary>
    public void StartTimer(int sec);

    /// <summary>Request stop timer on lighthouse server.</summary>
    public void StopTimer();

    /// <summary>Request Lighthouse to switch to certain detector mode
    public void DetectorMode(int detector, int mode, float fps);

    /// <summary>Request start playing on lighthouse server.</summary>
    public void StartPlayingVideo();

    /// <summary>Request stop playing on lighthouse server.</summary>
    public void StopPlayingVideo();

    /// <summary>Request recalibration on lighthouse server.</summary>
    public void RequestLighthouseCalibration(int align_type, int markerID);
    
    /// <summary>Request current Aruco settings on lighthouse server.</summary>
    public void RequestArucoSettings();

    /// <summary>
    /// Request reset of all tracked objects to prevent long living objects (like the chess objects) to live forever
    /// </summary>
    public void ResetTrackedObjects();
    public void Request_Deep_Models();

    /// <summary>
    /// Set the file recieve folder path on the lighthouse server
    /// </summary>
    public void SetFileRecieveFolder(string recieveFolderPath);

    /// <summary>
    /// set the current protocol state to be displayed on the lighthouse server
    /// </summary>
    public void SetProtocolStatus();
    public void ResetProtcolStatus();
}