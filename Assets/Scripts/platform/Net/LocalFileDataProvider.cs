using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

#if UNITY_WSA && !UNITY_EDITOR
using Windows.Storage;
#endif

/// <summary>
/// Implements IDataProvider interface for accessing available procedures and updating runtime state
/// </summary>
public class LocalFileDataProvider : IProcedureDataProvider, ITextDataProvider
{
    public async Task<List<ProcedureDescriptor>> GetProcedureList()
    {
        var list = new List<ProcedureDescriptor>();

#if UNITY_WSA && !UNITY_EDITOR
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        Debug.Log("GetProcedureListAsync " + storageFolder.Name);
        IReadOnlyList<StorageFile> fileList = await storageFolder.GetFilesAsync();

        foreach (StorageFile file in fileList)
        {
            if (file.Name.EndsWith(".json"))
            {
                list.Add(new ProcedureDescriptor()
                {
                    name = Path.GetFileNameWithoutExtension(file.Name),
                    title = file.Name,
                    description = file.Name
                });
            }
        }
#else
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
#endif
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
#if !UNITY_WSA && !UNITY_EDITOR
        try
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            Debug.Log("LoadProcedureDefinitionAsync " + storageFolder.Name);
            StorageFile file = await storageFolder.GetFileAsync(procedureFile);
            if (file != null)
            {
                string text = await Windows.Storage.FileIO.ReadTextAsync(file);
                procedure = Parsers.ParseProcedure(text);
            }
            else
            {
                Debug.LogError("Could not find file");
            }
        }
        catch (Exception ex)
        {
            //            Debug.LogErrorFormat("Could not load data from '{0}'", ex.Message);
            return null;
        }
#else
        using (StreamReader streamReader = new StreamReader(Path.Combine(Application.persistentDataPath, procedureFile)))
        {
            procedure = Parsers.ParseProcedure(streamReader.ReadToEnd());
            Debug.LogFormat("Data loaded from file '{0}'", procedureFile);
        }
#endif

        if (procedure == null)
        {
            // Create empty definition
            procedure = new ProcedureDefinition()
            {
                version = 8
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
#if !UNITY_WSA && !UNITY_EDITOR
        try
        {
            // No special access rights, Typically something like C:\Users\...\AppData\Local\Packages\<app GUID>\LocalState 
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            Debug.Log("SaveProcedureDefinition " + storageFolder.Name);
            // Create procedure file in the given storaeFolder
            StorageFile sampleFile = await storageFolder.CreateFileAsync(procedureName + ".json", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            using (var stream = await sampleFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        // Serialize procedure to JSON
                        var output = JsonConvert.SerializeObject(procedure, Formatting.Indented);

                        // Write to file
                        dataWriter.WriteString(output);
                        await dataWriter.StoreAsync();
                        await outputStream.FlushAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
#else
        using (StreamWriter streamWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, procedureName), append: false))
        {
            var output = JsonConvert.SerializeObject(procedure, Formatting.Indented, Parsers.serializerSettings);
            streamWriter.WriteLine(output);
            Debug.LogFormat("Data saved to file '{0}'", procedureName);
        }
#endif
    }

    public void DeleteProcedureDefinition(string procedureName)
    {
#if !UNITY_WSA && !UNITY_EDITOR
        try
        {
            Debug.Log("DeleteProcedureDefinition " + procedureName);
            // Create procedure file in the given storaeFolder
            string pathForDeletion = Path.Combine(ApplicationData.Current.LocalFolder.Path, procedureName + ".json")
            if(pathForDeletion != null)
            {
                File.Delete(pathForDeletion);
            }
            else
            {
                Debug.LogWarning("specified file for deletion " + procedureName + " could not be found");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
#else
        Debug.Log("DeleteProcedureDefinition " + procedureName);
        string pathForDeletion = Path.Combine(Application.persistentDataPath, procedureName + ".json");
        if(pathForDeletion != null)
        {
            File.Delete(pathForDeletion);
        }
        else
        {
            Debug.LogWarning("specified file for deletion " + procedureName + " could not be found");
        }
#endif
    }

    public async Task<string> LoadTextFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        try
        {
#if !UNITY_WSA && !UNITY_EDITOR
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            Debug.Log("LoadTextFile " + storageFolder.Name);
            StorageFile file = await storageFolder.GetFileAsync(filePath);
            if (file == null)
            {
                Debug.WriteLine("Could not find file '{0}'", filePath);
                return string.Empty;
            }

            return await Windows.Storage.FileIO.ReadTextAsync(file);
#else
            using (TextReader streamReader = new StreamReader(Path.Combine(Application.persistentDataPath, filePath)))
            {

                var data = streamReader.ReadToEnd();
                Debug.LogFormat("Data '{0}' loaded from file '{1}'", data, filePath);
                return data;
            }
#endif
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
#if !UNITY_WSA && !UNITY_EDITOR
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            Debug.Log("SaveTextFile " + storageFolder.Name);
            StorageFile storageFile = await storageFolder.CreateFileAsync(filePath, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(storageFile, contents);
#else
            using (TextWriter streamWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, filePath), append: false))
            {
                streamWriter.Write(contents);
                Debug.LogFormat("Data saved to file '{0}'", filePath);
            }
#endif
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
#if !UNITY_WSA && !UNITY_EDITOR
            string storageFolder = ApplicationData.Current.LocalFolder;
#else
            string storageFolder = Application.persistentDataPath;
#endif
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