using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;

/// <summary>
/// Stub to handle the transition from the legacy API
/// 
/// </summary>
public class StubbedWorkspaceProvider : IWorkspaceProvider
{
    public IObservable<WorkspaceFrame> GetWorkspace()
    {
        return Observable.FromCoroutine<WorkspaceFrame>((observer, cancellation) => CreateWorkspaceCoroutine(observer, cancellation));
    }

    private static IEnumerator CreateWorkspaceCoroutine(IObserver<WorkspaceFrame> observer, CancellationToken cancel)
    {
        yield return 0;

        // Create dummy WorkspaceFrame
        var ws = new WorkspaceFrame();
        ws.border = new List<Vector2>() { new Vector2(-.79f, .255f), new Vector2(.79f, .255f), new Vector2(.79f, -.745f), new Vector2(-.79f, -.745f), };
        ws.cameraPosition = new Vector3(0, 1, 0);
        observer.OnNext(ws);
        observer.OnCompleted();
    }
}
