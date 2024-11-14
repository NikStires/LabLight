using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;

public class Parsers
{
    public static List<ProtocolDescriptor> ParseProtocols(string json)
    {
        try
        {
            var protocols = JsonConvert.DeserializeObject<List<ProtocolDescriptor>>(json);
            return protocols;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing protocols list: " + e);
            throw;
        }
    }

    public static List<ProtocolDescriptor> ParseProtocols(string json)
    {
        var list = new List<ProtocolDescriptor>();
        var protocols = JArray.Parse(json);
        foreach (JObject protocol in protocols.Children())
        {
            list.Add(new ProtocolDescriptor()
            {
                name = (string)protocol["name"],
                title = (string)protocol["title"],
                description = (string)protocol["description"]
            });
        }
        return list;
    }*/

    public static WorkspaceFrame ParseWorkspace(string json)
    {
        // Workspace values are in meters
        try
        {
            var workspace = new WorkspaceFrame();

            var root = JObject.Parse(json);
            workspace.cameraPosition = vec3((JArray)root["camera"]);
            workspace.border = new List<Vector2>();

            var points = (JArray)root["border"];
            foreach (JArray pt in points.Children())
            {
                workspace.border.Add(vec2(pt));
            }

            return workspace;
        }
        catch (System.Exception e)
        {
            ServiceRegistry.Logger.LogError("Error parsing workspace: " + e.ToString());
            throw;
        }
    }

    public static NetStateFrame ParseNetStateFrame(string json)
    {
        try
        {
            var frame = new NetStateFrame();

            var root = JObject.Parse(json);
            frame.master = (string)root["master"];
            frame.procedure = (string)root["procedure"];
            frame.step = (int)root["step"];

            var screen = (JObject)root["screen"];
            var screenPos = (JArray)screen["pos"];
            var screenVec = (JArray)screen["vec"];

            frame.screen = new PositionRotation()
            {
                position = vec3(screenPos),
                lookForward = vec3(screenVec),
            };

            frame.objects = new List<TrackedObject>();

            var objects = (JArray)root["objects"];
            foreach (JObject obj in objects.Children())
            {

                var center = (JArray)obj["center"];
                var size = (JArray)obj["size"];
                var angle = (float)obj["angle"];
                var z = (float)obj["z"];

                frame.objects.Add(new TrackedObject()
                {
                    id = (int)obj["id"],
                    label = (string)obj["label"],
                    angle = angle,
                    scale = new Vector3((float)size[0], z, (float)size[1]),
                    position = new Vector3((float)center[0], 0, (float)center[1]),
                    rotation = Quaternion.AngleAxis(angle, Vector3.up)
                });
            }

            return frame;
        }
        catch (System.Exception e)
        {
            ServiceRegistry.Logger.LogError("Parsing protocol index: " + e.ToString());
            throw;
        }
    }

    // public static ProtocolDefinition ParseProtocol(string json)
    // {
    //     try
    //     {
    //         var protocol = new ProtocolDefinition();

    //         var root = JObject.Parse(json);
    //         protocol.version = (root["version"] == null) ? 0 : (int)root["version"];

    //         protocol.title = (string)root["title"];

    //         Debug.Log("Protocol '" + protocol.title + "' file version " + protocol.version);

    //         if (protocol.version >= 1)
    //         {
    //             protocol = JsonConvert.DeserializeObject<ProtocolDefinition>(json, serializerSettings);
    //         }
    //         else
    //         { 
    //             Debug.LogError("Version 0 procedure is no longer supported");
    //         }


    //         return protocol;
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError("Parsing protocol index: " + e.ToString());
    //         throw;
    //     }
    // }

    static Vector3 vec3(JArray arr)
    {
        return new Vector3((float)arr[0], (float)arr[1], (float)arr[2]);
    }

    static Vector2 vec2(JArray arr)
    {
        return new Vector2((float)arr[0], (float)arr[1]);
    }

    // public static ProtocolDefinition ConvertWellPlateCsvToProtocol(string filename, string csvString)
    // {
    //     try
    //     {
    //         ProtocolDefinition protocol = new ProtocolDefinition();
    //         protocol.version = 7;
    //         protocol.title = filename;

    //         Debug.Log("Protocol '" + protocol.title + "' file version " + protocol.version);

    //         protocol.steps = new List<StepDefinition>();
    //         //protocol.steps = convertCSVtoProtocol.ReadStepsFromCSV(csvString.Split('\n'));

    //         if(filename.Contains("piplight_"))
    //         {
    //             //protocol = convertCSVtoProtocol.ReadPipLightCSV(csvString.Split('\n'), filename);
    //         }
    //         else if(filename.Contains("pooling_"))
    //         {
    //             //protocol = convertCSVtoProtocol.ReadPoolingCSV(csvString.Split('\n'), filename);
    //         }

    //         return protocol;
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError("Parsing protocol index: " + e.ToString());
    //         throw;
    //     }
    // }

    public static AnchorData ParseAnchorData(string json)
    {
        try
        {
            var anchorData = new AnchorData();

            var root = JObject.Parse(json);
            anchorData.version = (root["version"] == null) ? 0 : (int)root["version"];

            if (anchorData.version >= 1)
            {
                anchorData = JsonConvert.DeserializeObject<AnchorData>(json, serializerSettings);
            }
            else
            {
                Debug.LogError("Anchor data is missing version.");
            }

            return anchorData;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Parsing anchor data: " + e.ToString());
            throw;
        }
    }
}