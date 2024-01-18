
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
namespace ACAM2.MessagePack
{
    [DataContract]
    public class DetectedObject
    {
        public DetectedObject(int count, float[] vertices)
        {
            this.ContourPointCount = count;
            ContourPoints = new UnityEngine.Vector3[this.ContourPointCount];

            for (int j = 0; j < ContourPointCount * 3; j += 3)
            {
                UnityEngine.Vector3 point = new UnityEngine.Vector3(vertices[j], vertices[j + 1], vertices[j + 2]);
                ContourPoints[(j / 3)] = point;
            }
        }

        public byte Type { get; set; }
        public byte SensorId { get; set; }
        public double TimeStamp { get; set; }
        public int TrackingId { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public float Score { get; set; }
        public byte ClassId { get; set; }
        public string ClassName { get; set; }
        public System.UInt16 BoxWidth { get; set; }
        public System.UInt16 BoxHeight { get; set; }
        public UnityEngine.Vector3 Center { get; set; }
        public UnityEngine.Vector3[] Bounds { get; set; }
        public UnityEngine.Vector3 Position { get; set; }
        public UnityEngine.Quaternion Rotation { get; set; }
        public UnityEngine.Vector3 EulerAngle { get; set; }
        public UnityEngine.Matrix4x4 Matrix { get; set; }
        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }
        public int ContourPointCount { get; set; }
        public UnityEngine.Vector3[] ContourPoints { get; set; }

        public UnityEngine.Color Color
        {
            get
            {
                return new UnityEngine.Color(this.Red, this.Green, this.Blue);
            }
        }

        private static UnityEngine.Vector3 ReadVector3(object[] fields, int indexStart)
        {
            return new UnityEngine.Vector3((float)fields[indexStart], (float)fields[indexStart + 1], (float)fields[indexStart + 2]);
        }

        private static UnityEngine.Quaternion ReadQuaternion(object[] fields, int indexStart)
        {
            return new UnityEngine.Quaternion((float)fields[indexStart], (float)fields[indexStart+1], (float)fields[indexStart+2], (float)fields[indexStart+3]);
        }

        public static UnityEngine.Matrix4x4 ReadMatrix(object[] fields, int indexStart)
        {
            return new UnityEngine.Matrix4x4(new UnityEngine.Vector4((float)fields[indexStart], (float)fields[indexStart + 1], (float)fields[indexStart + 2], (float)fields[indexStart + 3]),
                new UnityEngine.Vector4((float)fields[indexStart + 4], (float)fields[indexStart + 5], (float)fields[indexStart + 6], (float)fields[indexStart + 7]),
                new UnityEngine.Vector4((float)fields[indexStart + 8], (float)fields[indexStart + 9], (float)fields[indexStart + 10], (float)fields[indexStart + 11]),
                new UnityEngine.Vector4((float)fields[indexStart + 12], (float)fields[indexStart + 13], (float)fields[indexStart + 14], (float)fields[indexStart + 15]));
        }

        public static explicit operator DetectedObject(object[] fields) => new DetectedObject(
            (byte)fields[0] == 6 ? System.Convert.ToInt32((byte)fields[48]) : (byte)fields[0] == 36 ? System.Convert.ToInt32((byte)fields[57]) : System.Convert.ToInt32((byte)fields[47]),
            (byte)fields[0] == 6 ? fields.Skip(49).Cast<float>().ToArray() : (byte)fields[0] == 36 ? fields.Skip(58).Cast<float>().ToArray() : fields.Skip(48).Cast<float>().ToArray()
        )
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            SensorId = (byte)fields[3],
            TimeStamp = (double)fields[4],
            TrackingId = System.Convert.ToInt32(fields[5]),
            Score = (float)fields[6],
            ClassId = (byte)fields[7],
            ClassName = (string)fields[8],
            BoxWidth = System.Convert.ToUInt16(fields[9]), // can be UInt16 or byte
            BoxHeight = System.Convert.ToUInt16(fields[10]),// can be UInt16 or byte
            Center = new UnityEngine.Vector3((float)fields[11], (float)fields[12], (float)fields[13]),
            Bounds = new UnityEngine.Vector3[]
            {
                // first 4 are points projected onto the table
                new UnityEngine.Vector3((float)fields[14], (float)fields[15], (float)fields[16]),
                new UnityEngine.Vector3((float)fields[17], (float)fields[18], (float)fields[19]),
                new UnityEngine.Vector3((float)fields[20], (float)fields[21], (float)fields[22]),
                new UnityEngine.Vector3((float)fields[23], (float)fields[24], (float)fields[25]),
                
                // last  4 are points raised above the table based on the height of the center of the detection
                new UnityEngine.Vector3((float)fields[26], (float)fields[27], (float)fields[28]),
                new UnityEngine.Vector3((float)fields[29], (float)fields[30], (float)fields[31]),
                new UnityEngine.Vector3((float)fields[32], (float)fields[33], (float)fields[34]),
                new UnityEngine.Vector3((float)fields[35], (float)fields[36], (float)fields[37]),
            },

            Position = (byte)fields[0]==6 || (byte)fields[0] == 46 ? ReadVector3(fields,38): UnityEngine.Vector3.zero,
            Matrix = (byte)fields[0] == 36 ? ReadMatrix(fields, 38) : UnityEngine.Matrix4x4.identity,
            Rotation = (byte)fields[0] == 6 ? ReadQuaternion(fields, 41): UnityEngine.Quaternion.identity,
            EulerAngle = (byte)fields[0]== 46 ? ReadVector3(fields, 41) : UnityEngine.Vector3.one,

            Red = (byte)fields[0] == 6 ? (float)fields[45] : (byte)fields[0] == 36 ? (float)fields[54] : (byte)fields[0] == 46 ? (float)fields[44]:0,
            Green = (byte)fields[0] == 6 ? (float)fields[46] : (byte)fields[0] == 36 ? (float)fields[55] : (byte)fields[0] == 46 ? (float)fields[45] : 0,
            Blue = (byte)fields[0] == 6 ? (float)fields[47] : (byte)fields[0] == 36 ? (float)fields[56] : (byte)fields[0] == 46 ? (float)fields[46] : 0,
        };
    }
}
