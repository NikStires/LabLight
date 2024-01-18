//using System.IO;
//using System.Threading.Tasks;
//using UniRx;
//using UnityEngine;
//using UnityEngine.Networking;


///// <summary>
///// Stub to handle the transition from the legacy API
///// 
///// </summary>
//public class WellPlateCsvProvider : IWellPlateCsvProvider
//{
//    public async Task<string> DownloadWellPlateCsvAsync()
//    {
//        //string fileServerUri = ServiceRegistry.GetService<ILighthouseControl>()?.GetFileServerUri();

//        if (!string.IsNullOrEmpty(fileServerUri))
//        {
//            string uri;

//            bool filenameKnown = !string.IsNullOrEmpty(SessionState.CsvFileDownloadable.Value);
//            if (filenameKnown)
//            {
//                uri = fileServerUri + "/GetFile?Filename=" + SessionState.CsvFileDownloadable.Value;
//            }
//            else
//            {
//                uri = fileServerUri + "/GetWellPlateCsv";
//            }

//            Debug.Log("Downloading from " + uri);

//            UnityWebRequest request = UnityWebRequest.Get(uri);
//            await request.SendWebRequest();

//            if (request.result == UnityWebRequest.Result.Success)
//            {
//                var fileName = filenameKnown ? SessionState.CsvFileDownloadable.Value : request.GetResponseHeader("File-Name");

//                if (!string.IsNullOrEmpty(fileName))
//                {
//                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

//                    // Save the .csv
//                    var lfdp = new LocalFileDataProvider();

//                    Debug.Log("Saving text file");

//                    lfdp.SaveTextFile(fileName, request.downloadHandler.text);

//                    // Convert to procedure
//                    var procedure = Parsers.ConvertWellPlateCsvToProcedure(fileNameWithoutExtension, request.downloadHandler.text);

//                    Debug.Log("Saving json file");

//                    // Save the converted procedure to .json file
//                    lfdp.SaveProcedureDefinition(fileNameWithoutExtension + ".json", procedure);

//                    lfdp.DeleteTextFile(fileNameWithoutExtension, ".csv");

//                    var audioPlayer = ServiceRegistry.GetService<IAudio>();
//                    if (audioPlayer != null)
//                    {
//                        audioPlayer.Play(AudioEventEnum.DownloadComplete);
//                    }
//                }
//                else
//                {
//                    Debug.LogError("There is no 'File-Name' in the response header.");
//                }

//                SessionState.CsvFileDownloadable.Value = string.Empty;
//            }
//            else
//            {
//                Debug.LogError(request.error);
//            }
//        }
//        else
//        {
//            Debug.LogError("Could not retrieve FileServerUri from LightHouse");
//        }

//        return null;
//    }
//}
