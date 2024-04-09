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
public class LocalFileDataProvider : IProcedureDataProvider, ITextDataProvider
{
    bool hasLoadedResources = false;

    public async Task<List<ProcedureDescriptor>> GetProcedureList()
    {
        var list = new List<ProcedureDescriptor>();
  
        if (Directory.Exists(Application.persistentDataPath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath);
            foreach (var file in directoryInfo.GetFiles("*.json"))
            {
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
        ProcedureDefinition procedure = null;
        using (StreamReader streamReader = new StreamReader(Path.Combine(Application.persistentDataPath, procedureFile)))
        {
            procedure = Parsers.ParseProcedure(streamReader.ReadToEnd());
            Debug.LogFormat("Data loaded from file '{0}'", procedureFile);
        }

        if (procedure == null)
        {
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
                Debug.LogFormat("Data saved to file '{0}'", filePath);
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
}