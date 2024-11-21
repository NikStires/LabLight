using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ParsersV2
{
    public static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Converters = new List<JsonConverter>
        {
            new PropertiesConverter()
        }
    };

    public static List<ProtocolDescriptor> ParseProtocols(string jsonString)
    {
        try
        {
            var protocols = JsonConvert.DeserializeObject<List<ProtocolDescriptor>>(jsonString, serializerSettings);
            if (protocols == null)
            {
                throw new Exception("Failed to parse protocol list - result was null");
            }

            // Validate required fields
            foreach (var protocol in protocols)
            {
                if (string.IsNullOrEmpty(protocol.Title))
                {
                    throw new Exception("Protocol descriptor missing required Title field");
                }
                if (string.IsNullOrEmpty(protocol.Version))
                {
                    throw new Exception("Protocol descriptor missing required Title field");
                }
            }

            return protocols;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing protocol list: {e.Message}");
            throw;
        }
    }

    public static ProtocolDefinition ParseProtocol(string jsonString)
    {
        try
        {
            var protocol = JsonConvert.DeserializeObject<ProtocolDefinition>(jsonString, serializerSettings);
            if (protocol == null)
            {
                throw new Exception("Failed to parse protocol - result was null");
            }

            // Build lookup dictionary for AR objects
            protocol.BuildArObjectLookup();

            // Link AR objects to their references in content items and actions
            LinkArObjects(protocol);

            return protocol;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing protocol: {e.Message}");
            throw;
        }
    }

    private static void LinkArObjects(ProtocolDefinition protocol)
    {
        // Link AR objects in steps
        foreach (var step in protocol.Steps)
        {
            // Link content items
            foreach (var contentItem in step.ContentItems)
            {
                if (!string.IsNullOrEmpty(contentItem.ArObjectID) && 
                    protocol.ArObjectLookup.TryGetValue(contentItem.ArObjectID, out var arObject))
                {
                    contentItem.ArObject = arObject;
                }
            }

            // Link checklist items
            foreach (var checkItem in step.Checklist)
            {
                // Link content items in checklist
                foreach (var contentItem in checkItem.ContentItems)
                {
                    if (!string.IsNullOrEmpty(contentItem.ArObjectID) && 
                        protocol.ArObjectLookup.TryGetValue(contentItem.ArObjectID, out var arObject))
                    {
                        contentItem.ArObject = arObject;
                    }
                }

                // Link AR actions
                foreach (var arAction in checkItem.ArActions)
                {
                    if (!string.IsNullOrEmpty(arAction.ArObjectID) && 
                        protocol.ArObjectLookup.TryGetValue(arAction.ArObjectID, out var arObject))
                    {
                        arAction.ArObject = arObject;
                    }
                }
            }
        }
    }
}