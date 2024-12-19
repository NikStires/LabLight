using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ArObjectManager : MonoBehaviour
{
    public ProtocolItemLockingManager lockingManager;
    public HeadPlacementEventChannel headPlacementEventChannel;

    private readonly Dictionary<ArObject, ArObjectViewController> arViews = new Dictionary<ArObject, ArObjectViewController>();
    private readonly Dictionary<string, GameObject> modelPrefabCache = new Dictionary<string, GameObject>();
    private readonly HashSet<string> lockedObjectIds = new HashSet<string>();
    
    private Transform workspaceTransform;
    private Coroutine placementCoroutine;
    private bool isInitialized;
    private int pendingArViewInitializations = 0;

    private void Awake()
    {
        InitializeSubscriptions();
    }

    private void Start()
    {
        lockingManager = GetComponent<ProtocolItemLockingManager>();
    }

    private void OnDisable()
    {
        ClearScene(true);
    }

    private void InitializeSubscriptions()
    {
        if (ProtocolState.Instance == null) return;

        ProtocolState.Instance.ProtocolStream
            .Subscribe(protocol => HandleProtocolChange(protocol))
            .AddTo(this);

        // ProtocolState.Instance.StepStream
        //     .Subscribe(_ => UpdateArActions())
        //     .AddTo(this);

        ProtocolState.Instance.ChecklistStream
            .Subscribe(_ => UpdateArActions())
            .AddTo(this);
        HandleProtocolChange(ProtocolState.Instance.ActiveProtocol.Value);
    }

    private void HandleProtocolChange(ProtocolDefinition protocol)
    {
        ClearScene(true);
        if (protocol?.globalArObjects != null)
        {
            pendingArViewInitializations = protocol.globalArObjects.Count;
            InitializeArObjects(protocol.globalArObjects);
        }
    }

    private void InitializeArObjects(List<ArObject> arObjects)
    {
        if (!TryGetWorkspaceTransform()) return;

        foreach (var arObject in arObjects)
        {
            if (ValidateArObject(arObject))
            {
                CreateArView(arObject);
            }
        }
        isInitialized = true;
    }

    private bool TryGetWorkspaceTransform()
    {
        workspaceTransform = SessionManager.instance?.CharucoTransform;
        if (workspaceTransform == null)
        {
            Debug.LogError("WorkspaceTransform not found");
            return false;
        }
        return true;
    }

    private bool ValidateArObject(ArObject arObject)
    {
        if (string.IsNullOrEmpty(arObject.rootPrefabName))
        {
            Debug.LogWarning($"Invalid ArObject: Missing rootPrefabName");
            return false;
        }
        return true;
    }

    private void CreateArView(ArObject arObject)
    {
        var prefabPath = $"Models/{arObject.rootPrefabName}.prefab";
        
        ServiceRegistry.GetService<IMediaProvider>()?.GetPrefab(prefabPath)
            .Subscribe(
                prefab => InstantiateArView(prefab, arObject),
                error => Debug.LogError($"[ArObjectManager] Failed to load prefab {prefabPath}: {error}")
            )
            .AddTo(this);
    }

    private void InstantiateArView(GameObject prefab, ArObject arObject)
    {
        if (!prefab.TryGetComponent<ArObjectViewController>(out var arViewPrefab))
        {
            Debug.LogError($"[ArObjectManager] Prefab {prefab.name} missing ArObjectViewController component");
            pendingArViewInitializations--;
            CheckInitializationComplete();
            return;
        }

        if (arViews.ContainsKey(arObject))
        {
            Destroy(arViews[arObject].gameObject);
        }

        var instance = Instantiate(arViewPrefab, workspaceTransform);
        instance.Initialize(arObject, new List<TrackedObject> { new TrackedObject() });
        instance.gameObject.SetActive(false);

        arViews[arObject] = instance;
        modelPrefabCache[arObject.arObjectID] = instance.gameObject;

        pendingArViewInitializations--;
        CheckInitializationComplete();
    }

    private void CheckInitializationComplete()
    {
        if (pendingArViewInitializations == 0)
        {
            isInitialized = true;
            UpdateArActions();
        }
    }

    private CheckItemDefinition previousCheckItem;
    private void UpdateArActions()
    {
        if(previousCheckItem != null && previousCheckItem == ProtocolState.Instance.CurrentCheckItemDefinition) return;
        previousCheckItem = ProtocolState.Instance.CurrentCheckItemDefinition;

        if (!isInitialized || !ProtocolState.Instance.HasCurrentCheckItem() || ProtocolState.Instance.CurrentStepState.Value.SignedOff.Value) return;

        var currentCheckItem = ProtocolState.Instance.CurrentCheckItemDefinition;
        if (currentCheckItem == null) return;

        ProcessArActions(currentCheckItem.arActions);
    }

    private void ProcessArActions(List<ArAction> actions)
    {    
        var lockActions = new List<ArAction>(); //actions.Where(a => a.actionType.ToLower() == "lock").ToList();
        var highlightActions = new Dictionary<string, List<ArAction>>();
        var placementActions = new List<ArAction>();

        foreach (var action in actions)
        {
            switch (action.actionType.ToLower())
            {
                case "lock":
                    lockActions.Add(action);
                    break;
                case "highlight":
                    if (!string.IsNullOrEmpty(action.arObjectID))
                    {
                        if (!highlightActions.ContainsKey(action.arObjectID))
                            highlightActions[action.arObjectID] = new List<ArAction>();
                        highlightActions[action.arObjectID].Add(action);
                    }
                    break;
                case "placement":
                    placementActions.Add(action);
                    break;
            }
        }
        if(lockActions.Count > 0)   
        {
            ProcessLockActions(lockActions);
        }
        if(highlightActions.Count > 0)  
        {
            ProcessHighlightActions(highlightActions);
        }
        if(placementActions.Count > 0)
        {
            ProcessPlacementActions(placementActions);
        }

        if (actions.Count == 0)
        {
            foreach(var arView in arViews)
            {
                if(arView.Value is ModelElementViewController modelView)
                {
                    modelView.disablePreviousHighlight();
                }
            }
        }
    }

    private void ProcessLockActions(List<ArAction> lockActions)
    {
        var objectsToLock = new List<GameObject>();

        foreach (var action in lockActions)
        {
            if (!ValidateLockAction(action, out var arIDList)) continue;

            foreach (string id in arIDList)
            {
                if (modelPrefabCache.TryGetValue(id, out var prefab))
                {
                    objectsToLock.Add(prefab);
                    lockedObjectIds.Add(id);
                }
            }
        }

        if (objectsToLock.Count > 0)
        {
            lockingManager.EnqueueObjects(objectsToLock);
        }
    }

    private bool ValidateLockAction(ArAction action, out List<string> arIDList)
    {
        arIDList = null;

        if (action.properties == null)
        {
            Debug.LogWarning($"Lock action properties are null: {action.arObjectID}");
            return false;
        }

        if (!action.properties.TryGetValue("arIDList", out var arIDListObj) || arIDListObj == null)
        {
            Debug.LogWarning($"Invalid arIDList in lock action: {action.arObjectID}");
            return false;
        }

        // If ArIDs for locking are created at time of Parsing they will be saved as a JArray when reserialized
        // Convert the JSON array to a List<string>
        if (arIDListObj is JArray jArray)
        {
            arIDList = jArray.ToObject<List<string>>();
        }
        else if (arIDListObj is List<object> objList)
        {
            arIDList = objList.Select(x => x?.ToString()).ToList();
        }

        // Filter out empty strings
        arIDList = arIDList?.Where(id => !string.IsNullOrEmpty(id)).ToList();
        
        return arIDList != null && arIDList.Count > 0;
    }

    private void ProcessHighlightActions(Dictionary<string, List<ArAction>> highlightActions)
    {
        foreach (var arView in arViews)
        {
            if (arView.Value is ModelElementViewController modelView)
            {
                var arObjectId = arView.Key.arObjectID;
                if (highlightActions.TryGetValue(arObjectId, out var actions))
                {
                    modelView.HighlightGroup(actions);
                }
                else
                {
                    modelView.disablePreviousHighlight();
                }
            }
        }
    }

    private void ProcessPlacementActions(List<ArAction> placementActions)
    {
        foreach (var action in placementActions)
        {
            if (modelPrefabCache.TryGetValue(action.arObjectID, out var prefab))
            {
                RequestObjectPlacement(prefab);
            }
        }
    }

    private void RequestObjectPlacement(GameObject model)
    {
        if (placementCoroutine != null)
        {
            StopCoroutine(placementCoroutine);
        }
        placementCoroutine = StartCoroutine(StartObjectPlacement(model));
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

    private void ClearScene(bool clearLockedObjects = false)
    {
        foreach (var view in arViews.Values)
        {
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }
        
        arViews.Clear();
        modelPrefabCache.Clear();
        
        if (clearLockedObjects)
        {
            lockedObjectIds.Clear();
            ProtocolState.Instance.AlignmentTriggered.Value = false;
        }
        
        isInitialized = false;
    }


    private void UpdateArActionsForObject(ArObject arObject, ArObjectViewController instance)
    {
        if (!ProtocolState.Instance.HasCurrentCheckItem()) return;

        var currentCheckItem = ProtocolState.Instance.CurrentCheckItemDefinition;
        if (currentCheckItem == null) return;

        // Return early if check item hasn't changed
        if (previousCheckItem != null && previousCheckItem == currentCheckItem) return;
        previousCheckItem = currentCheckItem;

        // Filter actions that target this specific object
        var relevantActions = currentCheckItem.arActions
            .Where(action => action.arObjectID == arObject.arObjectID)
            .ToList();

        if (relevantActions.Count > 0)
        {
            ProcessArActions(relevantActions);
        }
        foreach(var arView in arViews)
        {
            if(arView.Key.arObjectID != arObject.arObjectID)
            {
                if(arView.Value is ModelElementViewController modelView)
                {
                    modelView.disablePreviousHighlight();
                }
            }
        }
    }
}