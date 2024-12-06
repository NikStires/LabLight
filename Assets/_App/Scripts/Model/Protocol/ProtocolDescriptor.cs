using System;
using Newtonsoft.Json;

[Serializable]
public class ProtocolDescriptor
{
    [JsonProperty("title")]
    public string title { get; set; }

    [JsonProperty("description")]
    public string description { get; set; }

    [JsonProperty("version")]
    public string version { get; set; }
} 