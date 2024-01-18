using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ACAM2.MessagePack
{
    [DataContract]
    public class DeepModelSettings
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public uint Num_Models { get; set; }
        public string[] models { get; set; }
        
        public static explicit operator DeepModelSettings(object [] fields) => new DeepModelSettings()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            Num_Models = (uint)fields[3],
            models = convertStringArray(fields, 4, (int)fields[3])
        };

        private static string[] convertStringArray(object[] fields, int start, int count)
        {
            var array = new string[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = (string)fields[start + i];
            }
            return array;
        }
    }
}

