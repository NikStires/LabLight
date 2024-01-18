//using Newtonsoft.Json;
//using Sirenix.OdinInspector;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
//using UnityEngine;
//using UniRx;
//using System.Collections;
//using System;
//using System.Linq;

///// <summary>
///// Unity editor MiniCMS for procedures 
///// </summary>
//[HideMonoScriptAttribute]

//public class ProcedureExplorer : MonoBehaviour
//{
//    /// <summary>
//    /// Build new lookup tables for model animations and highlights
//    /// </summary>
//    /// <param name="selectedArDefinition"></param>
//    public static void UpdateSelectedArDefinition(ArDefinition selectedArDefinition)
//    {
//        if (selectedArDefinition != null && selectedArDefinition.arDefinitionType == ArDefinitionType.Model)
//        {
//            if (string.IsNullOrEmpty(((ModelArDefinition)selectedArDefinition).url))
//                return;

//                                        //var prefabPath = SessionState.procedureDef.Value.mediaBasePath + "/" + ((ModelArDefinition)selectedArDefinition).url;
//            var prefabPath = "Models/" + ((ModelArDefinition)selectedArDefinition).url; //now derives from models folder
//            if (fileDataProvider == null)
//            {
//                fileDataProvider = new ResourceFileDataProvider();
//            }

//            fileDataProvider.GetPrefab(prefabPath).Subscribe(prefab =>
//            {
//                var model = prefab.GetComponent<ModelElementViewController>();
//                if (model != null)
//                {
//                    selectedModelHighlights = from a in model.HighlightGroups
//                                              select a.Name;

//                    // TODO fill selectedModelAnimations
//                }
//                else
//                {
//                    ServiceRegistry.Logger.LogError("Loaded model " + ((ModelArDefinition)selectedArDefinition).url + " does not contain the model script.");
//                }
//            }, (e) =>
//            {
//                ServiceRegistry.Logger.LogError("Could not load model " + ((ModelArDefinition)selectedArDefinition).url + ". " + e.ToString());
//            });
//        }
//    }

//    public static void UpdateSelectedOperation(ArOperation selectedOperation)
//    {
//        if (selectedOperation != null && selectedOperation.arDefinition != null && selectedOperation.arOperationType == ArOperationType.Highlight)
//        {
//            var modelArDefinition = selectedOperation.arDefinition as ModelArDefinition;
//            //var prefabPath = SessionState.procedureDef.Value.mediaBasePath + "/" + modelArDefinition.url;
//            var prefabPath = "Models/" + modelArDefinition.url; //now derives from models folder
//            if (fileDataProvider == null)
//            {
//                fileDataProvider = new ResourceFileDataProvider();
//            }

//            fileDataProvider.GetPrefab(prefabPath).Subscribe(prefab =>
//            {
//                var model = prefab.GetComponent<ModelElementViewController>();
//                if (model != null)
//                {
//                    selectedModelHighlights = from a in model.HighlightGroups
//                                              select a.Name;

//                    // TODO fill selectedModelAnimations
//                }
//                else
//                {
//                    ServiceRegistry.Logger.LogError("Loaded model " + modelArDefinition.url + " does not contain the model script.");
//                }
//            }, (e) =>
//            {
//                ServiceRegistry.Logger.LogError("Could not load model " + modelArDefinition.url + ". " + e.ToString());
//            });
//        }
//    }

//    /// <summary>
//    /// Adds empty checklist to current step
//    /// </summary>
//    /// <param name="operation"></param>
//    public static void AddEmptyCheckitem()
//    {
//        var checkitem = new CheckItemDefinition();

//        if (selectedProcedure.steps[ProtocolState.Step].checklist == null)
//        {
//            selectedProcedure.steps[ProtocolState.Step].checklist = new List<CheckItemDefinition>();
//        }
//        selectedProcedure.steps[ProtocolState.Step].checklist.Add(checkitem);
//    }

//    /// <summary>
//    /// Adds given operation to a new checkitem
//    /// </summary>
//    /// <param name="operation"></param>
//    public static void AddOperationInCheckItem(ArOperation operation)
//    {
//        var checkitem = new CheckItemDefinition();
//        checkitem.operations.Add(operation);

//        if (selectedProcedure.steps[ProtocolState.Step].checklist == null)
//        {
//            selectedProcedure.steps[ProtocolState.Step].checklist = new List<CheckItemDefinition>();
//        }
//        selectedProcedure.steps[ProtocolState.Step].checklist.Add(checkitem);
//    }

//    public static void AddOperationToCheckItem(ArOperation operation)
//    {
//        if (selectedProcedure.steps[ProtocolState.Step].checklist != null && selectedProcedure.steps[ProtocolState.Step].checklist[ProtocolState.CheckItem] != null)
//        {
//            var checkitem = selectedProcedure.steps[ProtocolState.Step].checklist[ProtocolState.CheckItem];
//            checkitem.operations.Add(operation);
//        }
//    }

//    private static IEnumerable GetAllImages()
//    {
//        return selectedProcedureMediaItems.FindAll(a => a.type == MediaDescriptorType.Image).Select(a => a.path);
//    }

//    private static IEnumerable GetAllVideos()
//    {
//        return selectedProcedureMediaItems.FindAll(a => a.type == MediaDescriptorType.Video).Select(a => a.path);
//    }

//    private static IEnumerable GetAllSounds()
//    {
//        return selectedProcedureMediaItems.FindAll(a => a.type == MediaDescriptorType.Sound).Select(a => a.path);
//    }

