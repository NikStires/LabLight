using System;
using Newtonsoft.Json;

[Serializable]
public class ProtocolDescriptor
{
    [JsonProperty("Title")]
    public string Title { get; set; }

    [JsonProperty("Version")]
    public string Version { get; set; }
}