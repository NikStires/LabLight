using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

/// <summary>
/// Monobehaviour for rendering workspace boundaries. 
/// CalibrationScreen uses the static methods to generate the lines and mesh in Unity.
/// </summary>
public class Workspace : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    [Tooltip("Dynamically generated mesh that covers the workspace")]
    public MeshFilter meshFilter;
    private Action disposeVoice;

    private void Awake()
    {
        SessionState.ShowGrid.Subscribe(value =>
        {
            if (value)
            {
                GridAnimationIn();
            }
            else
            {
                GridAnimationOut();
            }
        }).AddTo(this);
    }

    private void OnEnable()
    {
        meshFilter.mesh = null;          // remove grid mesh
        meshRenderer.material.SetFloat("_ThresholdX", 0);
        meshRenderer.material.SetFloat("_ThresholdY", 0);

        // Build workspace
        var workspaceFrame = SessionState.workspace;
        if (workspaceFrame != null)
        {
            var mesh = Workspace.BuildWorkspace(workspaceFrame.border);
            meshFilter.mesh = mesh;

            // Animate workspace
            GridAnimationIn();
        }
        else
        {
            Debug.LogWarning("No workspace in the SessionState. This must mean that the WorkspaceScreen never was activated.");
        }
        SetupVoiceCommands();

    }

    private void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;

        // Setup voice
        var commands = new Dictionary<string, Action>();

        commands.Add("flash grid", () =>
        {
            GridAnimationFlash();
        });

        commands.Add("hide grid", () =>
        {
            GridAnimationOut();
            SessionState.ShowGrid.Value = false;
        });

        commands.Add("show grid", () =>
        {
            GridAnimationIn();
            SessionState.ShowGrid.Value = true;
        });


        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(commands);
    }

    private void GridAnimationIn()
    {
        Vector2 target = InitializeMaterial();

        LeanTween.value(meshRenderer.gameObject, Vector2.zero, target, 3).setEaseInOutQuint().setOnUpdate((Vector2 val) =>
        {
            meshRenderer.material.SetFloat("_ThresholdX", val.x);
            meshRenderer.material.SetFloat("_ThresholdY", val.y);
        });
    }

    private void GridAnimationOut()
    {
        Vector2 target = InitializeMaterial();

        LeanTween.value(meshRenderer.gameObject, target, Vector2.zero, 3).setEaseInOutQuint().setOnUpdate((Vector2 val) =>
        {
            meshRenderer.material.SetFloat("_ThresholdX", val.x);
            meshRenderer.material.SetFloat("_ThresholdY", val.y);
        });
    }

    private void GridAnimationFlash()
    {
        Vector2 target = InitializeMaterial();

        LeanTween.value(meshRenderer.gameObject, Vector2.zero, target, 1).setEaseInOutQuint().setOnUpdate((Vector2 val) =>
        {
            meshRenderer.material.SetFloat("_ThresholdX", val.x);
            meshRenderer.material.SetFloat("_ThresholdY", val.y);
        }).setRepeat(3);
    }

    private Vector2 InitializeMaterial()
    {
        var border = SessionState.workspace.border;

        // Compute tiling to match charuco
        var bounds = Workspace.GetBounds(border);

        var charucoSquareLength = .020f;
        var tileRatio = 1f / charucoSquareLength;

        // Compute tiling
        var width = bounds.maxX - bounds.minX;
        var height = bounds.maxY - bounds.minY;
        meshRenderer.material.mainTextureScale = new Vector2(width * tileRatio, height * tileRatio);

        // Compute offset
        var xSquares = Mathf.Abs(bounds.minX) / charucoSquareLength;
        var xOff = Mathf.Ceil(xSquares) - xSquares;

        var ySquares = Mathf.Abs(bounds.minY) / charucoSquareLength;
        var yOff = Mathf.Ceil(ySquares) - ySquares;
        meshRenderer.material.mainTextureOffset = new Vector2(xOff, yOff);

        meshRenderer.material.SetFloat("_CenterX", (bounds.maxX + bounds.minX) / 2f);
        meshRenderer.material.SetFloat("_CenterY", (bounds.maxY + bounds.minY) / 2f);

        return new Vector2(.5f * width, .5f * height);
    }

    public struct Bounds
    {
        public float minX, minY, maxX, maxY;
    }

    public static Bounds GetBounds(List<Vector2> points)
    {
        var bounds = new Bounds()
        {
            minX = points[0].x,
            maxX = points[0].x,
            minY = points[0].y,
            maxY = points[0].y
        };

        for (int i = 1; i < points.Count; i++)
        {
            var p = points[i];
            if (p.x < bounds.minX) bounds.minX = (float)p.x;
            else if (p.x > bounds.maxX) bounds.maxX = (float)p.x;

            if (p.y < bounds.minY) bounds.minY = (float)p.y;
            else if (p.y > bounds.maxY) bounds.maxY = (float)p.y;
        }

        return bounds;
    }

    public static Mesh BuildWorkspace(List<Vector2> convexPoints)
    {
        // Clockwise winding, fan the convex shape
        var triangles = new int[(convexPoints.Count - 2) * 3];
        var t = 0;
        for (var i = 2; i < convexPoints.Count; i++)
        {
            triangles[t++] = 0;
            triangles[t++] = i - 1;
            triangles[t++] = i;
        }

        var bounds = GetBounds(convexPoints);
        var xRange = bounds.maxX - bounds.minX;
        var yRange = bounds.maxY - bounds.minY;
        var xOff = bounds.minX;
        var yOff = bounds.minY;

        var mesh = new Mesh();
        mesh.vertices = convexPoints.Select(b => new Vector3(b.x, 0, b.y)).ToArray();
        mesh.triangles = triangles;
        mesh.uv = convexPoints.Select(b => new Vector2((b.x - xOff) / xRange, (b.y - yOff) / yRange)).ToArray();
        mesh.RecalculateNormals();
        mesh.Optimize();
        return mesh;
    }
}