//    private static IEnumerable GetAllPrefabs()
//    {
//        return selectedPrefabs.FindAll(a => a.type == MediaDescriptorType.Prefab).Select(a => a.path);
//    }

//    private static IEnumerable GetPrefabAnimations()
//    {
//        return selectedModelAnimations;
//    }

//    private static IEnumerable GetPrefabHighlights()
//    {
//        return selectedModelHighlights;
//    }

//    // Cached lookups to fill dropdowns
//    private static List<MediaDescriptor> selectedProcedureMediaItems;
//    private static List<MediaDescriptor> selectedPrefabs;
//    private static IEnumerable<string> selectedModelAnimations;
//    private static IEnumerable<string> selectedModelHighlights;
    
//    [InfoBox("Select a procedure to load  by selecting an item in the list")]
//    [ListItemSelector("SetSelectedProcedure")]
//    public List<ProcedureDescriptor> Procedures;

//    [BoxGroup("Selected Procedure")]
//    [InfoBox("Select a step by selecting an item in the list. In playmode the AR definitions will be created.")]
//    [HideIf("@ProcedureExplorer.selectedProcedure == null")]
//    [HideReferenceObjectPicker]
//    [ShowInInspector] 
//    private static ProcedureDefinition selectedProcedure;

//    private string selectedProcedureName;

//    public void SetSelectedProcedure(int index)
//    {
//        if (index < 0)
//        {
//            selectedProcedure = null;
//            ProtocolState.SetProcedureDefinition(null);
//            return;
//        }

//        // Don't select it if folder name is empty
//        if (string.IsNullOrEmpty(Procedures[index].name))
//        {
//            return;
//        }

//        selectedProcedureName = Procedures[index].name;

//        if (fileDataProvider == null)
//        {
//            fileDataProvider = new ResourceFileDataProvider();
//        }

//        fileDataProvider.GetOrCreateProcedureDefinition(selectedProcedureName).First().Subscribe(procedureDefinition =>
//        {
//            selectedProcedure = procedureDefinition;
//            ProtocolState.Step = 0;
//            ProtocolState.ProcedureTitle = selectedProcedureName;
//            ProtocolState.SetProcedureDefinition(procedureDefinition);
//        });

//        var basePath = "Procedure/" + selectedProcedureName;
//        fileDataProvider.GetMediaList(basePath).First().Subscribe(mediaItems =>
//        {
//            selectedProcedureMediaItems = mediaItems;
//        });
//        //select models
//        var prefabPath = "Models/";
//        fileDataProvider.GetMediaList(prefabPath).First().Subscribe(prefabs =>
//        {
//            selectedPrefabs = prefabs;
//        });
//    }

//    private static ResourceFileDataProvider fileDataProvider;

//    [PropertyOrder(-1)]
//    [Sirenix.OdinInspector.FilePath(AbsolutePath = true)]
//    public string CsvPath;
//    [PropertyOrder(-1)]
//    [Button("Create Procedure From PipLight CSV")]
//    private void LoadProcedureFromCsvButton()
//    {
//        if (fileDataProvider == null)
//        {
//            fileDataProvider = new ResourceFileDataProvider();
//        }
//        if (CsvPath != null)
//        {
//            if (!string.IsNullOrEmpty(CsvPath))
//            {
//                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(CsvPath);
//                string csvText = System.IO.File.ReadAllText(CsvPath);
//                // Convert to procedure
//                var procedure = Parsers.ConvertWellPlateCsvToProcedure(fileNameWithoutExtension, csvText);
//                //add to procedure index
//                var newProcedure = new ProcedureDescriptor();
//                newProcedure.title = fileNameWithoutExtension;
//                newProcedure.name = fileNameWithoutExtension;
//                Procedures.Add(newProcedure);
//                SaveProceduresButton();
//                // Save the converted procedure to .json file
//                fileDataProvider.SaveProcedureDefinition(fileNameWithoutExtension, procedure);
//            }
//        }
//        else
//        {
//            Debug.LogWarning("file path must be provided");
//        }
//        LoadProceduresButton();
//    }

//    [PropertyOrder(-1)]
//    [Button("Load Procedures Index")]
//    private async void LoadProceduresButton()
//    {
//        if (fileDataProvider == null)
//        {
//            fileDataProvider = new ResourceFileDataProvider();
//        }

//        Procedures = await fileDataProvider.GetProcedureList();
//    }

//    [PropertyOrder(-1)]
//    [Button("Save Procedures Index")]
//    private void SaveProceduresButton()
//    {
//        string path = "Assets/Resources/Procedure/index.json";

//        StreamWriter writer = new StreamWriter(path, false);
//        var output = JsonConvert.SerializeObject(Procedures, Formatting.Indented, Parsers.serializerSettings);
//        writer.WriteLine(output);
//        writer.Close();

//#if UNITY_EDITOR
//        AssetDatabase.ImportAsset(path);
//#endif
//    }

//    [Button("Add Empty Step")]
//    [HideIf("@ProcedureExplorer.selectedProcedure == null")]
//    public void AddStepDefinition()
//    {
//        if (selectedProcedure.steps == null)
//        {
//            selectedProcedure.steps = new List<StepDefinition>();
//        }
//        selectedProcedure.steps.Add(new StepDefinition());
//    }

//    [Button("Save Procedure")]
//    [HideIf("@ProcedureExplorer.selectedProcedure == null")]
//    public void SaveProcedureDefinition()
//    {
//        if (fileDataProvider == null)
//        {
//            fileDataProvider = new ResourceFileDataProvider();
//        }

//        fileDataProvider.SaveProcedureDefinition(selectedProcedureName, selectedProcedure);
//    }
//}
