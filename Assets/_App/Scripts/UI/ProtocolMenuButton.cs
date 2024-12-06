using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.SceneManagement;
using TMPro;
using UniRx;
using MoreMountains.Feedbacks;
using Newtonsoft.Json;

/// <summary>
/// Represents a button in the protocol menu.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class ProtocolMenuButton : MonoBehaviour
{
    private ProtocolDescriptor protocolDescriptor;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    private XRSimpleInteractable interactable;
    private Renderer buttonRenderer;
    private Material defaultMaterial;
    [SerializeField] private Material progressFillMaterial;
    private float fillProgress = -0.09f;
    [SerializeField] private MMF_Player animationPlayer;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        buttonRenderer = GetComponent<Renderer>();
        defaultMaterial = buttonRenderer.material;
        animationPlayer = GetComponent<MMF_Player>();
    }

    void OnEnable()
    {
        animationPlayer.PlayFeedbacks();
    }

    /// <summary>
    /// Initializes the protocol menu button with the specified protocol descriptor.
    /// </summary>
    /// <param name="protocolDescriptor">The protocol descriptor to initialize the button with.</param>
    public void Initialize(ProtocolDescriptor protocolDescriptor)
    {
        this.protocolDescriptor = protocolDescriptor;
        titleText.text = protocolDescriptor.title;
        descriptionText.text = protocolDescriptor.description.Length > 100 
            ? protocolDescriptor.description.Substring(0, 97) + "..." 
            : protocolDescriptor.description;

        interactable.selectEntered.AddListener(_ => {
            StartCoroutine(ChangeMaterialAfterDelay(1f));
            InvokeRepeating(nameof(IncrementProgressFill), 1.1f, 0.05f);
        });

        interactable.selectExited.AddListener(_ => {
            CancelInvoke();
            buttonRenderer.material = defaultMaterial;
            
            string protocolDescriptorJson = JsonConvert.SerializeObject(protocolDescriptor);
            ServiceRegistry.GetService<IUIDriver>().ProtocolSelectionCallback(protocolDescriptorJson);
        });
    }

    private IEnumerator ChangeMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        buttonRenderer.material = progressFillMaterial;
        fillProgress = -0.09f;
        buttonRenderer.material.SetFloat("_FillRate", fillProgress);
    }

    private void IncrementProgressFill()
    {
        buttonRenderer.material.SetFloat("_FillRate", fillProgress += 0.0075f);
        if(fillProgress >= 0.09f)
        {
            interactable.selectExited.RemoveAllListeners();
            var localFileProvider = new LocalFileDataProvider();
            localFileProvider.DeleteProtocolDefinition(titleText.text);
            Destroy(gameObject);
        }
    }
}
