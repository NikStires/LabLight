using System;
using System.Collections.Generic;

/// <summary>
/// Interface for updating shared state between multiple clients
/// </summary>
public interface ISharedStateController
{
    /// <summary>
    /// Set given deviceId as master device (the device that controls the active step)
    /// </summary>
    /// <param name="deviceId"></param>
    void SetMaster(string deviceId);

    /// <summary>
    /// Sets the active procedure
    /// </summary>
    /// <param name="deviceId">The id of the device that sent the command</param>
    /// <param name="procedureName">Name of the procedure</param>
    void SetProcedure(string deviceId, string procedureName);

    /// <summary>
    /// Sets the current step in the active procedure
    /// </summary>
    /// <param name="deviceId">The id of the device that sent the command</param>
    /// <param name="step">Number of the step</param>
    void SetStep(string deviceId, int step);

    /// <summary>
    /// Sets the current substep in the active step
    /// </summary>
    /// <param name="deviceId">The id of the device that sent the command</param>
    /// <param name="step">Number of the substep</param>
    void SetSubStep(string deviceId, int substep);

    /// <summary>
    /// Sets the current checkitem in the active step
    /// </summary>
    /// <param name="deviceId">The id of the device that sent the command</param>
    /// <param name="step">Number of the substep</param>
    void SetCheckItem(string deviceId, int checkitem);
}