using System;
using System.Collections.Generic;
using Newtonsoft.Json;


[Serializable]
public class ContentItem
{
    [JsonProperty("contentType")]
    public string contentType { get; set; } // e.g., "Text", "Image", "Video", "Timer"

    [JsonProperty("arObjectID")]
    public string arObjectID { get; set; } // Used to link to a specific ArObject

    [JsonIgnore]
    public ArObject arObject { get; set; } // Reference to the actual ArObject

    [JsonProperty("properties")]
    public Dictionary<string, object> properties { get; set; } = new Dictionary<string, object>();
}