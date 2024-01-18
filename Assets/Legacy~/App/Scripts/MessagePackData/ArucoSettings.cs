using System.Runtime.Serialization;

namespace ACAM2.MessagePack
{
    public enum DetectorMode
    {
        SingleShot = 0,
        Frequency = 1,
        FreeRunning = 2
    }

    public enum TrackingMode
    {
        NoTracking = 0,
        Simple = 1,
        ImageFast = 2,
        ImageRobust = 3
    }

    public enum DictionaryType
    {
        charuco4x4_50 = 0,
        charuco5x5_50 = 1,
        charuco6x6_50 = 2,
        charuco7x7_50 = 3,
        original = 4,
        apriltag_16h5 = 5
    }

    public enum ArucoMode
    {
        Disabled = 0,
        ArucoMarkers = 1,
        CharucoBoards = 2
    }

    [DataContract]
    public class ArucoSettings
    {
        /// message packet version
        public byte PacketVersion { get; set; }
        // id of server version
        public uint ServerId { get; set; }
        public DetectorMode ModeDetector { get; set; }
        /// frequency (in Hz) when using detector_mode_frequency (for other modes this can be ignored)
        public float Fps { get; set; }
        public TrackingMode ModeTracking { get; set; }
        public DictionaryType DictionaryType { get; set; }
        /// number of squares on charuco board in horizontal direction
        public byte BoardNumX { get; set; }
        /// number of squares on charuco board in vertical direction
        public byte BoardNumY { get; set; }
        /// physical size of aruco marker in real world (measure outer black square of marker in millimeters)
        public float MarkerSize { get; set; }
        /// physical size of charuco board square in real world (measure a single black solid square (not marker) in millimeters)
        public float BoardSquareSize { get; set; }
        public ArucoMode ModeAruco { get; set; }

        public static explicit operator ArucoSettings(object[] fields) => new ArucoSettings()
        {
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            ModeDetector = (DetectorMode)(byte)fields[3],
            Fps = (float)fields[4],
            ModeTracking = (TrackingMode)(byte)fields[5],
            DictionaryType = (DictionaryType)(byte)fields[6],
            BoardNumX = (byte)fields[7],
            BoardNumY = (byte)fields[8],
            MarkerSize = (float)fields[9],
            BoardSquareSize = (float)fields[10],
            ModeAruco = (ArucoMode)(byte)fields[11]
        };
    }
}
