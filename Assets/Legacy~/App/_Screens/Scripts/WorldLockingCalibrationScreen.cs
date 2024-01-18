using OpenCVForUnity.UnityUtils;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using Application = UnityEngine.WSA.Application;

/// <summary>
/// Creates a CharucoDetector that reacts to VideoFrames coming from the current IVideoCamera 
/// Iterates through multiple SpaceSpins as configured 
/// 
/// Not self contained because the SpacePins are not part of this screen
/// Should we make this configurable in a separate configuration screen?
/// </summary>
public class WorldLockingCalibrationScreen : ScreenViewController
{ 
    public TextMeshPro calibrationText;

    [Serializable]
    public class SpaceSpinDescriptor
    {
        public string HelperString;
        public SimpleSpacePinHandler spacePin;
    }

    public List<SpaceSpinDescriptor> spacePins;

    CharucoDetector detector;
    Matrix4x4 pose;
    IDisposable cameraSub;

    // Charuco conversion matrices
    private static Matrix4x4 invY = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
    private static Matrix4x4 invZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
    private static Matrix4x4 xRot = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));

    private int spacePinIndex = 0;

    private void OnEnable()
    {
        // // Setup detector for Charucos coasters
        // detector = new CharucoDetector(4, 3, 3, 0.04f, 0.031f);

        //setup detector for 4x4 40mm charuco
        detector = new CharucoDetector(12, 4, 4, 0.04f, 0.031f);
        pose = new Matrix4x4();

        spacePinIndex = 0;
        SetupCameraSubscription();
    }

    private void SetupCameraSubscription()
    {
        if (spacePins.Count == 0)
        {
            Debug.LogWarning("No SpacePins specified in the WorldLockingCalibration screen, yet this screen is made active. Disable Calibrate World Locking in the SessionManager.");
            return;
        }

        calibrationText.text = spacePins[spacePinIndex].HelperString;

        var camera = ServiceRegistry.GetService<IVideoCamera>();
        // Note: Callback can be on a background thread
        cameraSub = camera.GetFrames().Subscribe(frame =>
        {
            // Find Charuco
            var found = detector.GetCharucoTransform(frame, camera.Flip, camera.Format, ref pose);

            if (found)
            {
                Debug.Log("Found corner charuco");

                // Transform to unity coords (correctly this is a copy owned by main thread)
#if UNITY_EDITOR
                var charucoTransform = frame.camera2World * invY * pose * invY;
#else
            var charucoTransform = invZ * frame.camera2World * invZ * invY * pose * invY * xRot;
#endif

                StopCamera();

                // Ensure execution on main thread
                Application.InvokeOnAppThread(() => calibrationComplete(charucoTransform), false);
            }
        });
    }

    private void OnDisable()
    {
        StopCamera();
    }

    void calibrationComplete(Matrix4x4 charucoTransform)
    {
        ServiceRegistry.GetService<IAudio>()?.Play(AudioEventEnum.CalibrationCompleted);

        if (spacePinIndex < spacePins.Count)
        {
            spacePins[spacePinIndex].spacePin.gameObject.SetActive(true);
            // Set the spacepin coordinate frame
            ARUtils.SetTransformFromMatrix(spacePins[spacePinIndex].spacePin.transform, ref charucoTransform);
            // Update WLT
            spacePins[spacePinIndex].spacePin.UpdateSpacePin();
        }

        Observable.Timer(TimeSpan.FromMilliseconds(700)).Subscribe(_ =>
        {
            spacePinIndex++;

            Observable.Timer(TimeSpan.FromMilliseconds(2000)).Subscribe(_ =>
            {
                if (spacePinIndex < spacePins.Count)
                {
                    SetupCameraSubscription();
                }
                else
                {
                    SessionManager.Instance.GoBack();
                }
            });
        });
    }

    void StopCamera()
    {
        cameraSub?.Dispose();
        cameraSub = null;
    }

    /// <summary>
    /// Closes calibration
    /// </summary>
    public void CancelCalibration()
    {
        SessionManager.Instance.GoBack();
    }
}