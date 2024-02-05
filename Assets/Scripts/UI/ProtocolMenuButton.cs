using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UniRx;

public class ProtocolMenuButton : SpatialUIButton
{
    public void Initialize(ProcedureDescriptor protocol)
    {
        this.protocol = protocol;
        title.text = protocol.title;
        description.text = protocol.description;

        GetComponent<XRSimpleInteractable>().onSelectEntered.AddListener((XRBaseInteractor interactor) => {
            ServiceRegistry.GetService<IProcedureDataProvider>().GetOrCreateProcedureDefinition(protocol.title).First().Subscribe(procedure =>
            {
                Debug.Log(procedure.title + " loaded");
                //ServiceRegistry.Logger.Log("Select procedure " + protocol.title);
                ProtocolState.SetProcedureDefinition(procedure);
                //TODO: Go to procedure scene
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
