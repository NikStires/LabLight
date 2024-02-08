using System.Runtime.Serialization;

namespace Lighthouse.MessagePack
{
    public class CsvFileInfo
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }

        public string FileName;

        public static explicit operator CsvFileInfo(object[] fields) => new CsvFileInfo()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            FileName = (string)fields[3]
        };
    }
}