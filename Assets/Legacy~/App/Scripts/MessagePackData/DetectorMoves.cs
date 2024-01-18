using System.Runtime.Serialization;

namespace ACAM2.MessagePack
{
    [DataContract]
    public class DetectorMoves
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public Move[] Moves { get; set; }

        public static explicit operator DetectorMoves(object[] fields) => new DetectorMoves()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            Moves = Move.ConvertMoveArray(fields)
        };
    }
}
