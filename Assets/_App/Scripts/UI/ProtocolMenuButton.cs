using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.SceneManagement;
using TMPro;
using UniRx;
using MoreMountains.Feedbacks;

/// <summary>
/// Represents a button in the protocol menu.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class ProtocolMenuButton : MonoBehaviour
{

    private ProtocolDescriptor protocol;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    XRSimpleInteractable interactable;
    Renderer buttonRenderer;
    Material defaultMaterial;
    [SerializeField] Material redFillShader;
    float progress = -0.09f;
    [SerializeField] MMF_Player animationPlayer;

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
    /// Initializes the protocol menu button with the specified protocol.
    /// </summary>
    /// <param name="protocol">The protocol to initialize the button with.</param>
    public void Initialize(ProtocolDescriptor protocol)
    {
        this.protocol = protocol;
        title.text = Path.GetFileNameWithoutExtension(protocol.title);
        description.text = protocol.description;

        interactable.selectEntered.AddListener(_ => {
            StartCoroutine(ChangeMaterialAfterDelay(1f));
            InvokeRepeating("incrementShaderFill", 1.1f, 0.05f);
        });

        interactable.selectExited.AddListener(_ => {
            ServiceRegistry.GetService<IProtocolDataProvider>().GetOrCreateProtocolDefinition(protocol.title).First().Subscribe(protocol =>
            {
                CancelInvoke();
                buttonRenderer.material = defaultMaterial;
                Debug.Log(protocol.title + " loaded");
                ProtocolState.Instance.SetProtocolDefinition(protocol);
                SceneLoader.Instance.LoadSceneClean("Protocol");
            }, (e) =>
            {
                Debug.Log("Error fetching protocol from resources, checking local files");
                var lfdp = new LocalFileDataProvider();
                lfdp.LoadProtocolDefinitionAsync(protocol.title).ToObservable<ProtocolDefinition>().Subscribe(protocol =>
                {
                    ProtocolState.Instance.SetProtocolDefinition(protocol);
                    SceneLoader.Instance.LoadSceneClean("Protocol");
                }, (e) =>
                {
                    Debug.Log("Error fetching protocol from local files");
                });
            });
        });
    }

    private IEnumerator ChangeMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        buttonRenderer.material = redFillShader;
        progress = -0.09f;
        buttonRenderer.material.SetFloat("_FillRate", progress);
    }

    private void incrementShaderFill()
    {
        buttonRenderer.material.SetFloat("_FillRate", progress += 0.0075f);
        if(progress >= 0.09f)
        {
            interactable.selectExited.RemoveAllListeners();
            var lfdp = new LocalFileDataProvider();
            lfdp.DeleteProtocolDefinition(title.text);
            //ServiceRegistry.GetService<IProtocolDataProvider>().DeleteProtocolDefinition(title.text);
            Destroy(gameObject);
        }
    }
}
