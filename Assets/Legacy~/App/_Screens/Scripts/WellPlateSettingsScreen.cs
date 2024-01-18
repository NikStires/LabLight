using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using UniRx;
using TMPro;
using System.Linq;

public class WellPlateSettingsScreen : ScreenViewController
{
    [SerializeField]
    private GameObject examplePlate;
    private Color green = new Color(0,1,0.2862745f,1);
    private Color gray = new Color(1,1,1,0.3058824f);
    [SerializeField]
    private Transform rowInd;
    [SerializeField]
    private Transform colInd;
    [SerializeField]
    private Transform rowIndHighlight;
    [SerializeField]
    private Transform colIndHighlight;
    [SerializeField]
    private Transform rowHighlights;
    [SerializeField]
    private Transform colHighlights;
    [SerializeField]
    private Transform informationPanel;
    [SerializeField]
    private Transform showBB;
    // [SerializeField]
    // private Transform reservoir;
    // [SerializeField]
    // private Transform tubeContents;
    // [SerializeField]
    // private Transform tubeCaps;


    [SerializeField]
    private Interactable showRowColIndicatorsInteractable; 

    [SerializeField]
    private Interactable showRowColIndicatorHighlightInteractable; //changes indicator to the current color, disabling this will also disable row & column indicators

    [SerializeField]
    private Interactable showRowColHighlightsInteractable; //gray boxes that highlight the rows & columns

    [SerializeField]
    private Interactable showInformationPanelInteractable; 

    [SerializeField]
    private Interactable showBBInteractable;

    [SerializeField]
    private Interactable showTubeContentsInteractable;

    [SerializeField]
    private Interactable showTubeCapsInteractable;


    // Start is called before the first frame update
    void Start()
    {
        //Update UI by subscribing to changes
        SessionState.ShowRowColIndicators.Subscribe(value =>
        {
            showRowColIndicatorsInteractable.IsToggled = value;
            rowInd.gameObject.SetActive(value);
            colInd.gameObject.SetActive(value);
        }).AddTo(this);

        SessionState.ShowRowColIndicatorHighlight.Subscribe(value =>
        {
            showRowColIndicatorHighlightInteractable.IsToggled = value;
            if(value)
            {
                rowIndHighlight.GetComponent<TextMeshProUGUI>().color = green;
                colIndHighlight.GetComponent<TextMeshProUGUI>().color = green;
            }
            else
            {
                rowIndHighlight.GetComponent<TextMeshProUGUI>().color = gray;
                colIndHighlight.GetComponent<TextMeshProUGUI>().color = gray;
            }
        }).AddTo(this);

        SessionState.ShowRowColHighlights.Subscribe(value =>
        {
            showRowColHighlightsInteractable.IsToggled = value;
            rowHighlights.gameObject.SetActive(value);
            colHighlights.gameObject.SetActive(value);
        }).AddTo(this);

        SessionState.ShowInformationPanel.Subscribe(value =>
        {
            showInformationPanelInteractable.IsToggled = value;
            informationPanel.gameObject.SetActive(value);
        }).AddTo(this);

        SessionState.ShowBB.Subscribe(value =>
        {
            showBBInteractable.IsToggled = value;
            showBB.gameObject.SetActive(value);
        }).AddTo(this);

        SessionState.ShowTubeContents.Subscribe(value => 
        {
            showTubeContentsInteractable.IsToggled = value;
            //set up tube rack
        }).AddTo(this);

        SessionState.ShowTubeCaps.Subscribe(value => 
        {
            showTubeCapsInteractable.IsToggled = value;
            //set up tube rack
        }).AddTo(this);

        //update values by setting click handlers
        showRowColIndicatorsInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowRowColIndicators.Value = showRowColIndicatorsInteractable.IsToggled; });
        
        showRowColIndicatorHighlightInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowRowColIndicatorHighlight.Value = showRowColIndicatorHighlightInteractable.IsToggled; });
        
        showRowColHighlightsInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowRowColHighlights.Value = showRowColHighlightsInteractable.IsToggled; });
            
        showInformationPanelInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowInformationPanel.Value = showInformationPanelInteractable.IsToggled; });
        
        showBBInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowBB.Value = showBBInteractable.IsToggled; });

        showTubeContentsInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowTubeContents.Value = showTubeContentsInteractable.IsToggled; });

        showTubeCapsInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowTubeCaps.Value = showTubeCapsInteractable.IsToggled; });    
        if(ProtocolState.procedureDef.Value == null || ProtocolState.procedureDef.Value.globalArElements.Where(ar => (ar.arDefinitionType == ArDefinitionType.Model && ((ModelArDefinition)ar).url.Contains("wellplate"))).FirstOrDefault() == null)
        {
            examplePlate.SetActive(true);
        }else
        {
            examplePlate.SetActive(false);
        }
    }

    // Update is called once per frame
    public void GoBack()
    {
        examplePlate.SetActive(false);
        SessionManager.Instance.GoBack();
    }
}
