
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ProtocolDefinition
{
    [JsonProperty("Version")]
    public string Version { get; set; } // Version to help identify differences in file formats

    [JsonProperty("Title")]
    public string Title { get; set; } // Title of the protocol

    [JsonProperty("ProtocolPDFNames")]
    public List<string> ProtocolPDFNames { get; set; } = new List<string>(); // List of PDF file names

    [JsonProperty("GlobalArObjects")]
    public List<ArObject> GlobalArObjects { get; set; } = new List<ArObject>(); // List of global AR objects

    [JsonProperty("Steps")]
    public List<StepDefinition> Steps { get; set; } = new List<StepDefinition>(); // List of steps

    [JsonIgnore]
    public Dictionary<string, ArObject> ArObjectLookup { get; private set; }

    public void BuildArObjectLookup()
    {
        ArObjectLookup = new Dictionary<string, ArObject>();
        foreach (var arObject in GlobalArObjects)
        {
            if (!string.IsNullOrEmpty(arObject.ArObjectID))
            {
                ArObjectLookup[arObject.ArObjectID] = arObject;
            }
        }
    }
}

[Serializable]
public class StepDefinition
{
    [JsonProperty("IsCritical")]
    public bool IsCritical { get; set; }

    [JsonProperty("Title")]
    public string Title { get; set; }

    [JsonProperty("EstimatedDurationInSeconds")]
    public int EstimatedDurationInSeconds { get; set; }

    [JsonProperty("ContentItems")]
    public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();

    [JsonProperty("Checklist")]
    public List<CheckItem> Checklist { get; set; } = new List<CheckItem>();
}

[Serializable]
public class CheckItem
{
    [JsonProperty("Text")]
    public string Text { get; set; }

    [JsonProperty("ContentItems")]
    public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();

    [JsonProperty("ArActions")]
    public List<ArAction> ArActions { get; set; } = new List<ArAction>();
}

[Serializable]
public class ContentItem
{
    [JsonProperty("Type")]
    public string Type { get; set; } // e.g., "Text", "Image", "Video", "Timer"

    [JsonProperty("ArObjectID")]
    public string ArObjectID { get; set; } // Used to link to specific ArObject

    [JsonIgnore]
    public ArObject ArObject { get; set; } // Reference to the actual ArObject

    [JsonProperty("Properties")]
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}


[Serializable]
public class ArAction
{
    [JsonProperty("Type")]
    public string Type { get; set; } // e.g., "Highlight", "Activate Timer"

    [JsonProperty("ArObjectID")]
    public string ArObjectID { get; set; } // Reference to the target ArObject

    [JsonIgnore]
    public ArObject ArObject { get; set; } // Reference to the actual ArObject

    [JsonProperty("Properties")]
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

[Serializable]
public class ArObject
{
    // [JsonIgnore] 
    // public Condition placementCondition; AM TODO -> rework condition to get condition from prefab (view)

    [JsonProperty("SpecificObjectName")]
    public string SpecificObjectName { get; set; }


    [JsonProperty("ArObjectID")]
    public string ArObjectID { get; set; }

    [JsonProperty("RootPrefabName")]
    public string RootPrefabName { get; set; }
}   


