using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UniRx;
using TMPro;


//TODO
//variable to verify if object is currently active in scene

[RequireComponent(typeof(CapsuleCollider))]
public class WellPlateViewController : ModelElementViewController
{
    public bool debugEnableAllSettings = true;
    
    public bool modelActive;

    [SerializeField]
    public HighlightGroup wellPlateAlignmentGroup;

    public bool isSource; //if object only acts as a source
    //if source -> fill in Sources & nametag objects only

    [SerializeField]
    public Transform Markers;

    public Transform Markers2D;

    public Transform rowIndicators;
    public Transform rowHighlights;

    public Transform colIndicators;
    public Transform colHighlights;

    [SerializeField]
    public Transform Plate2D;

    [SerializeField]
    public Transform infoPanel;

    [SerializeField]
    public Color defaultIndicatorColor;

    public Transform Cover;

    public Transform Outline;

    public List<HighlightAction> currActions;

    private bool disableComponents = false;

    private bool alignmentTriggered;

    private int prevCheckItem = 0;

    void Awake()
    {
        ProtocolState.stepStream.Subscribe(Step => OnStepChanged(Step)).AddTo(this);
        ProtocolState.checklistStream.Subscribe(_ => OnCheckItemChanged()).AddTo(this);
    }

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);

        if(ModelName != null)
        {
            ModelName.GetComponent<TextMeshProUGUI>().text = ((ModelArDefinition)arDefinition).name;
            ModelName.gameObject.SetActive(true);
        }
        AddSubscriptions();
    }

    private void InitializeMarkers2D()
    {
        bool firstHighlight = true;
        if(Markers2D != null && ((ModelArDefinition)arDefinition).name.Contains("extraction"))
        {
            Debug.Log("initalizing 2d markers for " + ((ModelArDefinition)arDefinition).name);
            toggleTransform(Plate2D, true);
            //deactivate all markers
            foreach(Transform marker in Markers2D)
            {
                marker.gameObject.SetActive(false);
            }
            //activate markers for wells to be highlighted
            foreach(var checkItem in ProtocolState.procedureDef.steps[ProtocolState.Step].checklist)
            {
                foreach(HighlightArOperation highlight in checkItem.operations.Where(op => op.arOperationType == ArOperationType.Highlight))
                {
                    if(highlight.highlightActions != null)
                    {
                        foreach(var highlightAction in highlight.highlightActions)
                        {
                            foreach(string id in highlightAction.chainIDs)
                            {
                                if(highlightAction.isSource && Markers2D != null)
                                {
                                    if(firstHighlight)
                                    {
                                        toggleTransform(Markers2D, true, id, Color.green);
                                        firstHighlight = false;
                                    }
                                    else
                                    {
                                        toggleTransform(Markers2D, true, id, Color.blue);
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
    }

    public override void AlignmentGroup()
    {
        if(!disableComponents)
        {
            alignmentTriggered = true;
            wellPlateAlignmentGroup.SubParts.ForEach(subPart => subPart.SetActive(true));
        }
    }
    //resets model back to previous highlight if there is one
    public override void ResetToCurrentHighlights()
    {
        if(!disableComponents)
        {
            alignmentTriggered = false;
            //toggleTransform(ModelName, false);
            wellPlateAlignmentGroup.SubParts.ForEach(subPart => subPart.SetActive(false));

            if(currActions != null && modelActive)
            {
                HighlightGroup(currActions);
            }
        }
    }
            //new imp
    public override void HighlightGroup(List<HighlightAction> actions)
    {

        this.gameObject.SetActive(true); //debug
        if (actions != null && !alignmentTriggered)
        {
            modelActive = true;
            currActions = actions;
            enableHighlight(actions[0]);
            Debug.Log("enabling highlight");
            if (actions.Count() == 2) //usually dealing with transfer step on the same plate
            {
                enableHighlight(actions[1]);
            }
            if (debugEnableAllSettings)
            {
                toggleIndicators(true);
                if (actions[0].chainIDs.Count() > 0)
                {
                    toggleInfoPanel(true, actions);
                }
            }
            else
            {
                toggleIndicators(SessionState.ShowRowColIndicators.Value);
                if (actions[0].chainIDs.Count() > 0)
                {
                    toggleInfoPanel(SessionState.ShowInformationPanel.Value, actions);
                }
            }
        }
    }

    public override void disablePreviousHighlight()
    {
        if(currActions != null)
        {
            modelActive = false;
            disableHighlight(currActions[0]);
            if(currActions.Count() == 2)
            {
                disableHighlight(currActions[1]);
            }
            toggleIndicators(false);
            toggleInfoPanel(false, currActions);
        }
    }

    private void enableHighlight(HighlightAction action)
    {
        if(disableComponents)
        { 
            InitializeMarkers2D();
            toggleActiveComponents(true);
        }
        Color parsedColor;
        if(ColorUtility.TryParseHtmlString(action.colorInfo.Item1, out parsedColor))
        {
            parsedColor.a = 255;
        }
        foreach(string id in action.chainIDs)
        {
            if(action.isSource && Markers2D != null && ((ModelArDefinition)arDefinition).name.Contains("extraction"))
            {
                toggleTransform(Markers2D, true, id, Color.green);
            }
            if(debugEnableAllSettings)
            {
                toggleTransform(Markers, true, id, parsedColor);
                toggleTransform(rowIndicators, true, id.Substring(0, 1), parsedColor);
                toggleTransform(colIndicators, true, id.Substring(1), parsedColor);
                // toggleTransform(rowHighlights, true, id.Substring(0, 1));
                // toggleTransform(colHighlights, true, id.Substring(1));
                Debug.Log("enabling color: " + parsedColor);
                Debug.Log("default color: " + defaultIndicatorColor);
            }
            else
            {
                toggleTransform(Markers, SessionState.ShowMarker.Value, id, parsedColor);
                toggleTransform(rowIndicators, SessionState.ShowRowColIndicatorHighlight.Value, id.Substring(0, 1), parsedColor);
                toggleTransform(colIndicators, SessionState.ShowRowColIndicatorHighlight.Value, id.Substring(1), parsedColor);
                //toggleTransform(rowHighlights, SessionState.ShowRowColHighlights.Value, id.Substring(0, 1));
                //toggleTransform(colHighlights, SessionState.ShowRowColHighlights.Value, id.Substring(1));
            }
        }
    }

    //new imp
    private void disableHighlight(HighlightAction action)
    {
        foreach(string id in action.chainIDs)
        {
            if(action.isSource && Markers2D != null && ((ModelArDefinition)arDefinition).name.Contains("extraction"))
            {
                if(prevCheckItem == ProtocolState.CheckItem + 1)
                {
                    toggleTransform(Markers2D, true, id, Color.blue);
                }
                else
                {
                    toggleTransform(Markers2D, true, id, Color.gray);
                }
                prevCheckItem = ProtocolState.CheckItem;
            }
            toggleTransform(Markers, false, id, Color.green);
            toggleTransform(rowIndicators, true, id.Substring(0,1), defaultIndicatorColor);
            toggleTransform(colIndicators, true, id.Substring(1), defaultIndicatorColor);
            //toggleTransform(rowHighlights, false, id.Substring(0,1));
            //toggleTransform(colHighlights, false, id.Substring(1));
        }
    }

    private void toggleTransform(Transform parentTransform, bool value, string id = "", Color color = default)
    {
        if(parentTransform != null)
        {
            if(!String.IsNullOrEmpty(id))
            {
                GameObject childObject = parentTransform.Find(id).gameObject;
                childObject.SetActive(value);
                if(color != default)
                {
                    if(childObject.TryGetComponent<MeshRenderer>(out MeshRenderer ren))
                    {
                        ren.material.color = color;
                    }
                    else if(childObject.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmp))
                    {
                        tmp.color = color;
                    }
                }
            }else
            {
                parentTransform.gameObject.SetActive(value);
            }
        }
    }

    private void toggleIndicators(bool value)
    {
        if(rowIndicators != null && colIndicators != null)
        {
            foreach(Transform obj in rowIndicators)
            {
                if(debugEnableAllSettings)
                {
                    obj.GetComponent<TextMeshProUGUI>().color = defaultIndicatorColor;
                }
                else if(!SessionState.ShowRowColIndicatorHighlight.Value && SessionState.ShowRowColIndicators.Value)
                {
                    obj.GetComponent<TextMeshProUGUI>().color = defaultIndicatorColor;
                }

                if(obj.GetComponent<TextMeshProUGUI>().color == defaultIndicatorColor)
                {
                    obj.gameObject.SetActive(value);
                }
            }
            foreach(Transform obj in colIndicators)
            {
                if (debugEnableAllSettings)
                {
                    obj.GetComponent<TextMeshProUGUI>().color = defaultIndicatorColor;
                }
                else if (!SessionState.ShowRowColIndicatorHighlight.Value && SessionState.ShowRowColIndicators.Value)
                {
                    obj.GetComponent<TextMeshProUGUI>().color = defaultIndicatorColor;
                }

                if(obj.GetComponent<TextMeshProUGUI>().color == defaultIndicatorColor)
                {
                    obj.gameObject.SetActive(value);
                }
            }
        }

    }


    private void toggleInfoPanel(bool value, List<HighlightAction> actions) //info panel takes in 1-2 actions and prints the necessary info on the information panel based on the information provided in the highlight actions provided
    {
        if(infoPanel != null)
        {
            infoPanel.gameObject.SetActive(value);
            if(value && actions != null)
            {
                string colorHex = actions[0].colorInfo.Item1;
                Color color;
                if(ColorUtility.TryParseHtmlString(actions[0].colorInfo.Item1, out color))
                {
                    //color.a = 255;
                }
                foreach(Transform child in infoPanel.GetComponentInChildren<Transform>())
                {
                    switch(child.name)
                    {
                        //ignoring actions = 2 for now
                        case "InfoDisplay":
                            if(actions[0].actionName.Contains("transfer") && actions.Count() == 1)
                            {
                                if(actions[0].isSource)
                                {
                                    child.GetComponent<TextMeshProUGUI>().text = "Aliquot " + actions[0].volume.Item1.ToString("0.00") + actions[0].volume.Item2 + (actions[0].contents.Item2 != "" ? (" of " + "<" + colorHex + ">" + actions[0].contents.Item2 + "</color>") : "") + (actions[0].isSource ? " from " : " into ") + "<" + colorHex + ">" + "Well " + (actions[0].chainIDs.Count() > 1 ? (actions[0].chainIDs[0] + "-" + actions[0].chainIDs[actions[0].chainIDs.Count()-1]) : actions[0].chainIDs[0]) + "</color>";
                                }
                                else
                                {
                                    child.GetComponent<TextMeshProUGUI>().text = "Transfer " + actions[0].volume.Item1.ToString("0.00") + actions[0].volume.Item2 + (actions[0].contents.Item2 != "" ? (" of " + "<" + colorHex + ">" + actions[0].contents.Item2 + "</color>") : "") + (actions[0].isSource ? " from " : " into ") + "<" + colorHex + ">" + "Well " + (actions[0].chainIDs.Count() > 1 ? (actions[0].chainIDs[0] + "-" + actions[0].chainIDs[actions[0].chainIDs.Count()-1]) : actions[0].chainIDs[0]) + "</color>";
                                }
                            }else
                            {
                                child.GetComponent<TextMeshProUGUI>().text = "Transfer " + actions[0].volume.Item1.ToString("0.00") + actions[0].volume.Item2 + (actions[0].contents.Item2 != "" ? (" of <" + colorHex + ">" + actions[0].contents.Item2 + "</color>") : "") + (actions[0].isSource ? " from " : " into ") + "<" + colorHex + ">" + "Well " + (actions[0].chainIDs.Count() > 1 ? (actions[0].chainIDs[0] + "-" + actions[0].chainIDs[actions[0].chainIDs.Count()-1]) : actions[0].chainIDs[0]) + "</color>";
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    private void toggleActiveComponents(bool value)
    {
        if(currActions != null)
        {
            foreach(HighlightAction action in currActions)
            {
                foreach(string id in action.chainIDs)
                {
                    if (debugEnableAllSettings)
                    {
                        toggleTransform(rowIndicators, (true && value), id.Substring(0, 1));
                        toggleTransform(colIndicators, (true && value), id.Substring(1));
                        //toggleTransform(rowHighlights, (true && value), id.Substring(0, 1));
                        //toggleTransform(colHighlights, (true && value), id.Substring(1));
                        toggleTransform(Markers, (true && value), id);
                    }
                    else
                    {
                        toggleTransform(rowIndicators, (SessionState.ShowRowColIndicatorHighlight.Value && value), id.Substring(0, 1));
                        toggleTransform(colIndicators, (SessionState.ShowRowColIndicatorHighlight.Value && value), id.Substring(1));
                        //toggleTransform(rowHighlights, (SessionState.ShowRowColHighlights.Value && value), id.Substring(0, 1));
                        //toggleTransform(colHighlights, (SessionState.ShowRowColHighlights.Value && value), id.Substring(1));
                        toggleTransform(Markers, (SessionState.ShowMarker.Value && value), id);
                    }
                }
            }
        }
        toggleTransform(ModelName, value);
        if(debugEnableAllSettings)
        {
            toggleInfoPanel((true && value), currActions);
            toggleIndicators((true && value));
        }
        else
        {
            toggleInfoPanel((SessionState.ShowInformationPanel.Value && value), currActions);
            toggleIndicators((SessionState.ShowRowColIndicators.Value && value));
        }
    }

    void OnCheckItemChanged()
    {
        if (!disableComponents && ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count() - 1 && ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value) //if on last checked item disable all active components
        {
            //play audio for check item completed
            toggleActiveComponents(false);
            toggleTransform(Cover, true);
            disableComponents = true;
        }
        else if(disableComponents)
        {
            toggleActiveComponents(true);
            toggleTransform(Cover, false);
            disableComponents = false;
        }
    }

    void OnStepChanged(ProtocolState.StepState step)
    {
        prevCheckItem = step.CheckNum;
        toggleTransform(Cover, false);
    }
    private void AddSubscriptions()
    {
        SessionState.ShowRowColIndicators.Subscribe(value =>
        {
            toggleIndicators(value);
        }).AddTo(this);

        SessionState.ShowRowColIndicatorHighlight.Subscribe(value =>
        {
            if(currActions != null)
            {
                Color parsedColor;

                foreach(HighlightAction action in currActions)
                {
                    if(ColorUtility.TryParseHtmlString(action.colorInfo.Item1, out parsedColor))
                    {
                        parsedColor.a = 255;
                    }
                    foreach(string id in action.chainIDs)
                    {
                        if(!value && SessionState.ShowRowColIndicators.Value) //if indicators should be changed to default color and stay enabled if indicators are enabled
                        {
                            toggleTransform(rowIndicators, true, id.Substring(0,1), defaultIndicatorColor); 
                            toggleTransform(colIndicators, true, id.Substring(1), defaultIndicatorColor);
                        }else
                        {
                            toggleTransform(rowIndicators, value, id.Substring(0,1), parsedColor);
                            toggleTransform(colIndicators, value, id.Substring(1), parsedColor);
                        }
                    }
                }
            }
        }).AddTo(this);

        SessionState.ShowRowColHighlights.Subscribe(value =>
        {
            if(currActions != null)
            {
                foreach(HighlightAction action in currActions)
                {
                    foreach(string id in action.chainIDs)
                    {
                        toggleTransform(rowHighlights, value, id.Substring(0,1));
                        toggleTransform(colHighlights, value, id.Substring(1));
                    }
                }
            }
        }).AddTo(this);

        SessionState.ShowInformationPanel.Subscribe(value =>
        {
            toggleInfoPanel(value, currActions);
        }).AddTo(this);

        SessionState.ShowMarker.Subscribe(value =>
        {
            if(currActions != null)
            {
                Debug.Log("Enabling bbs");
                foreach(HighlightAction action in currActions)
                {
                    Color parsedColor;
                    if(ColorUtility.TryParseHtmlString(action.colorInfo.Item1, out parsedColor))
                    {
                        parsedColor.a = 255;
                    }
                    foreach(string id in action.chainIDs)
                    {
                        toggleTransform(Markers, value, id, parsedColor);
                    }
                }
            }
        }).AddTo(this);
    }
}
