using System.Runtime.Serialization;

namespace ACAM2.MessagePack
{
    [DataContract]
    public class SuggestedMove
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }

        public bool isWhite;
        public uint fromFieldId;
        public uint toFieldId;
        public string fromString;
        public string toString;

        public static explicit operator SuggestedMove(object[] fields) => new SuggestedMove()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            isWhite = (byte)fields[3] > 0,
            fromFieldId = (byte)fields[4],
            toFieldId = (byte)fields[5],
            fromString = (string)fields[6],
            toString = (string)fields[7]
        };
    }
}
