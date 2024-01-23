using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class ProtocolMenuViewController : MonoBehaviour
{
    List<ProcedureDescriptor> protocols;
    List<ProtocolMenuButton> buttons = new List<ProtocolMenuButton>();

    public GridLayoutGroup buttonGrid;
    public GameObject buttonPrefab;

    private void OnEnable()
    {
        LoadProtocols();
    }

    private void OnDisable()
    {
        //foreach (var buttton in buttons)
        //{
        //    if (button != null)
        //    {
        //        Destroy(button.gameObject);
        //    }
        //}
        //buttons.Clear();
    }

    void Build(List<ProcedureDescriptor> protocols, Action<ProcedureDescriptor> onSelected, Action<ProcedureDescriptor> onHeld)
    {
        // Destroy design time data,
        // Note that tileObjectCollection.UpdateCollection may run before the design time objects are destroyed
        // Making objects inactive so no one notices
        for (int i = 0; i < buttonGrid.transform.childCount; i++)
        {
            buttonGrid.transform.GetChild(i).gameObject.SetActive(false);
            Destroy(buttonGrid.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < protocols.Count; i++)
        {
            var protocol = protocols[i];
            var button = Instantiate(buttonPrefab, buttonGrid.transform);
            ProtocolMenuButton buttonScript = button.GetComponent<ProtocolMenuButton>();
            buttonScript.title.text = protocol.title;
            buttonScript.description.text = protocol.description;
            buttonScript.OnClick += () => {
                ServiceRegistry.Logger.Log("Select procedure " + protocol.title);

                if (String.IsNullOrEmpty(protocol.title))
                {
                    ProtocolState.SetProcedureDefinition(null);
                    return;
                }

                ServiceRegistry.GetService<IProcedureDataProvider>().GetOrCreateProcedureDefinition(protocol.title).First().Subscribe(procedure =>
                {
                    ProtocolState.SetProcedureDefinition(procedure);
                }, (e) =>
                {
                    Debug.Log("Error fetching procedure");
                    // TODO retry?!
                });

                //GO TO PROCEDURE SCENE

                ProtocolState.SetProcedureTitle(protocol.title);
            };

            buttons.Add(buttonScript);
        }
    }

    async void LoadProtocols()
    {
        // Load procedure list
        if(ServiceRegistry.GetService<IProcedureDataProvider>() != null)
        {
            protocols = await ServiceRegistry.GetService<IProcedureDataProvider>()?.GetProcedureList();

            // Build menu
            Build(protocols, pi => selectProtocol(pi.name), pi => deleteProtocol(pi.name));
        }
        else
        {
            Debug.LogWarning("Cannot load protocols, protocol data provider service NULL");
        }
    }

    void selectProtocol(string protocolTitle)
    {
        ServiceRegistry.Logger.Log("Select protocol " + protocolTitle);

        if (String.IsNullOrEmpty(protocolTitle))
        {
            ProtocolState.SetProcedureDefinition(null);
            return;
        }

        ServiceRegistry.GetService<IProcedureDataProvider>().GetOrCreateProcedureDefinition(protocolTitle).First().Subscribe(protocol =>
        {
            ProtocolState.SetProcedureDefinition(protocol);
        }, (e) =>
        {
            Debug.Log("Error fetching procedure");
            // TODO retry?!
        });

        //GO TO PROCEDURE SCENE

        ProtocolState.SetProcedureTitle(protocolTitle);
    }

    void deleteProtocol(string protocolTitle)
    {
        ServiceRegistry.Logger.Log("Delete protocol " + protocolTitle);

        // Save the .csv
        var lfdp = new LocalFileDataProvider();

        lfdp.DeleteProcedureDefinition(protocolTitle);
    }
}
