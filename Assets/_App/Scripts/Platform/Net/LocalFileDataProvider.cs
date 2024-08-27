using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

/// <summary>
/// Implements IDataProvider interface for accessing available procedures and updating runtime state
/// </summary>
public class LocalFileDataProvider : IProcedureDataProvider, ITextDataProvider, IAnchorDataProvider
{
    private const string anchorDataFile = "AnchorData.jason";

    public async Task<List<ProcedureDescriptor>> GetProcedureList()
    {
        var list = new List<ProcedureDescriptor>();

        if (Directory.Exists(Application.persistentDataPath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath);

            foreach (var file in directoryInfo.GetFiles("*.json"))
            {
                Debug.Log("Found file: " + file.Name);
                list.Add(new ProcedureDescriptor()
                {
                    name = Path.GetFileNameWithoutExtension(file.Name),
                    title = file.Name,
                    description = file.Name
                });
            }
        }
        return list;
    }

    public IObservable<ProcedureDefinition> GetOrCreateProcedureDefinition(string procedureName)
    {
        return LoadProcedureDefinitionAsync(procedureName + ".json").ToObservable<ProcedureDefinition>();
    }


    /// <summary>
    /// load procedure from local folder
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="procedure"></param>
    public async Task<ProcedureDefinition> LoadProcedureDefinitionAsync(string procedureFile)
    {
        Debug.Log("Local file data provider trying to load " + Path.Combine(Application.persistentDataPath, procedureFile));
        ProcedureDefinition procedure = null;
        using (StreamReader streamReader = new StreamReader(Path.Combine(Application.persistentDataPath, procedureFile)))
        {
            procedure = Parsers.ParseProcedure(streamReader.ReadToEnd());
            Debug.LogFormat("Data loaded from file '{0}'", procedureFile);
        }

        if (procedure == null)
        {
            Debug.Log("protocol not found, creating empty protocol");
            // Create empty definition
            procedure = new ProcedureDefinition()
            {
                version = 9
            };
        }

        procedure.mediaBasePath = "CSV";

        return procedure;
    }


    /// <summary>
    /// Save procedure to local folder
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="procedure"></param>
    public async void SaveProcedureDefinition(string procedureName, ProcedureDefinition procedure)
    {
        using (StreamWriter streamWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, procedureName), append: false))
        {
            var output = JsonConvert.SerializeObject(procedure, Formatting.Indented, Parsers.serializerSettings);
            streamWriter.WriteLine(output);
            Debug.LogFormat("Data saved to file '{0}'", procedureName);
        }
    }

    public void DeleteProcedureDefinition(string procedureName)
    {
        Debug.Log("DeleteProcedureDefinition " + procedureName);
        string pathForDeletion = Path.Combine(Application.persistentDataPath, procedureName + ".json");
        if (pathForDeletion != null)
        {
            File.Delete(pathForDeletion);
        }
        else
        {
            Debug.LogWarning("specified file for deletion " + procedureName + " could not be found");
        }
    }

    public async Task<string> LoadTextFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        try
        {
            using (TextReader streamReader = new StreamReader(Path.Combine(Application.persistentDataPath, filePath)))
            {

                var data = streamReader.ReadToEnd();
                Debug.LogFormat("Data '{0}' loaded from file '{1}'", data, filePath);
                return data;
            }
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Could not load data from '{0}'", ex.Message);
            return string.Empty;
        }
    }

    public async void SaveTextFile(string filePath, string contents)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        try
        {
            using (TextWriter streamWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, filePath), append: false))
            {
                streamWriter.Write(contents);
                Debug.LogFormat("Data saved to file '{0}'", Path.Combine(Application.persistentDataPath, filePath));
            }
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Could not save data to '{0}'", ex.Message);
        }
    }

    public void DeleteTextFile(string fileName, string fileExt)
    {
        if(string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            string storageFolder = Application.persistentDataPath;
            Debug.Log("DeleteTextFile " + fileName + " from " + storageFolder);
            if (File.Exists(Path.Combine(storageFolder, fileName + fileExt)))
            {
                File.Delete(Path.Combine(storageFolder, fileName + fileExt));
            }
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Could not delete file '{0}'", ex.Message);
        }
    }

    public IObservable<AnchorData> GetOrCreateAnchorData()
    {
        return LoadAnchorDataAsync().ToObservable<AnchorData>();
    }

    public async Task<AnchorData> LoadAnchorDataAsync()
    {
        var anchorDataFilePath = Path.Combine(Application.persistentDataPath, anchorDataFile);
        Debug.Log("Local file data provider trying to load " + anchorDataFilePath);

        AnchorData anchorData = null;

        if (File.Exists(anchorDataFilePath))
        {
            using (StreamReader streamReader = new StreamReader(anchorDataFilePath))
            {
                anchorData = Parsers.ParseAnchorData(streamReader.ReadToEnd());
                Debug.LogFormat("Data loaded from file '{0}'", anchorDataFile);
            }
        }

        if (anchorData == null)
        {
            Debug.Log("AnchorData not found, creating empty anchorData");
            anchorData = new AnchorData();
            anchorData.version = 1;
        }

        return anchorData;
    }

    public async void SaveAnchorData(AnchorData anchorData)
    {
        using (StreamWriter streamWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, anchorDataFile), append: false))
        {
            var output = JsonConvert.SerializeObject(anchorData, Formatting.Indented, Parsers.serializerSettings);
            streamWriter.WriteLine(output);
            Debug.LogFormat("Data saved to file '{0}'", anchorDataFile);
        }
    }
}