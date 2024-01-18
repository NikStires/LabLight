using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestModel : MonoBehaviour
{
    private ProcedureDefinition BuildTestProcedure()
    {
        var testModel = new ModelArDefinition()
        {
            url = "somprefab.prefab"
        };


        return new ProcedureDefinition()
        {
            version = 7,
            title = "TestProcedure",
            globalArElements = new List<ArDefinition>()
            {
                testModel
            },
            steps = new List<StepDefinition>()
            {
                new StepDefinition(){}
            },
        };
    }

    [Button("Save and load")]
    public void SaveLoad()
    {
        var procedure = BuildTestProcedure();
        var jsonString = JsonConvert.SerializeObject(procedure, Formatting.Indented, Parsers.serializerSettings);
        var loadedProcedure = Parsers.ParseProcedure(jsonString);
    }

    [Button("Save Procedure")]
    public void SaveProcedureDefinition()
    {
        var procedure = BuildTestProcedure();
        string filePath = "Assets/Resources/test.json";
        StreamWriter writer = new StreamWriter(filePath, false);
        var output = JsonConvert.SerializeObject(procedure, Formatting.Indented, Parsers.serializerSettings);
        writer.WriteLine(output);
        writer.Close();
    }
}
