using ACAM2.MessagePack;
using Microsoft.MixedReality.WorldLocking.Core;
using OpenCVForUnity.ArucoModule;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Application = UnityEngine.WSA.Application;

/// <summary>
/// Creates a CharucoDetector that reacts to VideoFrames coming from the current IVideoCamera.
/// -Shows calibration instruction
/// -Sets the charucoFrame transform to the Charuco detected by OpenCV.
/// -Close
/// </summary>
public class CalibrationScreen : ScreenViewController
{
    private const int lighthouseArucoSettingsTimeoutInMilliseconds = 5000;
    public TMPro.TextMeshPro Text;

    CharucoDetector detector;
    Matrix4x4 pose;
    IDisposable cameraSub;
    int foundFrames;

    // Charuco conversion matrices
    private static Matrix4x4 invY = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
    private static Matrix4x4 invZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
    private static Matrix4x4 xRot = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));

    private async void OnEnable()
    {
        Assert.IsNotNull(SessionState.workspace);

        SessionState.ArucoSettings.Subscribe(_ => { HandleUpdatedCharucoSettings(); }).AddTo(this);
        var task = UpdateArucoSettingAsync();

        // We need to wait for Lighthouse to be connected before we can request it's aruco settings
        if (await Task.WhenAny(task, Task.Delay(lighthouseArucoSettingsTimeoutInMilliseconds)) == task)
        {
        }
        else
        {
            Debug.LogWarning("Could not retrieve Charuco settings from Lighthouse, using locally stored setting instead.");
        }

        Recalibrate();
    }

    private bool arucoSettingsReceived = false;
    private async Task UpdateArucoSettingAsync()
    {
        ILighthouseControl lighthouseControl = ServiceRegistry.GetService<ILighthouseControl>();

        // Wait for connection, note that this method will be timed out
        while (!lighthouseControl.IsInitialized())
        {
            await Task.Delay(50);
        }

        // Request aruco settings
        arucoSettingsReceived = false;
        lighthouseControl.RequestArucoSettings();

        // Wait for updated aruco settings
        while (!arucoSettingsReceived)
        {
            await Task.Delay(50);
        }
    }

    private void HandleUpdatedCharucoSettings()
    {
        if (SessionState.ArucoSettings.Value != null)
        {
            Text.text = SessionState.ArucoSettings.Value.DictionaryType.ToString() +
                        "\nBoard Num X " + SessionState.ArucoSettings.Value.BoardNumX +
                        "\nBoard Num Y " + SessionState.ArucoSettings.Value.BoardNumY;

            SessionState.LastUsedArucoSettings = SessionState.ArucoSettings.Value;
            SessionState.CalibrationDirty.Value = false;

            arucoSettingsReceived = true;
        }
    }

    private void Recalibrate()
    {
        WorldLockingManager.GetInstance().Reset();
        WorldLockingManager.GetInstance().Save();

#if UNITY_EDITOR
        // Stubbed IVideoCamera image only works with the default settings
        detector = new CharucoDetector();
#else
        int minArucosNeeded = (int)Math.Floor(((float)SessionState.ArucoSettings.Value.BoardNumX * (float)SessionState.ArucoSettings.Value.BoardNumY) / 2f);
        detector = new CharucoDetector( minArucosNeeded,
                                        SessionState.ArucoSettings.Value.BoardNumX,
                                        SessionState.ArucoSettings.Value.BoardNumY,
                                        SessionState.ArucoSettings.Value.BoardSquareSize / 1000,
                                        SessionState.ArucoSettings.Value.BoardSquareSize / 1000 * .75f,
                                        LookupPredefinedDictionary());
#endif

        Debug.Log($"Current Charuco Settings: {SessionState.ArucoSettings.Value.BoardNumX}x{SessionState.ArucoSettings.Value.BoardNumY} {SessionState.ArucoSettings.Value.BoardSquareSize}");


        if (SessionState.ArucoSettings.Value != null)
        {
            Text.text = SessionState.ArucoSettings.Value.DictionaryType.ToString() +
                        "\nBoard Num X " + SessionState.ArucoSettings.Value.BoardNumX +
                        "\nBoard Num Y " + SessionState.ArucoSettings.Value.BoardNumY;

            SessionState.LastUsedArucoSettings = SessionState.ArucoSettings.Value;
            SessionState.CalibrationDirty.Value = false;
        }
        
        pose = new Matrix4x4();
        foundFrames = 0;

        var camera = ServiceRegistry.GetService<IVideoCamera>();
        // Note: Callback can be on a background thread
        cameraSub = camera.GetFrames().Subscribe(frame =>
        {
            // Find Charuco
            var found = detector.GetCharucoTransform(frame, camera.Flip, camera.Format, ref pose);

            if (found)
            {
                foundFrames++;

                if (found)
                {
                    Debug.Log("Found Charuco");
                }
            }
            else
            {
                // Reset if not found
                foundFrames = 0;
            }

            // Must find for consecutive number of frames before view updates to position
            if (foundFrames < 3)
                return;

            // Transform to unity coords (correctly this is a copy owned by main thread)
#if UNITY_EDITOR
            var charucoTransform = frame.camera2World * invY * pose * invY;
#else
      var charucoTransform = invZ * frame.camera2World * invZ * invY * pose * invY * xRot;
#endif

            StopCamera();

            // Ensure execution on main thread
            Application.InvokeOnAppThread(() => calibrationComplete(charucoTransform), false);
        }).AddTo(this);
    }


    private int LookupPredefinedDictionary()
    {
        switch (SessionState.ArucoSettings.Value.DictionaryType)
        {
            case DictionaryType.charuco4x4_50:
                return Aruco.DICT_4X4_50;
            case DictionaryType.charuco5x5_50:
                return Aruco.DICT_5X5_50;
            case DictionaryType.charuco6x6_50:
                return Aruco.DICT_6X6_50;
            case DictionaryType.charuco7x7_50:
                return Aruco.DICT_7X7_50;
            case DictionaryType.original:
                return Aruco.DICT_ARUCO_ORIGINAL;
            case DictionaryType.apriltag_16h5:
                return Aruco.DICT_APRILTAG_16h5;
            default:
                break;
        }
        return 0;
    }


    private void OnDisable()
    {
        StopCamera();
    }

    void calibrationComplete(Matrix4x4 charucoPose)
    {
        ServiceRegistry.Logger.Log("Calibration complete");

        ServiceRegistry.GetService<ILighthouseControl>()?.RequestLighthouseCalibration();

        ServiceRegistry.GetService<IAudio>()?.Play(AudioEventEnum.CalibrationCompleted);
        SessionManager.Instance.UpdateCalibration(charucoPose);

        Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ =>
        {
            if (SessionState.MultiplePinLocking.Value)
            {
                SessionManager.Instance.GotoScreen(ScreenType.WorldLocking);
            }
            else
            {
                SessionManager.Instance.GoBack();
            }
        });
    }
  
    void StopCamera()
    {
        Debug.Log("Stopping camera");
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

    /// <summary>
    /// Skips camera based coordinate frame alignment and uses a fixed position in front of the user
    /// Useful for quick testing 
    /// </summary>
    public void PlaceInView()
    {
        Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 0.6f;
        var rotationEuler = Camera.main.transform.rotation.eulerAngles;

        SessionManager.Instance.UpdateCalibration(Matrix4x4.TRS(position, Quaternion.Euler(0, rotationEuler.y + 180, 0), Vector3.one));
        SessionManager.Instance.GoBack();
    }
}