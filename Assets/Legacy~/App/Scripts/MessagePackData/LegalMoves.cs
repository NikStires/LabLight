using System.Runtime.Serialization;

namespace ACAM2.MessagePack
{
    [DataContract]
    public class LegalMoves
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public Move[] Moves { get; set; }

        public static explicit operator LegalMoves(object[] fields) => new LegalMoves()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            Moves = Move.ConvertMoveArray(fields)
        };
    }
}
