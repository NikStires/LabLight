using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Static helper methods to convert parsed JSON data into instanced classes
/// </summary>
public class Parsers
{
    public static JsonSerializerSettings serializerSettings = new JsonSerializerSettings
    {
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        NullValueHandling = NullValueHandling.Ignore
    };

    public static List<ProcedureDescriptor> ParseProcedures(string json)
    {
        var list = new List<ProcedureDescriptor>();
        var procedures = JArray.Parse(json);
        foreach (JObject procedure in procedures.Children())
        {
            list.Add(new ProcedureDescriptor()
            {
                name = (string)procedure["name"],
                title = (string)procedure["title"],
                description = (string)procedure["description"]
            });
        }
        return list;
    }

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
//            ServiceRegistry.Logger.LogError("Error parsing workspace: " + e.ToString());
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
   //         ServiceRegistry.Logger.LogError("Parsing protocol index: " + e.ToString());
            throw;
        }
    }

    public static ProcedureDefinition ParseProcedure(string json)
    {
        try
        {
            var procedure = new ProcedureDefinition();

            var root = JObject.Parse(json);
            procedure.version = (root["version"] == null) ? 0 : (int)root["version"];

            procedure.title = (string)root["title"];

            Debug.Log("Procedure '" + procedure.title + "' file version " + procedure.version);

            if (procedure.version >= 1)
            {
                procedure = JsonConvert.DeserializeObject<ProcedureDefinition>(json, serializerSettings);
            }
            else
            {
                Debug.LogError("Version 0 procedure is no longer supported");
            }

            return procedure;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Parsing protocol index: " + e.ToString());
            throw;
        }
    }

    static Vector3 vec3(JArray arr)
    {
        return new Vector3((float)arr[0], (float)arr[1], (float)arr[2]);
    }

    static Vector2 vec2(JArray arr)
    {
        return new Vector2((float)arr[0], (float)arr[1]);
    }

    public static ProcedureDefinition ConvertWellPlateCsvToProcedure(string filename, string csvString)
    {
        try
        {
            ProcedureDefinition procedure = new ProcedureDefinition();
            procedure.version = 7;
            procedure.title = filename;

            Debug.Log("Procedure '" + procedure.title + "' file version " + procedure.version);

            procedure.steps = new List<StepDefinition>();
            //procedure.steps = convertCSVtoProcedure.ReadStepsFromCSV(csvString.Split('\n'));

            if(filename.Contains("piplight_"))
            {
                //procedure = convertCSVtoProcedure.ReadPipLightCSV(csvString.Split('\n'), filename);
            }
            else if(filename.Contains("pooling_"))
            {
                //procedure = convertCSVtoProcedure.ReadPoolingCSV(csvString.Split('\n'), filename);
            }

            return procedure;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Parsing protocol index: " + e.ToString());
            throw;
        }
    }
}