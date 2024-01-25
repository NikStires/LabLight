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
public class SourceElementViewController : ModelElementViewController
{
    public bool debugeEnableAllSettings = true;

    public bool modelActive;

    //public bool isSource; //if object only acts as a source
    //if source -> fill in Sources & nametag objects

    [SerializeField]
    public Transform nameTags;

    [SerializeField]
    public Transform Sources;

    [SerializeField]
    public Transform Outline;

    public List<HighlightAction> currActions;

    private bool disableComponents = false;


    private bool alignmentTriggered;

    private int prevCheckItem = 0;

    void Awake()
    {
        ProtocolState.checklistStream.Subscribe(_ => OnCheckItemChanged()).AddTo(this);
        ProtocolState.stepStream.Subscribe(Step => OnStepChanged(Step)).AddTo(this);
    }


    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);

        if(ModelName != null)
        {
            ModelName.GetComponent<TextMeshProUGUI>().text = ((ModelArDefinition)arDefinition).name;
            ModelName.gameObject.SetActive(true);
        }
        //if source instantiate colors and nametags
        if(Sources != null)
        {
            if(((ModelArDefinition)arDefinition).contentsToColors.Count() > 0)
            {
                int count = 0;
                foreach(string contents in ((ModelArDefinition)arDefinition).contentsToColors.Keys)
                {
                    if(nameTags != null)
                    {
                        nameTags.Find(Convert.ToString(count)).Find("Contents").GetComponent<TextMeshProUGUI>().text = contents.Contains(":") ? contents.Substring(contents.IndexOf(':') + 1) : contents;
                    }

                    if(Sources.childCount > 1) //if object is multiSource
                    {
                        Color parsedColor;
                        if(ColorUtility.TryParseHtmlString(((ModelArDefinition)arDefinition).contentsToColors[contents], out parsedColor))
                        {
                            parsedColor.a = 125;
                            Sources.Find(Convert.ToString(count)).GetComponent<Renderer>().material.SetColor("_Color", parsedColor);
                        }
                    }
                    count++;
                }
            }
        }
        AddSubscriptions();
    }

    public override void AlignmentGroup()
    {
        if(!disableComponents)
        {
            alignmentTriggered = true;
            //toggleTransform(ModelName, true);
            if(nameTags != null)
            {
                foreach(Transform nametag in nameTags)
                {
                    if(nametag.Find("Contents").GetComponent<TextMeshProUGUI>().text != "")
                    {
                        toggleTransform(nametag, true);
                        toggleTransform(Sources, true, nametag.name);
                    }
                }
            }
        }
    }
    //resets model back to previous highlight if there is one
    public override void ResetToCurrentHighlights()
    {
        if(!disableComponents)
        {
            alignmentTriggered = false;
            //toggleTransform(ModelName, false);
            if(nameTags != null)
            {
                foreach(Transform nametag in nameTags)
                {
                    if(nametag.Find("Contents").GetComponent<TextMeshProUGUI>().text != "")
                    {
                        toggleTransform(nametag, false);
                        toggleTransform(Sources, false, nametag.name);
                    }
                }
            }

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
            if(actions.Count() == 2) //usually dealing with transfer step on the same plate
            {
                enableHighlight(actions[1]);
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
        }
    }

    private void enableHighlight(HighlightAction action)
    {
        if(disableComponents)
        {
            toggleActiveComponents(true);
            disableComponents = false;
        }
        foreach(string id in action.chainIDs)
        {
            if(debugeEnableAllSettings)
            {
                toggleTransform(Sources, true, id);
                toggleTransform(nameTags, true, id);
            }
            else
            {
                toggleTransform(Sources, SessionState.ShowSourceTransform.Value, id);
                toggleTransform(nameTags, SessionState.ShowSourceContents.Value, id);
            }
        }
    }

    //new imp
    private void disableHighlight(HighlightAction action)
    {
        foreach(string id in action.chainIDs)
        {
            toggleTransform(Sources, false, id);
            toggleTransform(nameTags, false, id);
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
                    if(childObject.TryGetComponent<Renderer>(out Renderer ren))
                    {
                        ren.material.SetColor("_Color", color);
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

    private void toggleActiveComponents(bool value)
    {
        if(currActions != null)
        {
            foreach(HighlightAction action in currActions)
            {
                foreach(string id in action.chainIDs)
                {
                    if (debugeEnableAllSettings)
                    {
                        toggleTransform(Sources, (true && value), id);
                        toggleTransform(nameTags, (true && value), id);
                    }
                    else
                    {
                        toggleTransform(Sources, (SessionState.ShowSourceTransform.Value && value), id);
                        toggleTransform(nameTags, (SessionState.ShowSourceContents.Value && value), id);
                    }
                }
            }
        }
        toggleTransform(ModelName, value);
    }

    void OnCheckItemChanged()
    {
        if (!disableComponents && ProtocolState.CheckItem == ProtocolState.procedureDef.steps[ProtocolState.Step].checklist.Count()) //if on last checked item disable all active components
        {
            //play audio for last event completed
            toggleActiveComponents(false);
            disableComponents = true;
        }
        else if(disableComponents)
        {
            toggleActiveComponents(true);
            disableComponents = false;
        }
    }

    void OnStepChanged(ProtocolState.StepState step)
    {
        prevCheckItem = step.CheckNum;
    }

    private void AddSubscriptions()
    {

        SessionState.ShowSourceContents.Subscribe(value => 
        {
            if(currActions != null)
            {
                foreach(HighlightAction action in currActions)
                {
                    foreach(string id in action.chainIDs)
                    {
                        toggleTransform(nameTags, value, id);
                    }
                }
            }
        }).AddTo(this);

        SessionState.ShowSourceTransform.Subscribe(value => 
        {
            if(currActions != null)
            {
                foreach(HighlightAction action in currActions)
                {
                    foreach(string id in action.chainIDs)
                    {
                        toggleTransform(nameTags, value, id);
                    }
                }
            }
        }).AddTo(this);
    }
    

}
