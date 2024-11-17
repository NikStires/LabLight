
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ProtocolDefinition
{
    [JsonProperty("version")]
    public string version { get; set; } // Version to help identify differences in file formats

    [JsonProperty("title")]
    public string title { get; set; } // Title of the protocol

    [JsonProperty("protocolPDFNames")]
    public List<string> protocolPDFNames { get; set; } = new List<string>(); // List of PDF file names

    [JsonProperty("globalArObjects")]
    public List<ArObject> globalArObjects { get; set; } = new List<ArObject>(); // List of global AR objects

    [JsonProperty("steps")]
    public List<StepDefinition> steps { get; set; } = new List<StepDefinition>(); // List of steps

    [JsonIgnore]
    public Dictionary<string, ArObject> arObjectLookup { get; private set; }

    public void BuildArObjectLookup()
    {
        arObjectLookup = new Dictionary<string, ArObject>();
        foreach (var arObject in globalArObjects)
        {
            if (!string.IsNullOrEmpty(arObject.arObjectID))
            {
                arObjectLookup[arObject.arObjectID] = arObject;
            }
        }
    }
}

[Serializable]
public class StepDefinition
{
    [JsonProperty("isCritical")]
    public bool isCritical { get; set; }

    [JsonProperty("estimatedDurationInSeconds")]
    public int estimatedDurationInSeconds { get; set; }

    [JsonProperty("contentItems")]
    public List<ContentItem> contentItems { get; set; } = new List<ContentItem>();

    [JsonProperty("checklist")]
    public List<CheckItemDefinition> checklist { get; set; } = new List<CheckItem>();
}

[Serializable]
public class CheckItemDefinition
{
    [JsonProperty("text")]
    public string text { get; set; }

    [JsonProperty("contentItems")]
    public List<ContentItem> contentItems { get; set; } = new List<ContentItem>();

    [JsonProperty("arActions")]
    public List<ArAction> arActions { get; set; } = new List<ArAction>();
}

[Serializable]
public class ContentItem
{
    [JsonProperty("contentType")]
    public string contentType { get; set; } // e.g., "Text", "Image", "Video", "Timer"

    [JsonProperty("arObjectID")]
    public string arObjectID { get; set; } // Used to link to specific ArObject

    [JsonIgnore]
    public ArObject arObject { get; set; } // Reference to the actual ArObject

    [JsonProperty("properties")]
    public Dictionary<string, object> properties { get; set; } = new Dictionary<string, object>();
}


[Serializable]
public class ArAction
{
    [JsonProperty("actionType")]
    public string actionType { get; set; } // e.g., "Highlight", "Activate Timer"

    [JsonProperty("arObjectID")]
    public string arObjectID { get; set; } // Reference to the target ArObject

    [JsonIgnore]
    public ArObject arObject { get; set; } // Reference to the actual ArObject

    [JsonProperty("properties")]
    public Dictionary<string, object> properties { get; set; } = new Dictionary<string, object>();
}

[Serializable]
public class ArObject
{
    // [JsonIgnore] 
    // public Condition placementCondition; AM TODO -> rework condition to get condition from prefab (view)

    [JsonProperty("specificObjectName")]
    public string specificObjectName { get; set; }


    [JsonProperty("arObjectID")]
    public string arObjectID { get; set; }

    [JsonProperty("rootPrefabName")]
    public string rootPrefabName { get; set; }
}   


