using System.Runtime.Serialization;
namespace Lighthouse.MessagePack
{
    [DataContract]
    public class CharucoBoard
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public byte SensorId { get; set; }
        public double TimeStamp { get; set; }
        public float SquareLength { get; set; }
        public byte BoardNumX { get; set; }
        public byte BoardNumY { get; set; }
        public UnityEngine.Vector3 Position { get; set; }
        public UnityEngine.Quaternion Rotation { get; set; }
        public UnityEngine.Vector3 EulerAngle { get; set; }
        public UnityEngine.Matrix4x4 Matrix {get;set;}

        public static explicit operator CharucoBoard(object[] fields) => new CharucoBoard()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            SensorId = (byte)fields[3],
            TimeStamp = (double)fields[4],
            SquareLength = (float)fields[5],
            BoardNumX = (byte)fields[6],
            BoardNumY = (byte)fields[7],
            Position = (byte)fields[0] == 4 || (byte)fields[0] == 44 ? new UnityEngine.Vector3((float)fields[8], (float)fields[9], (float)fields[10]):UnityEngine.Vector3.zero,
            Rotation = (byte)fields[0] == 4 ? new UnityEngine.Quaternion((float)fields[11], (float)fields[12], (float)fields[13], (float)fields[14]) : UnityEngine.Quaternion.identity,
            EulerAngle = (byte)fields[0] == 44 ? new UnityEngine.Vector3((float)fields[11], (float)fields[12], (float)fields[13]) : UnityEngine.Vector3.one,
            Matrix = (byte)fields[0] == 34 ? new UnityEngine.Matrix4x4(new UnityEngine.Vector4((float)fields[8], (float)fields[9], (float)fields[10], (float)fields[11]),
                new UnityEngine.Vector4((float)fields[12], (float)fields[13], (float)fields[14], (float)fields[15]),
                new UnityEngine.Vector4((float)fields[16], (float)fields[17], (float)fields[18], (float)fields[19]),
                new UnityEngine.Vector4((float)fields[20], (float)fields[21], (float)fields[22], (float)fields[23])) : UnityEngine.Matrix4x4.identity
        };
    }

}
