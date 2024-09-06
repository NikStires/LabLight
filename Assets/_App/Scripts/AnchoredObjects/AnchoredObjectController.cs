using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UniRx;
using System.Collections;

/// <summary>
/// AnchoredObjectController is a script that initializes the payload of this GameObject
/// Using the trackableId of the anchor a lookup is performed to find the corresponding LabLight data that is attached to this anchor
/// </summary>
[RequireComponent(typeof(ARAnchor))]
public class AnchoredObjectController : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] HeadPlacementEventChannel headPlacementEventChannel;

    [SerializeField]
    private TextMeshProUGUI debugText;

    [SerializeField]
    private Transform payloadParent;

    private ARAnchor arAnchor;

    public PayloadLookup payloadFactory;

    private static AnchorData allAnchorData;
    private static IAnchorDataProvider anchorDataProvider;

    public void Start()
    {
        arAnchor = this.GetComponent<ARAnchor>();

        if (allAnchorData == null)
        {
            // Find the LabLight data that corresponds to this anchor
            anchorDataProvider = ServiceRegistry.GetService<IAnchorDataProvider>();
            if (anchorDataProvider != null)
            {
                anchorDataProvider.GetOrCreateAnchorData().First().Subscribe((data) =>
                {
                    allAnchorData = data;
                    InitializePayload();
                });
            }
        }
        else
        {
            InitializePayload();
        }
    }

    private void InitializePayload()
    {
        var anchorPayload = FindAnchorPayload(arAnchor.trackableId.ToString());
        if (anchorPayload != null)
        {
            debugText.text = " Found " + arAnchor.trackableId.ToString() + "  of type " + anchorPayload.payloadType;

            Initialize(anchorPayload, false);
        }
        else
        {
            // Wait until initialized at runtime
            debugText.text = "No payload found for " + arAnchor.trackableId.ToString();
        }
    }

    // Initialize from data
    public void Initialize(AnchorPayload payload, bool add)
    {
        AnchorPayloadController payloadPrefab = payloadFactory.FindPrefabToCreate(payload.payloadType);

        if (payloadPrefab != null)
        {
            // Instantiate prefab
            var payloadController = Instantiate(payloadPrefab, payloadParent);
            payloadController.Initialize(payload);
        }
        else
        {
            Debug.LogError("No prefab configure for anchor payloadtype " + payload.payloadType);
        }

        if (add)
        {
            StartCoroutine(Save(payload));
        }
    }

    IEnumerator Save(AnchorPayload payload)
    {
        yield return new WaitUntil(() => allAnchorData != null && arAnchor != null);

        var anchor = new Anchor();
        anchor.id = arAnchor.trackableId.ToString();
        anchor.payload = payload;
        allAnchorData.anchors.Add(anchor);
        SaveAnchorData();
    }

    private static AnchorPayload FindAnchorPayload(string trackableId)
    {
        foreach (var anchorData in allAnchorData.anchors)
        {
            if (anchorData.id.Equals(trackableId))
            {
                return anchorData.payload;
            }
        }
        return null;
    }

    private static Anchor FindAnchor(string trackableId)
    {
        foreach (var anchorData in allAnchorData.anchors)
        {
            if (anchorData.id.Equals(trackableId))
            {
                return anchorData;
            }
        }
        return null;
    }

    public void RemoveAnchoredObject()
    {
        Debug.Log("RemoveAnchoredObject");

        Destroy(this.gameObject);

        var anchor = FindAnchor(arAnchor.trackableId.ToString());
        if (anchor != null)
        {
            allAnchorData.anchors.Remove(anchor);
        }
    }

    public static void SaveAnchorData()
    {
        if (anchorDataProvider != null)
        {
            anchorDataProvider.SaveAnchorData(allAnchorData);
        }
    }

    public void StartPlacementDelayed()
    {
        StartCoroutine(StartPlacement());
    }

    IEnumerator StartPlacement()
    {
        yield return new WaitForSeconds(1.0f);

        //arAnchor.enabled = false;
        headPlacementEventChannel.OnSetHeadtrackedObject(gameObject);
    }

}
