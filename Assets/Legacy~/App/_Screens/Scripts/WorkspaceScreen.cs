using System;
using UniRx;

/// <summary>
/// Controller for displaying the active workspace.
/// Note this screen used to display an IP entry field to be able to change server IP inside HoloLens.
/// </summary>
public class WorkspaceScreen : ScreenViewController
{
    IDisposable sub;
    void GetWorkspace()
    {
        var workspaceProvider = ServiceRegistry.GetService<IWorkspaceProvider>();
        sub = workspaceProvider.GetWorkspace().Subscribe(workspace =>
        {
            SessionState.workspace = workspace;
            SessionManager.Instance.GotoScreen(ScreenType.Calibration);
        }, e =>
        {
            GetWorkspace();
            ServiceRegistry.Logger.Log("Cannot load workspace. Retrying...");
        });
    }

    private void OnEnable()
    {
        GetWorkspace();
    }

    private void OnDisable()
    {
        sub?.Dispose();
    }
}