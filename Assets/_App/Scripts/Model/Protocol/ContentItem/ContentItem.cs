

public enum ContentType //depricated 
{
    Text = 0,               // Single block of text (RTF?)
    Image = 1,              // Image URL
    Video = 2,              // Video URL
    Layout = 3,             // Subcontainer that specifies layout of children
    Sound = 4,              // Sound URL
    Property = 5,            // TrackedObject property as string (only useful for containers that are attached to a trackedObject)
    WebUrl = 6              // URL to a webpage
}

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