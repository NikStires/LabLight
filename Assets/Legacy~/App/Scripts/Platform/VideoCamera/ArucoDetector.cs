using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections.Generic;
using UnityEngine;

public class DetectedAruco
{
    public int id;
    public Matrix4x4 pose;
}

/// <summary>
/// Detects a collection of Arucos in the given VideoFrames
/// </summary>
public class ArucoDetector
{
    private float markerLength = .0265f;

    bool initialFrame = true;

    Mat camMatrix;
    MatOfDouble distCoeffs;
    Mat imageInput;
    Mat image;
    DetectorParameters detectorParams;
    Dictionary dictionary;
    CharucoBoard board;

    Mat ids;
    List<Mat> corners;
    List<Mat> rejectedCorners;
    Mat rvecs;
    Mat tvecs;
    double[] rvecArr = new double[3];
    double[] tvecArr = new double[3];

    public ArucoDetector(float markerLength = .0265f)
    {
        this.markerLength = markerLength;

        // Do not reference plugin classes outside of main thread
        detectorParams = DetectorParameters.create();
        dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);

        ids = new Mat();
        corners = new List<Mat>();
        rejectedCorners = new List<Mat>();
        rvecs = new Mat();
        tvecs = new Mat();
    }

    private static Matrix4x4 rectify = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, -1));

    public List<DetectedAruco> DetectArucos(VideoFrame frame, FlipCode code, PixelFormat format)
    {
        if (initialFrame)
        {
            initialFrame = false;

            // Build the camera calibration matrix
            camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, frame.intrinsics.focalLength.x);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, frame.intrinsics.principalPoint.x);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, frame.intrinsics.focalLength.y);
            camMatrix.put(1, 2, frame.intrinsics.principalPoint.y);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);

            // Build the distortion matrix
            distCoeffs = new MatOfDouble(
                frame.intrinsics.radialDistortion.x,
                frame.intrinsics.radialDistortion.y,
                frame.intrinsics.tangentialDistortion.x,
                frame.intrinsics.tangentialDistortion.y,
                frame.intrinsics.radialDistortion.z
            );

            imageInput = new Mat(frame.intrinsics.resolution.y, frame.intrinsics.resolution.x, CvType.CV_8UC4);
            image = new Mat(frame.intrinsics.resolution.y, frame.intrinsics.resolution.x, CvType.CV_8UC1);
        }

        // Move bytes into Mat
        imageInput.put(0, 0, frame.image);

        // Convert to gray
        Imgproc.cvtColor(imageInput, image, PixelFormat.BGRA == format ? Imgproc.COLOR_BGRA2GRAY : Imgproc.COLOR_RGBA2GRAY);

        // Flip the image if needed
        if (FlipCode.None != code)
        {
            int ocvFlipCode = 0;    // Vertical is default
            if (FlipCode.Horizontal == code) ocvFlipCode = 1;
            else if (FlipCode.VerticalHorizontal == code) ocvFlipCode = -1;
            Core.flip(image, image, ocvFlipCode);
        }

        // Detect aruco markers
        Aruco.detectMarkers(image, dictionary, corners, ids, detectorParams, rejectedCorners, camMatrix, distCoeffs);

        // Estimate 3d pose for all corners
        Aruco.estimatePoseSingleMarkers(corners, markerLength, camMatrix, distCoeffs, rvecs, tvecs);

        List<DetectedAruco> list = new List<DetectedAruco>();

        for (int i = 0; i < ids.total(); i++)
        {
            PoseData poseData = ARUtils.ConvertRvecTvecToPoseData(rvecs.get(i,0), tvecs.get(i, 0));

            list.Add(new DetectedAruco()
            {
                id = (int)ids.get(i, 0)[0],
                pose = ARUtils.ConvertPoseDataToMatrix(ref poseData, false)
            }); 
        }

        return list;
    }
}