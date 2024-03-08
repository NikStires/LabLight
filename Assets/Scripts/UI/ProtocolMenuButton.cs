using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using TMPro;
using UniRx;

[RequireComponent(typeof(XRSimpleInteractable))]
public class ProtocolMenuButton : MonoBehaviour
{
    public void Initialize(ProcedureDescriptor protocol)
    {
        this.protocol = protocol;
        title.text = protocol.title;
        description.text = protocol.description;

        GetComponent<XRSimpleInteractable>().onSelectEntered.AddListener((XRBaseInteractor interactor) => {
            ServiceRegistry.GetService<IProcedureDataProvider>().GetOrCreateProcedureDefinition(protocol.title).First().Subscribe(protocol =>
            {
                Debug.Log(protocol.title + " loaded");
                //ServiceRegistry.Logger.Log("Select procedure " + protocol.title);
                SessionState.Instance.activeProtocol = protocol;
                SceneLoader.Instance.LoadNewScene("Protocol");
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
