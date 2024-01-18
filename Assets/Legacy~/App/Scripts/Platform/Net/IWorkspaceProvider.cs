using System;

/// <summary>
/// Interface for accessing workspace
/// </summary>
public interface IWorkspaceProvider
{
    /// <summary>
    /// Retrieve workspace boundaries
    /// </summary>
    /// <returns></returns>
    IObservable<WorkspaceFrame> GetWorkspace();
}