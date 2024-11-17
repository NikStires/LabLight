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
    public SettingsManagerScriptableObject settingsManagerSO;
    public bool debugeEnableAllSettings = false;

    public bool modelActive;

    //public bool isSource; //if object only acts as a source
    //if source -> fill in Sources & nametag objects

    [SerializeField]
    public Transform nameTags;

    [SerializeField]
    public Transform Sources;

    [SerializeField]
    public Transform Outline;

    public List<ArAction> currActions;
    private List<ArAction> activeHighlights = new List<ArAction>();

    private bool disableComponents = false;


    private bool alignmentTriggered;

    private int prevCheckItem = 0;

    void Awake()
    {
        ProtocolState.Instance.ChecklistStream.Subscribe(_ => OnCheckItemChanged()).AddTo(this);
        ProtocolState.Instance.StepStream.Subscribe(Step => OnStepChanged(Step)).AddTo(this);
    }


    public override void Initialize(ArObject arObject, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arObject, trackedObjects);

        if (ModelName != null)
        {
            ModelName.GetComponent<TextMeshProUGUI>().text = arObject.specificObjectName;
            ModelName.gameObject.SetActive(true);
        }

        if (Sources != null)
        {
            var contentsToColors = arObject.Properties?.GetValueOrDefault("contentsToColors", 
                new Dictionary<string, string>());

            if (contentsToColors?.Count > 0)
            {
                int count = 0;
                foreach (var contentPair in contentsToColors)
                {
                    string contents = contentPair.Key;
                    string colorHex = contentPair.Value;

                    if (nameTags != null)
                    {
                        nameTags.Find(Convert.ToString(count))
                            .Find("Contents")
                            .GetComponent<TextMeshProUGUI>().text = 
                            contents.Contains(":") ? contents.Substring(contents.IndexOf(':') + 1) : contents;
                    }

                    if (Sources.childCount > 1)
                    {
                        if (ColorUtility.TryParseHtmlString(colorHex, out Color parsedColor))
                        {
                            parsedColor.a = 125;
                            Sources.Find(Convert.ToString(count))
                                .GetComponent<Renderer>()
                                .material.SetColor("_Color", parsedColor);
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
    public override void HighlightGroup(List<ArAction> actions)
    {
        if (actions == null || actions.Count == 0 || alignmentTriggered)
            return;

        // Reset disableComponents when new highlights are enabled
        disableComponents = false;

        // Disable previous highlights first
        disablePreviousHighlight();

        modelActive = true;
        activeHighlights = actions;
        currActions = actions;  // Ensure currActions is updated

        foreach (var action in actions)
        {
            EnableHighlight(action);
        }
    }

    private void EnableHighlight(ArAction action)
    {
        if (action?.Properties == null) return;

        // Cache property lookups
        var chainIDs = action.Properties.GetValueOrDefault("subIDs", new List<string>());
        var colorHex = action.Properties.GetValueOrDefault("colorHex", "#FFFFFF").ToString();

        foreach (string id in chainIDs)
        {
            if (debugeEnableAllSettings)
            {
                toggleTransform(Sources, true, id);
                toggleTransform(nameTags, true, id);
            }
            else
            {
                toggleTransform(Sources, settingsManagerSO.GetSettingValue(LablightSettings.Source_Container), id);
                toggleTransform(nameTags, settingsManagerSO.GetSettingValue(LablightSettings.Source_Contents), id);
            }
        }
    }

    public override void disablePreviousHighlight()
    {
        if (activeHighlights == null || activeHighlights.Count == 0) return;

        modelActive = false;

        foreach (var action in activeHighlights)
        {
            DisableHighlight(action);
        }

        activeHighlights.Clear();
    }

    private void DisableHighlight(ArAction action)
    {
        if (action?.Properties == null) return;

        var chainIDs = action.Properties.GetValueOrDefault("subIDs", new List<string>());

        foreach (string id in chainIDs)
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
        if (currActions != null)
        {
            foreach (var action in currActions)
            {
                var chainIDs = action.Properties.GetValueOrDefault("subIDs", new List<string>());
                foreach (string id in chainIDs)
                {
                    if (debugeEnableAllSettings)
                    {
                        toggleTransform(Sources, (true && value), id);
                        toggleTransform(nameTags, (true && value), id);
                    }
                    else
                    {
                        toggleTransform(Sources, settingsManagerSO.GetSettingValue(LablightSettings.Source_Container) && value, id);
                        toggleTransform(nameTags, settingsManagerSO.GetSettingValue(LablightSettings.Source_Contents) && value, id);
                    }
                }
            }
        }
        toggleTransform(ModelName, value);
    }

    void OnCheckItemChanged()
    {
        if(!ProtocolState.Instance.HasCurrentChecklist())
        {
            return;
        }
        if (!disableComponents && ProtocolState.Instance.CurrentCheckNum == ProtocolState.Instance.CurrentChecklist.Count()) //if on last checked item disable all active components
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
        prevCheckItem = step.CheckNum.Value;
    }

    private void AddSubscriptions()
    {
        settingsManagerSO.settingChanged.AddListener(settingChanged =>
        {
            if(currActions != null)
            {
                foreach(HighlightAction action in currActions)
                {
                    foreach(string id in action.chainIDs)
                    {
                        switch(settingChanged.Item1)
                        {
                            case LablightSettings.Source_Container:
                                toggleTransform(Sources, settingChanged.Item2, id);
                                break;
                            case LablightSettings.Source_Contents:
                                toggleTransform(nameTags, settingChanged.Item2, id);
                                break;
                        }
                    }
                }
            }
        });
    }
    

}
