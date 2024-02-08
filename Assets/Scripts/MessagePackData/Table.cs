
using System.Numerics;
using System.Runtime.Serialization;
namespace Lighthouse.MessagePack
{
    [DataContract]
    public class Table
    {
        public byte Type { get; set; }
        public byte SensorId { get; set; }
        public double TimeStamp { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public UnityEngine.Vector3 Center { get; set; }
        public UnityEngine.Quaternion Rotation { get; set; }
        public UnityEngine.Vector3 Normal { get; set; }
        public float Size { get; set; }
        public UnityEngine.Vector3 EulerAngle { get; set; }
        public UnityEngine.Matrix4x4 Matrix { get; set; }

        public static explicit operator Table(object[] fields) => new Table()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            SensorId = (byte)fields[3],
            TimeStamp = (double)fields[4],
            Center = (byte)fields[0] == 2 || (byte)fields[0] == 42 ? new UnityEngine.Vector3((float)fields[5], (float)fields[6], (float)fields[7]) : UnityEngine.Vector3.zero,
            Rotation = (byte)fields[0] == 2 ? new UnityEngine.Quaternion((float)fields[8], (float)fields[9], (float)fields[10], (float)fields[11]) : UnityEngine.Quaternion.identity,
            EulerAngle = (byte)fields[0] == 42 ? new UnityEngine.Vector3((float)fields[8], (float)fields[9], (float)fields[10]) : UnityEngine.Vector3.one,
            Normal = (byte)fields[0] == 2 ? new UnityEngine.Vector3((float)fields[12], (float)fields[13], (float)fields[14]) :
                    (byte)fields[0] == 32 ? new UnityEngine.Vector3((float)fields[21], (float)fields[22], (float)fields[23]) :
                    (byte)fields[0] == 42 ? new UnityEngine.Vector3((float)fields[11], (float)fields[12], (float)fields[13]) : UnityEngine.Vector3.one,
            Size = (byte)fields[0] == 2 ? (float)fields[15] : (byte)fields[0] == 32 ? (float)fields[24] : (byte)fields[0] == 42 ? (float)fields[14] : 0,
            Matrix = (byte)fields[0] == 32 ? new UnityEngine.Matrix4x4(new UnityEngine.Vector4((float)fields[5], (float)fields[6], (float)fields[7], (float)fields[8]),
                new UnityEngine.Vector4((float)fields[9], (float)fields[10], (float)fields[11], (float)fields[12]),
                new UnityEngine.Vector4((float)fields[13], (float)fields[14], (float)fields[15], (float)fields[16]),
                new UnityEngine.Vector4((float)fields[17], (float)fields[18], (float)fields[19], (float)fields[20])) : UnityEngine.Matrix4x4.identity
        };
    }
}
