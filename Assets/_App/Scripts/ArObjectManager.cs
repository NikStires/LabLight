using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class ArObjectManager : MonoBehaviour
{
    public ProtocolItemLockingManager lockingManager;
    public HeadPlacementEventChannel headPlacementEventChannel;

    private List<ArObject> activeArObjects = new List<ArObject>();
    private Dictionary<ArObject, ArObjectViewController> arViews = new Dictionary<ArObject, ArObjectViewController>();
    private Dictionary<string, GameObject> modelPrefabCache = new Dictionary<string, GameObject>();
    
    private Transform workspaceTransform;
    private Coroutine placementCoroutine;

    private static readonly HashSet<string> SupportedPrefabs = new HashSet<string>
    {
        "wellPlate",
        "source"
    };

    void Awake()
    {
        // Subscribe to protocol state changes
        ProtocolState.Instance.ProtocolStream.Subscribe(_ => InitializeArObjects()).AddTo(this);
        ProtocolState.Instance.StepStream.Subscribe(_ => RebuildArObjects()).AddTo(this);
        ProtocolState.Instance.ChecklistStream.Subscribe(_ => RebuildArObjects()).AddTo(this);

        // Commented out tracked object handling for future implementation
        /*
        SessionState.TrackedObjects.ObserveAdd().Subscribe(x => ProcessAddedObject(x.Value)).AddTo(this);
        SessionState.TrackedObjects.ObserveRemove().Subscribe(x => ProcessRemovedObject(x.Value)).AddTo(this);
        */
    }

    void Start()
    {
        lockingManager = GetComponent<ProtocolItemLockingManager>();
    }

    void OnDisable()
    {
        ClearScene();
        ProtocolState.Instance.AlignmentTriggered.Value = false;
    }

    private void InitializeArObjects()
    {
        workspaceTransform = SessionManager.instance.WorkspaceTransform;
        
        var currentProtocol = ProtocolState.Instance.ActiveProtocol.Value;
        if (currentProtocol?.globalArObjects != null)
        {
            foreach (var arObject in currentProtocol.globalArObjects)
            {
                activeArObjects.Add(arObject);
                CreateArView(arObject);
            }
        }
    }

    private void CreateArView(ArObject arObject)
    {
        if (string.IsNullOrEmpty(arObject.specificObjectName))
        {
            ServiceRegistry.Logger.LogError($"specificObjectName cannot be null or empty for ArObject: {arObject.arObjectID}");
            return;
        }

        if (!SupportedPrefabs.Contains(arObject.specificObjectName))
        {
            ServiceRegistry.Logger.LogWarning(
                $"Prefab '{arObject.specificObjectName}' is not currently supported. " +
                $"Supported prefabs are: {string.Join(", ", SupportedPrefabs)}");
            return;
        }

        var prefabPath = "Models/" + arObject.specificObjectName;
        
        ServiceRegistry.GetService<IMediaProvider>().GetPrefab(prefabPath).Subscribe(
            prefab => {
                if (prefab.TryGetComponent<ArObjectViewController>(out var arViewPrefab))
                {
                    var instance = Instantiate(arViewPrefab, workspaceTransform);
                    
                    // Create a dummy tracked object if needed
                    // TODO: Replace with actual tracking implementation
                    var dummyTrackedObject = new TrackedObject();
                    
                    instance.Initialize(arObject, new List<TrackedObject> { dummyTrackedObject });
                    instance.gameObject.SetActive(false);
                    
                    if (arViews.ContainsKey(arObject))
                    {
                        Destroy(arViews[arObject].gameObject);
                    }
                    
                    arViews[arObject] = instance;
                    modelPrefabCache[arObject.arObjectID] = instance.gameObject;
                    
                    ApplyCurrentActions(arObject, instance);
                }
                else
                {
                    ServiceRegistry.Logger.LogError(
                        $"Model {prefabPath} missing required ArObjectViewController component");
                }
            },
            error => ServiceRegistry.Logger.LogError($"Failed to load model {prefabPath}: {error}")
        );
    }

    private void RebuildArObjects()
    {
        if (activeArObjects.Count == 0)
        {
            InitializeArObjects();
        }

        // Apply current actions to all models
        foreach (var view in arViews)
        {
            if (view.Value is ModelElementViewController modelView)
            {
                modelView.disablePreviousHighlight();
                ApplyCurrentActions(view.Key, view.Value);
            }
        }
    }

    private void ApplyCurrentActions(ArObject arObject, ArObjectViewController viewController)
    {
        var currentProtocol = ProtocolState.Instance.ActiveProtocol.Value;
        if (currentProtocol == null || !ProtocolState.Instance.HasCurrentChecklist() || 
            !ProtocolState.Instance.HasCurrentCheckItem()) return;

        var currentCheckItem = ProtocolState.Instance.CurrentCheckItemDefinition;
        if (currentCheckItem == null || ProtocolState.Instance.CurrentCheckItemState.Value.IsChecked.Value) return;

        var relevantActions = new List<ArAction>();
        
        foreach (var action in currentCheckItem.actions)
        {
            if (action.arObjectID == arObject.arObjectID)
            {
                switch (action.actionType.ToLower())
                {
                    case "highlight":
                        relevantActions.Add(action);
                        break;

                    case "placement":
                        if (modelPrefabCache.TryGetValue(action.arObjectID, out var prefab))
                        {
                            RequestObjectPlacement(prefab);
                        }
                        break;
                }
            }
        }

        // Apply all highlight actions at once if there are any
        if (relevantActions.Count > 0 && viewController is ModelElementViewController modelView)
        {
            modelView.HighlightGroup(relevantActions);
        }
    }

    private void RequestObjectPlacement(GameObject model)
    {
        if (placementCoroutine == null)
        {
            placementCoroutine = StartCoroutine(StartObjectPlacement(model));
        }
    }

    private IEnumerator StartObjectPlacement(GameObject model)
    {
        yield return new WaitForSeconds(0.36f);
        if (model != null)
        {
            headPlacementEventChannel.SetHeadtrackedObject.Invoke(model);
        }
        placementCoroutine = null;
    }

    private void ClearScene()
    {
        foreach (var view in arViews.Values)
        {
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }
        
        arViews.Clear();
        activeArObjects.Clear();
        modelPrefabCache.Clear();
    }
}