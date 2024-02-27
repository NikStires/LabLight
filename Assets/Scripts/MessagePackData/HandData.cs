using System.Runtime.Serialization;
using System.Linq;
namespace Lighthouse.MessagePack
{
    [DataContract]
    public class HandData
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public byte SensorId { get; set; }
        public float MarkerLengthPixels { get; set; }
        public float AngleToSensor { get; set; }
        public UnityEngine.Matrix4x4 Matrix {get;set;}

        public static explicit operator HandData(object[] fields) => new HandData()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            SensorId = (byte)fields[3],
            MarkerLengthPixels = (float)fields[9],
            AngleToSensor = (float)fields[10],
            Matrix = (byte)fields[0] == 33 ? new UnityEngine.Matrix4x4(new UnityEngine.Vector4((float)fields[11], (float)fields[12], (float)fields[13], (float)fields[14]),
                new UnityEngine.Vector4((float)fields[15], (float)fields[16], (float)fields[17], (float)fields[18]),
                new UnityEngine.Vector4((float)fields[19], (float)fields[20], (float)fields[21], (float)fields[22]),
                new UnityEngine.Vector4((float)fields[23], (float)fields[24], (float)fields[25], (float)fields[26])) : UnityEngine.Matrix4x4.identity
        };
    }

}
