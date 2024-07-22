using System.Runtime.Serialization;

namespace Lighthouse.MessagePack
{
    public class JsonFileInfo
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }

        public string FileName;

        public static explicit operator JsonFileInfo(object[] fields) => new JsonFileInfo()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            FileName = (string)fields[3]
        };
    }
}