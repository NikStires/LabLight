using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.MixedReality.Toolkit.Utilities; // GetAwaiter extension method for UnityWebRequest
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.Storage;
using Windows.Storage.Search;
#endif

/// <summary>
/// Uploads files to server using UnityWebRequest
/// </summary>
public class FileUploadHandler : IFileUploadHandler
{
    // async upload of file to server
    public async Task UploadFile(string filePath)
    {
        Debug.Log("Uploading " + filePath);

        string fileServerUri = ServiceRegistry.GetService<ILighthouseControl>()?.GetFileServerUri();

        if (!string.IsNullOrEmpty(fileServerUri))
        {
            try
            {
                var fileData = File.ReadAllBytes(filePath);

                string uri = fileServerUri + "/UploadFile";
                var request = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST);
                request.useHttpContinue = false;
                request.timeout = 0;                // Timeout 0 means infinite
                request.uploadHandler = new UploadHandlerRaw(fileData);
                request.SetRequestHeader("File-Name", Path.GetFileName(filePath));
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("File upload complete!");
                }
                else
                {
                    Debug.LogError(request.error);
                }
            }
            catch (Exception ex)
            { 
                Debug.LogError(ex.Message);
                return;
            }
        }
        else
        {
            Debug.LogWarning("Could not retrieve FileServerUri from LightHouse");
        }
    }

    public async Task UploadMediaFiles(string protocolName, DateTime startTime, DateTime endTime)
    {
        //set the lighthouse recieve folder from the protocol name
        Debug.Log("Setting Lighthouse file receive folder to C:/LabLight/" + protocolName + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
        ServiceRegistry.GetService<ILighthouseControl>()?.SetFileRecieveFolder("C:/LabLight/" + protocolName + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));

#if WINDOWS_UWP
        await UploadStorageFolderFiles(KnownFolders.CameraRoll, startTime, endTime);
#else
        Debug.LogWarning("UploadImageFiles not implemented for this platform.");
#endif
    }


#if WINDOWS_UWP
    private async Task UploadStorageFolderFiles(StorageFolder storageFolder, DateTime startTime, DateTime endTime)
    {
        bool storageFolderExists = await DoesFolderExistAsync(storageFolder.Path);

        if (!storageFolderExists)
        {
            Debug.LogError("Storagefolder does not exist or no permission for use.");
            return;
        }

        Debug.Log("UploadStorageFolderFiles " + storageFolder.Path +  " between "  + startTime + " and " + endTime );

        try
        {
            //var fileList = await FindFilesBetweenTimestamps(storageFolder, startTime, endTime);
            var fileList = await storageFolder.GetFilesAsync();

            Debug.Log("FileList " + fileList.Count + " files found");

            foreach (StorageFile file in fileList)
            {
                if (file.DateCreated >= startTime)
                {
                    // process single file before prograssing to next
                    await UploadFile(file.Path);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    // Does not seem to work on HoloLens
    private async Task<IReadOnlyList<StorageFile>> FindFilesBetweenTimestamps(StorageFolder storageFolder, DateTime startTime, DateTime endTime)
    {
        QueryOptions queryOptions = new QueryOptions();
        // Using Advanced Query Syntax (AQS) to find files created between start and end time
        queryOptions.ApplicationSearchFilter = "System.DateCreated:>=" + startTime.ToString() + "<=" + endTime.ToString();
        Debug.Log("AQS filter " + queryOptions.ApplicationSearchFilter);

        StorageFileQueryResult queryResult = storageFolder.CreateFileQueryWithOptions(queryOptions);

        return await queryResult.GetFilesAsync();
    }

    async Task<bool> DoesFolderExistAsync(string folderPath)
    {
        try
        {
            // Attempt to get the folder
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderPath);

            // If the folder exists, return true
            return folder != null;
        }
        catch (System.IO.FileNotFoundException)
        {
            // If the folder doesn't exist, catch the exception and return false
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            // Handle any other exceptions that may occur due to access permissions
            // For example, if the app doesn't have the necessary permissions, it may throw this exception.
            return false;
        }
    }

#endif
}
