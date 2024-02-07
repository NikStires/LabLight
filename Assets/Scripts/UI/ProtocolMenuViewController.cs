using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class ProtocolMenuViewController : MonoBehaviour
{
    [SerializeField] GridLayoutGroup buttonGrid;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] GameObject previousButton;
    [SerializeField] GameObject nextButton;
    [SerializeField] Transform transformControls;

    private int currentPage = 0;
    private int maxPage = 0;
    List<ProcedureDescriptor> protocols;
    List<ProtocolMenuButton> buttons = new List<ProtocolMenuButton>();

    private void Start()
    {
        if(SessionState.Instance.mainPanelPosition != null)
        {
            transformControls.position = SessionState.Instance.mainPanelPosition;
            transformControls.eulerAngles = SessionState.Instance.mainPanelRotation;
        }
        LoadProtocols();
    }

    private void OnDisable()
    {
        foreach (var button in buttons)
        {
           if (button != null)
           {
               Destroy(button.gameObject);
           }
        }
        buttons.Clear();
    }

    private void OnDestroy() 
    {
        Debug.Log("ProtocolMenuViewController destroyed");
        SessionState.Instance.mainPanelPosition = transformControls.position;
        SessionState.Instance.mainPanelRotation = transformControls.eulerAngles;
    }

    public void NextPage()
    {
        if (currentPage < maxPage - 1)
        {
            currentPage++;
            Build(currentPage);
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            Build(currentPage);
        }
    }

    void Build(int pageNum)
    {
        //Destroy current page
        for (int i = 0; i < buttonGrid.transform.childCount; i++)
        {
            buttonGrid.transform.GetChild(i).gameObject.SetActive(false);
            Destroy(buttonGrid.transform.GetChild(i).gameObject);
        }


        //build the requested page
        for (int i = currentPage * 8; i < Math.Min((currentPage + 1) * 8, protocols.Count); i++)
        {
            var protocol = protocols[i];
            var button = Instantiate(buttonPrefab, buttonGrid.transform);
            ProtocolMenuButton buttonScript = button.GetComponent<ProtocolMenuButton>();
            buttons.Add(buttonScript);
            buttonScript.Initialize(protocol);
        }

        // Activate or deactivate previous and next buttons
        previousButton.SetActive(currentPage > 0);
        nextButton.SetActive(currentPage < maxPage - 1);
    }

    async void LoadProtocols()
    {
        // Load procedure list
        if(ServiceRegistry.GetService<IProcedureDataProvider>() != null)
        {
            protocols = await ServiceRegistry.GetService<IProcedureDataProvider>()?.GetProcedureList();
            maxPage = (int)Math.Ceiling((float)protocols.Count / 8);
            currentPage = 0;

            // Build page 1
            Build(currentPage);
        }
        else
        {
            Debug.LogWarning("Cannot load protocols, protocol data provider service NULL");
        }
    }

    void deleteProtocol(string protocolTitle)
    {
        ServiceRegistry.Logger.Log("Delete protocol " + protocolTitle);

        // Save the .csv
        var lfdp = new LocalFileDataProvider();

        lfdp.DeleteProcedureDefinition(protocolTitle);
    }
}
