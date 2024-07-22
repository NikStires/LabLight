
using System.Numerics;
using System.Runtime.Serialization;
namespace Lighthouse.MessagePack
{
    [DataContract]
    public class Sensor
    {
        public byte Type { get; set; }
        public byte SensorId { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public UnityEngine.Vector3 Position { get; set; }
        public UnityEngine.Quaternion Rotation { get; set; }
        public UnityEngine.Vector3 EulerAngle { get; set; }
        public UnityEngine.Matrix4x4 Matrix { get; set; }

        public static explicit operator Sensor(object[] fields) => new Sensor()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            SensorId = (byte)fields[3],
            Position = (byte)fields[0] == 1 || (byte)fields[0] == 41 ? new UnityEngine.Vector3((float)fields[4], (float)fields[5], (float)fields[6]) : UnityEngine.Vector3.zero,
            Rotation = (byte)fields[0] == 1 ? new UnityEngine.Quaternion((float)fields[7], (float)fields[8], (float)fields[9], (float)fields[10]) : UnityEngine.Quaternion.identity,
            EulerAngle = (byte)fields[0] == 41 ? new UnityEngine.Vector3((float)fields[7], (float)fields[8], (float)fields[9]) : UnityEngine.Vector3.one,
            Matrix = (byte)fields[0] == 31 ? new UnityEngine.Matrix4x4(new UnityEngine.Vector4((float)fields[4], (float)fields[5], (float)fields[6], (float)fields[7]),
                new UnityEngine.Vector4((float)fields[8], (float)fields[9], (float)fields[10], (float)fields[11]),
                new UnityEngine.Vector4((float)fields[12], (float)fields[13], (float)fields[14], (float)fields[15]),
                new UnityEngine.Vector4((float)fields[16], (float)fields[17], (float)fields[18], (float)fields[19])) : UnityEngine.Matrix4x4.identity
        };
    }
}
