using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class ArObjectManager : MonoBehaviour
{
    //public LockingDisplayController lockingDisplayController;

    [Header("World Container Prefabs")]
    public ContainerElementViewController WorldContainerHorizontal;
    public ContainerElementViewController WorldContainerVertical;

    [Header("Content Item Prefabs")]
    [Tooltip("Placed in contentFrame canvas")]
    public LayoutController ContainerHorizontalItem;
    public LayoutController ContainerVerticalItem;
    public TextController TextItem;
    public PropertyTextController PropertyItem;
    public ImageController ImageItem;
    public VideoController VideoItem;
    public SoundController SoundItem;

    //prefabs for generic
    public ArrowElementViewController ArrowPrefab;

    private List<ArDefinition> genericArDefinitions = new List<ArDefinition>();
    private Dictionary<ArDefinition, List<ArElementViewController>> genericArViews = new Dictionary<ArDefinition, List<ArElementViewController>>();

    private List<ArDefinition> specificArDefinitions = new List<ArDefinition>();
    private Dictionary<ArDefinition, ArElementViewController> specificArViews = new Dictionary<ArDefinition, ArElementViewController>();

    private List<MonoBehaviour> contentItemInstances = new List<MonoBehaviour>();
    private List<ArDefinition> anchorDefs = new List<ArDefinition>();
    private Transform workspaceTransform;

    void Awake()
    {
        //add audio... 
        ProtocolState.procedureStream.Subscribe(_ => InitializeArObjects()).AddTo(this);
        ProtocolState.stepStream.Subscribe(_ => RebuildArObjects()).AddTo(this);
        ProtocolState.checklistStream.Subscribe(_ => RebuildArObjects()).AddTo(this);

        SessionState.TrackedObjects.ObserveAdd().Subscribe(x => processAddedObject(x.Value)).AddTo(this);
        SessionState.TrackedObjects.ObserveRemove().Subscribe(x => processRemovedObject(x.Value)).AddTo(this);

        SessionState.enableGenericVisualizations.Subscribe(_ => ToggleGenericViews()).AddTo(this);
    }

    void OnDisable()
    {
        ClearScene();
        ProtocolState.AlignmentTriggered.Value = false;
    }

    //private void SetupVoiceCommands()
    //{
    //    disposeVoice?.Invoke();
    //    disposeVoice = null;

    //    disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
    //    {
    //        { "align", () =>
    //            {
    //                if(ProtocolState.LockingTriggered.Value && lockingDisplayController.GetCurrentAction() == LockingAction.Aligning)
    //                {
    //                    lockingDisplayController.NextAction();
    //                }
    //            }
    //        },
    //        { "anchor", () =>
    //            {
    //                if(ProtocolState.LockingTriggered.Value && lockingDisplayController.GetCurrentAction() == LockingAction.Anchoring)
    //                {
    //                    lockingDisplayController.NextAction();
    //                }
    //            }
    //        },
    //        { "set anchor", () =>
    //            {
    //                if(ProtocolState.LockingTriggered.Value && lockingDisplayController.GetCurrentAction() == LockingAction.Anchoring)
    //                {
    //                    lockingDisplayController.NextAction();
    //                }
    //            }
    //        },
    //        { "unlock anchors", () =>
    //            {
    //                if(!ProtocolState.LockingTriggered.Value)
    //                {
    //                    lockingDisplayController.TriggerLocking(anchorDefs, specificArViews);
    //                }
    //            }
    //        }
    //    });
    //}

    private void InitializeArObjects()
    {
        workspaceTransform = SessionManager.Instance.WorkspaceTransform;
        if(ProtocolState.procedureDef.Value != null)
        {
            var currentProcedure = ProtocolState.procedureDef.Value;
            if(currentProcedure.globalArElements != null)
            {
                specificArDefinitions.AddRange(currentProcedure.globalArElements.Where(ar => ar.IsSpecific()));
                
                foreach(var arDefinition in specificArDefinitions)
                {
                    SpecificArDefinitionAdded(arDefinition);
                }
            }
            genericArDefinitions.AddRange(currentProcedure.globalArElements.Where(ar => ar.IsGeneric()));
            if(SessionState.enableGenericVisualizations.Value)
            {
                ServiceRegistry.GetService<ILighthouseControl>()?.DetectorMode(4,1,10f);
            }
        }
    }

    private void processAddedObject(TrackedObject trackedObject)
    {
        //ignore detections within the bounds of an anchored model
        var modelViews = specificArViews.Where(arView => arView.Key.IsOfInterest(trackedObject.label) && arView.Key.arDefinitionType == ArDefinitionType.Model);
        foreach (var modelView in modelViews)
        {
            ModelElementViewController modelController = (ModelElementViewController)specificArViews[modelView.Key];
            if ((Vector3.Distance(trackedObject.position, modelController.transform.position) < (modelController.GetComponent<CapsuleCollider>().radius * 0.9f)) && modelController.positionLocked)
                return;
        }

        // Select instances that are waiting for this trackedObject to arrive
        var arViews = specificArViews.Where(arView => arView.Key.IsOfInterest(trackedObject.label) && !((WorldPositionController)arView.Value).positionLocked && ((WorldPositionController)arView.Value).selectedForLocking);
        
        // Resolve
        foreach (var arView in arViews)
        {
            arView.Value.TrackedObjects.Add(trackedObject);
        }
        if(SessionState.enableGenericVisualizations.Value)
        {
            processAddedGenericObject(trackedObject);
        }
    }

    private void processAddedGenericObject(TrackedObject trackedObject)
    {
        Transform parent = SessionManager.Instance.WorkspaceTransform;

        // Apply generic definitions
        foreach (var arDefinition in genericArDefinitions)
        {
            switch (arDefinition.arDefinitionType)
            {
                //case ArDefinitionType.Outline:
                //    createGenericArView(OutlinePrefab, arDefinition, parent, trackedObject);
                //    break;
                //case ArDefinitionType.Overlay:
                //    createGenericArView(OverlayPrefab, arDefinition, parent, trackedObject);
                //    break;
                //case ArDefinitionType.Mask:
                //    createGenericArView(MaskAndTitlePrefab, arDefinition, parent, trackedObject);
                //    break;
                case ArDefinitionType.Container:
                    createGenericArContainerView((ContainerArDefinition)arDefinition, parent, trackedObject);
                    break;
                case ArDefinitionType.Model:
                    if (!arDefinition.IsTargeted() || arDefinition.IsOfInterest(trackedObject.label))
                    {
                        createGenericModel((ModelArDefinition)arDefinition, parent, trackedObject);
                    }
                    break;
                case ArDefinitionType.Arrow:
                    // JA not sure what happens here, RS this should generate an arrow for each trackedobject, but also not something that will happen often
                    break;
                //case ArDefinitionType.BoundingBox:
                //    createGenericArView(BoundingBoxPrefab, arDefinition, parent, trackedObject);
                //    break;
                default:
                    Debug.LogError("ArDefinition of type '" + arDefinition.arDefinitionType + "' is not supported as generic definition yet.");
                    break;
            }
        }
    }

    private void processRemovedObject(TrackedObject trackedObject)
    {
        // Select instances that are waiting for this trackedObject to arrive
        var arViews = specificArViews.Where(arView => arView.Key.IsOfInterest(trackedObject.label));

        // Undo resolve so specifc views can disable themselves
        foreach (var arView in arViews)
        {
            arView.Value.TrackedObjects.Remove(trackedObject);
        }
        if(SessionState.enableGenericVisualizations.Value)
        {
            processRemovedGenericObject(trackedObject);
        }
    }

    private void processRemovedGenericObject(TrackedObject trackedObject)
    {
        foreach (var arDefinition in genericArDefinitions)
        {
            List<ArElementViewController> views;
            if (genericArViews.TryGetValue(arDefinition, out views))
            {
                var trackedObjectView = (from view in views
                                         where view.TrackedObjects.Contains(trackedObject)
                                         select view).FirstOrDefault();

                if (trackedObjectView)
                {
                    Destroy(trackedObjectView.gameObject);
                    views.Remove(trackedObjectView);
                }
            }
        }
    }

    private static List<TrackedObject> ResolveTrackedObjects(string[] targets)
    {
        var trackedObjects = new List<TrackedObject>();
        if (targets != null)
        {
            foreach (var target in targets)
            {
                var trackedObject = (from to in SessionState.TrackedObjects where to.label == target select to).FirstOrDefault();
                if (trackedObject != null)
                {
                    trackedObjects.Add(trackedObject);
                }
            }
        }

        return trackedObjects;
    }

    private void RebuildArObjects()
    {
        if(specificArDefinitions.Count == 0)
        {
            InitializeArObjects();
        }
        //rebuild models
        foreach (var modelDef in specificArViews.Keys.Where(key => key.arDefinitionType == ArDefinitionType.Model))
        {
            ((ModelElementViewController)specificArViews[modelDef]).disablePreviousHighlight();
            ApplyOperations(modelDef, specificArViews[modelDef]);
        }

        //apply locking if needed
        if (anchorDefs.Count > 0)
        {
            //reassign tracked objects on locking start
            foreach (var of in SessionState.TrackedObjects)
            {
                processAddedObject(of);
            }
            //lockingDisplayController.TriggerLocking(new List<ArDefinition>(anchorDefs), specificArViews);
        }
        anchorDefs.Clear();
    }

    // private void UpdateGenericDefinitions()
    // {
    //     List<ArDefinition> oldGenericArDefinitions = new List<ArDefinition>(genericArDefinitions);

    //     List<ArDefinition> rebuiltGenericArDefinitions = new List<ArDefinition>();

    //     if (ProtocolState.procedureDef.Value != null)
    //     {
    //         var currentProcedure = ProtocolState.procedureDef.Value;
    //         if (currentProcedure.globalArElements != null) //should only be called once at beginning of procedure AM
    //         {
    //             rebuiltGenericArDefinitions.AddRange(currentProcedure.globalArElements.Where(ar => ar.IsGeneric()));
    //             genericArDefinitions.RemoveAll(ar => ar.arDefinitionType == ArDefinitionType.Container);
    //         }
    //     }

    //     foreach (var arDefinition in rebuiltGenericArDefinitions)
    //     {
    //         if (!genericArDefinitions.Contains(arDefinition))
    //         {
    //             genericArDefinitions.Add(arDefinition);
    //             GenericArDefinitionAdded(arDefinition);
    //         }
    //     }

    //     foreach (var arDefinition in oldGenericArDefinitions)
    //     {
    //         if (!rebuiltGenericArDefinitions.Contains(arDefinition))
    //         {
    //             genericArDefinitions.Remove(arDefinition);
    //             GenericArDefinitionRemoved(arDefinition);
    //         }
    //     }
    // }

    private void SpecificArDefinitionAdded(ArDefinition arDefinition)
    {
        // Precreate ArViews for all arDefinitions even though the optional TrackedObject does not exist yet
        // Each ArView is responsible for showing/hiding itself depending on the availability of the TrackedObject it should be attached to
        // When the object is not there it may show an alternative view to indicate that it is expecting a certain TrackedObject

        switch (arDefinition.arDefinitionType)
        {
            case ArDefinitionType.Line:
                // Not handled
                break;
            case ArDefinitionType.Outline:
                // Not handled
                break;
            case ArDefinitionType.Overlay:
                // Not handled
                break;
            case ArDefinitionType.Model:
                createModel((ModelArDefinition)arDefinition);
                break;
            case ArDefinitionType.Container:
                createContainer((ContainerArDefinition)arDefinition);
                break;
            case ArDefinitionType.Arrow:
                //createArrow((ArrowArDefinition)arDefinition, workspaceTransform); not handled
                break;
        }
    }

    private void SpecificArDefinitionRemoved(ArDefinition arDefinition)
    {
        ArElementViewController arView;
        if (specificArViews.TryGetValue(arDefinition, out arView))
        {
            Destroy(arView.gameObject);
            specificArViews.Remove(arDefinition);
        }
    }

    // private void GenericArDefinitionAdded(ArDefinition arDefinition)
    // {
    //     Transform parent = SessionManager.Instance.workspaceTransform;

    //     switch (arDefinition.arDefinitionType)
    //     {
    //         case ArDefinitionType.Line:
    //             //not handled
    //             break;
    //         case ArDefinitionType.Outline:
    //             //not handled
    //             break;
    //         case ArDefinitionType.Overlay:
    //             //not handled
    //             break;
    //         case ArDefinitionType.Model:
    //             createModel((ModelArDefinition)arDefinition);
    //             break;
    //         case ArDefinitionType.Container:
    //             createGenericArContainerView((ContainerArDefinition)arDefinition, parent);
    //             break;
    //         case ArDefinitionType.Arrow:
    //             //createArrow((ArrowArDefinition)arDefinition, parent); not handled
    //             break;
    //     }
    // }

    private void GenericArDefinitionRemoved(ArDefinition arDefinition)
    {
        List<ArElementViewController> arViews;
        if (genericArViews.TryGetValue(arDefinition, out arViews))
        {
            foreach (var arView in arViews)
            {
                Destroy(arView.gameObject);
            }
            genericArViews.Remove(arDefinition);
        }
    }

    private void ApplyOperations(ArDefinition arDefinition, ArElementViewController arViewController) //todo NS
    {
        if (ProtocolState.procedureDef.Value != null)
        {
            var currentProcedure = ProtocolState.procedureDef.Value;

            if (currentProcedure.steps != null && currentProcedure.steps[ProtocolState.Step] != null)
            {
                var currentStep = currentProcedure.steps[ProtocolState.Step];
                
                if (currentStep.checklist != null && currentStep.checklist.Count > 0 && ProtocolState.CheckItem < currentStep.checklist.Count && !ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value)
                {
                    var currentCheckItem = currentStep.checklist[ProtocolState.CheckItem];
                    if (currentCheckItem != null)
                    {
                        foreach (var operation in currentCheckItem.operations)
                        {
                            if (operation.arDefinition == arDefinition)
                            {
                                if (operation.arOperationType == ArOperationType.Anchor)
                                {
                                    anchorDefs.Add(arDefinition);
                                }
                                else
                                {
                                    operation.Apply(arViewController);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void createModel(ModelArDefinition modelArDefinition, TrackedObject trackedObject = null)
    {
        //var prefabPath = ProtocolState.procedureDef.Value.mediaBasePath + "/" + modelArDefinition.url;
        var prefabPath = "Models/" + modelArDefinition.url;
        ServiceRegistry.GetService<IMediaProvider>().GetPrefab(prefabPath).Subscribe(prefab =>
        {
            ArElementViewController arViewPrefab = prefab.GetComponent<ArElementViewController>();
            if (arViewPrefab)
            {
                var prefabInstance = Instantiate(arViewPrefab, workspaceTransform);
                // Resolve all trackedObjects that this definition wants
                List<TrackedObject> trackedObjects = ResolveTrackedObjects(modelArDefinition.Targets());

                prefabInstance.Initialize(modelArDefinition, trackedObjects);

                // RS Quick fix to prevent overwriting models that were late loaded
                // Check if this can be done better
                if (specificArViews.ContainsKey(modelArDefinition))
                {
                    Destroy(specificArViews[modelArDefinition].gameObject);
                }
                
                specificArViews[modelArDefinition] = prefabInstance;

                //if we are creating an unlocked model with an anchor condition deactivate it until locking starts
                if(modelArDefinition.condition.conditionType == ConditionType.Anchor && !((WorldPositionController)specificArViews[modelArDefinition]).positionLocked)
                {
                    specificArViews[modelArDefinition].transform.gameObject.SetActive(false);
                }

                // Check if we need to perform operations on this view
                // might not be relevant anymore AM
                ApplyOperations(modelArDefinition, prefabInstance);
            }
            else
            {
                ServiceRegistry.Logger.LogError("Loaded model " + modelArDefinition.url + " does not contain the model script.");
            }
        }, (e) =>
        {
            ServiceRegistry.Logger.LogError("Could not load model " + modelArDefinition.url + ". " + e.ToString());
        });
    }

    private void createContainer(ContainerArDefinition arDefinition)
    {
        ContainerElementViewController viewController = Instantiate((arDefinition.layout.layoutType == LayoutType.Vertical) ? WorldContainerVertical : WorldContainerHorizontal, workspaceTransform);

        var layoutController = viewController.GetComponent<LayoutController>();
        LayoutGroup container = layoutController.LayoutGroup;

        // Resolve all trackedObjects that this definition wants
        List<TrackedObject> trackedObjects = ResolveTrackedObjects(arDefinition.Targets());

        viewController.Initialize(arDefinition, trackedObjects);
        specificArViews[arDefinition] = viewController;

        CreateContentItem(arDefinition.layout.contentItems, container, viewController);
    }

    private void CreateContentItem(List<ContentItem> contentItems, LayoutGroup container, ContainerElementViewController containerController, bool store = true)
    {
        foreach (var contentItem in contentItems)
        {
            switch (contentItem.contentType)
            {
                case ContentType.Property:
                    PropertyTextController propertyTextController = Instantiate(PropertyItem, container.transform);
                    propertyTextController.ContentItem = contentItem as PropertyItem;
                    propertyTextController.ContainerController = containerController;

                    if (store)
                    {
                        contentItemInstances.Add(propertyTextController);
                    }
                    break;
                case ContentType.Text:
                    var textController = Instantiate(TextItem, container.transform);
                    textController.ContentItem = contentItem as TextItem;

                    if (store)
                    {
                        contentItemInstances.Add(textController);
                    }
                    break;
                case ContentType.Image:
                    var imageController = Instantiate(ImageItem, container.transform);
                    imageController.ContentItem = contentItem as ImageItem;

                    if (store)
                    {
                        contentItemInstances.Add(imageController);
                    }
                    break;
                case ContentType.Video:
                    var videoController = Instantiate(VideoItem, container.transform);
                    videoController.ContentItem = contentItem as VideoItem;

                    if (store)
                    {
                        contentItemInstances.Add(videoController);
                    }
                    break;
                case ContentType.Sound:
                    var soundController = Instantiate(SoundItem, container.transform);
                    soundController.ContentItem = contentItem as SoundItem;

                    if (store)
                    {
                        contentItemInstances.Add(soundController);
                    }
                    break;
                case ContentType.Layout:
                    // Recurse into subcontainers and their items
                    LayoutItem layoutItem = ((LayoutItem)contentItem);
                    var layoutController = Instantiate((layoutItem.layoutType == LayoutType.Vertical) ? ContainerVerticalItem : ContainerHorizontalItem, container.transform);
                    layoutController.ContentItem = contentItem as LayoutItem;

                    if (store)
                    {
                        contentItemInstances.Add(layoutController);
                    }
                    CreateContentItem(layoutItem.contentItems, layoutController.LayoutGroup, containerController);
                    break;
            }
        }
    }

    private void createGenericModel(ModelArDefinition modelArDefinition, Transform parent, TrackedObject trackedObject = null)
    {
        //var prefabPath = ProtocolState.procedureDef.Value.mediaBasePath + "/" + modelArDefinition.url;
        var prefabPath = "Models/" + modelArDefinition.url;
        ServiceRegistry.GetService<IMediaProvider>().GetPrefab(prefabPath).Subscribe(prefab =>
        {
            ArElementViewController arViewPrefab = prefab.GetComponent<ArElementViewController>();
            if (arViewPrefab)
            {
                var prefabInstance = Instantiate(arViewPrefab, parent);
                List<ArElementViewController> views;
                if (!genericArViews.TryGetValue(modelArDefinition, out views))
                {
                    views = new List<ArElementViewController>();
                    genericArViews[modelArDefinition] = views;
                }
                views.Add(prefabInstance);

                prefabInstance.Initialize(modelArDefinition, new List<TrackedObject>() { trackedObject });
            }
            else
            {
                ServiceRegistry.Logger.LogError("Loaded model " + modelArDefinition.url + " does not contain the model script.");
            }
        }, (e) =>
        {
            ServiceRegistry.Logger.LogError("Could not load model " + modelArDefinition.url + ". " + e.ToString());
        });
    }


    private ArElementViewController createGenericArView(ArElementViewController prefab, ArDefinition arDefinition, Transform parent, TrackedObject trackedObject)
    {
        var genericInstance = Instantiate(prefab, parent);
        genericInstance.Initialize(arDefinition, new List<TrackedObject>() { trackedObject });
        List<ArElementViewController> views;
        if (!genericArViews.TryGetValue(arDefinition, out views))
        {
            views = new List<ArElementViewController>();
            genericArViews[arDefinition] = views;
        }
        views.Add(genericInstance);
        return genericInstance;
    }
    private void createGenericArContainerView(ContainerArDefinition containerArDefinition, Transform parent, TrackedObject trackedObject)
    {
        var prefab = (containerArDefinition.layout.layoutType == LayoutType.Vertical) ? WorldContainerVertical : WorldContainerHorizontal;
        var containerViewController = createGenericArView(prefab, containerArDefinition, parent, trackedObject) as ContainerElementViewController;

        var layoutController = containerViewController.GetComponent<LayoutController>();
        var container = layoutController.LayoutGroup;

        if (container != null)
        {
            CreateContentItem(containerArDefinition.layout.contentItems, container, containerViewController, false);
        }
        else
        {
            Debug.LogWarning("Missing LayoutGroup on one of the container prefabs");
        }
    }
    
        //maybe remove AM
    private void createArrow(ArrowArDefinition arrowArDefinition, Transform parent)
    {
        ArrowElementViewController arViewPrefab = ArrowPrefab.GetComponent<ArrowElementViewController>();
        if (arViewPrefab)
        {
            var prefabInstance = Instantiate(ArrowPrefab, parent);

            // Resolve all trackedObjects that this definition wants
            List<TrackedObject> trackedObjects = ResolveTrackedObjects(arrowArDefinition.Targets());

            prefabInstance.Initialize(arrowArDefinition, trackedObjects);
            specificArViews[arrowArDefinition] = prefabInstance;

            // Arrows do not currently support operations
        }
        else
        {
            ServiceRegistry.Logger.LogError("Arrow does not contain arrow controller script.");
        }
    }
    private void clearContentItems()
    {
        foreach (var contentItem in contentItemInstances)
        {
            Destroy(contentItem.gameObject);
        }
        contentItemInstances.Clear();
    }

    private void ClearScene()
    {
        clearContentItems();

        foreach (var arView in specificArViews)
        {
            Destroy(arView.Value.gameObject);
        }
        specificArViews.Clear();

        foreach(var arDef in genericArViews.Keys)
        {
            foreach(var arView in genericArViews[arDef])
            {   
                Destroy(arView.gameObject);
            }
        }
        genericArViews.Clear();
    }

    public void ToggleGenericViews()
    {
        if(SessionState.enableGenericVisualizations.Value)
        {
            ServiceRegistry.GetService<ILighthouseControl>()?.DetectorMode(4,1,10f);
            
            foreach(var arDef in genericArViews.Keys)
            {
                foreach(var arView in genericArViews[arDef])
                {   
                    arView.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            ServiceRegistry.GetService<ILighthouseControl>()?.DetectorMode(4,0,10f);

            foreach(var arDef in genericArViews.Keys)
            {
                foreach(var arView in genericArViews[arDef])
                {   
                    arView.gameObject.SetActive(false);
                }
            }
        }
    }
}
