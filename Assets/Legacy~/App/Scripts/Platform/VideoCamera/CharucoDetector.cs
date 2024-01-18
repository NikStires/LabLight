using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections.Generic;
using UnityEngine;

public class CharucoDetector
{
    int numArucosNeeded = 1;
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
    Mat rotMat;
    Mat boardCorners;
    Mat boardIds;
    Mat rvec;
    Mat tvec;
    double[] rvecArr = new double[3];
    double[] tvecArr = new double[3];

    /// <summary>
    /// Constructor
    /// Default values are for a printed A4 Charuco
    /// </summary>
    /// <param name="minimumArucosNeeded">The minimum number of Arucos needed.</param>
    /// <param name="numSquaresX"></param>
    /// <param name="numSquaresY"></param>
    /// <param name="squareLengthMeters"></param>
    /// <param name="markerLengthMeters"></param>
    public CharucoDetector(int numArucosNeeded = 12, int numSquaresX = 5, int numSquaresY = 5, float squareLengthMeters = 0.04f, float markerLengthMeters = 0.03f, int predefinedDictionary = Aruco.DICT_4X4_50)
    {
        this.numArucosNeeded = numArucosNeeded;
        // Do not reference plugin classes outside of main thread
        detectorParams = DetectorParameters.create();

        dictionary = Aruco.getPredefinedDictionary(predefinedDictionary);
        board = CharucoBoard.create(numSquaresX, numSquaresY, squareLengthMeters, markerLengthMeters, dictionary);

        ids = new Mat();
        corners = new List<Mat>();
        rejectedCorners = new List<Mat>();
        rotMat = new Mat(3, 3, CvType.CV_64FC1);
        boardCorners = new Mat();
        boardIds = new Mat();
        rvec = new Mat();
        tvec = new Mat();
    }

    private static Matrix4x4 rectify = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, -1));

    public bool GetCharucoTransform(VideoFrame frame, FlipCode code, PixelFormat format, ref Matrix4x4 pose)
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
        // Aruco.refineDetectedMarkers (grayMat, charucoBoard, corners, ids, rejectedCorners, camMatrix, distCoeffs, 10f, 3f, true, recoveredIdxs, detectorParams);
        if (ids.total() == 0) return false;

        if (ids.total() != numArucosNeeded)
            return false;

        Aruco.interpolateCornersCharuco(corners, ids, image, board, boardCorners, boardIds, camMatrix, distCoeffs);
        bool valid = Aruco.estimatePoseCharucoBoard(boardCorners, boardIds, board, camMatrix, distCoeffs, rvec, tvec);
        if (!valid) return false;

        rvec.get(0, 0, rvecArr);
        tvec.get(0, 0, tvecArr);

        PoseData poseData = ARUtils.ConvertRvecTvecToPoseData(rvecArr, tvecArr);
        pose = ARUtils.ConvertPoseDataToMatrix(ref poseData, false);

        return true;
    }
}