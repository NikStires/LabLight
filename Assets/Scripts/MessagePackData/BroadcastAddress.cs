using System.Runtime.Serialization;
namespace Lighthouse.MessagePack
{
    /// <summary>Class BroadcastAddress.</summary>
    [DataContract]
    public class BroadcastAddress
    {
        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public byte Type { get; set; }
        /// <summary>Gets or sets the direct ip address.</summary>
        /// <value>The direct ip address.</value>
        public string DirectIpAddress { get; set; }
        /// <summary>Gets or sets the port.</summary>
        /// <value>The port.</value>
        public ushort Port { get; set; }
        /// <summary>Gets or sets the direct port.</summary>
        /// <value>The direct port.</value>
        public ushort DirectPort { get; set; }
        /// <summary>Gets or sets the name of the server.</summary>
        /// <value>The name of the server.</value>
        public string ServerName { get; set; }
        /// <summary>Gets or sets the packet version.</summary>
        /// <value>The packet version.</value>
        public byte PacketVersion { get; set; }
        /// <summary>Gets or sets the name of the build.</summary>
        /// <value>The name of the build.</value>
        public string BuildName { get; set; }

        /// <summary>Performs an explicit conversion from <see cref="System.Object[]" /> to <see cref="T:ACAM2.MessagePack.BroadcastAddress" />.</summary>
        /// <param name="fields">The fields.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator BroadcastAddress(object[] fields) => new BroadcastAddress() { Type = (byte)fields[0], DirectIpAddress = (string)fields[1], Port = (ushort)fields[2], DirectPort = (ushort)fields[3], ServerName = (string)fields[4], PacketVersion=(byte)fields[5], BuildName=(string)fields[6] };
    }
}
