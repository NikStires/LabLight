
using System.Runtime.Serialization;
using UnityEngine;

namespace ACAM2.MessagePack
{
    [DataContract]
    public class ChessBoard
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public byte SensorId { get; set; }
        public double TimeStamp { get; set; }
        public double TotalScore { get; set; }
        public UnityEngine.Vector3[] Corners { get; set; }
        public UnityEngine.Vector3[] Centers { get; set; }

        public static explicit operator ChessBoard(object[] fields) => new ChessBoard()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            SensorId = (byte)fields[3],
            TimeStamp = (double)fields[4],
            TotalScore = (double)fields[5],
            Corners = convertVector3Array(fields, 6, 4),
            Centers = convertVector3Array(fields, 18, 64),
        };

        private static UnityEngine.Vector3[] convertVector3Array(object[] fields, int start, int count)
        {
            var array = new UnityEngine.Vector3[count];
            for (int i = 0; i < count; i++)
            {
                int vectorStart = start + i * 3;
                array[i] = new UnityEngine.Vector3((float)fields[vectorStart + 0], (float)fields[vectorStart + 1], (float)fields[vectorStart + 2]);
            }
            return array;
        }
    }
}
