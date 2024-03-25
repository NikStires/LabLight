using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using TMPro;
using UniRx;

/// <summary>
/// Represents a button in the protocol menu.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class ProtocolMenuButton : MonoBehaviour
{
    /// <summary>
    /// Initializes the protocol menu button with the specified protocol.
    /// </summary>
    /// <param name="protocol">The protocol to initialize the button with.</param>
    public void Initialize(ProcedureDescriptor protocol)
    {
        this.protocol = protocol;
        title.text = protocol.title;
        description.text = protocol.description;

        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().selectEntered.AddListener(_ => {
            ServiceRegistry.GetService<IProcedureDataProvider>().GetOrCreateProcedureDefinition(protocol.title).First().Subscribe(protocol =>
            {
                Debug.Log(protocol.title + " loaded");
                SessionState.Instance.activeProtocol = protocol;
                SceneLoader.Instance.LoadSceneClean("Protocol");
            }, (e) =>
            {
                Debug.Log("Error fetching procedure");
                // TODO retry?!
            });
        });
    }

    private ProcedureDescriptor protocol;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
}
