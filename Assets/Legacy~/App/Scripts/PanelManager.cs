using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;
using Sirenix.OdinInspector;
using UniRx;

public class PanelManager : MonoBehaviour
{
    public Transform PanelViews;
    public Transform panelTransforms;
    public GameObject togglePanelButton;
    private Dictionary<string, PanelData> panels = new Dictionary<string, PanelData>();
    public Transform ButtonParent;

    void Awake()
    {
        ProtocolState.procedureStream.Subscribe(_ => OnProtocolLoaded()).AddTo(this);
        this.transform.position = SessionManager.Instance.CharucoTransform.position;
    }

    void OnProtocolLoaded()
    {
        PanelViews = GameObject.Find("Panels").transform;
        foreach(Transform view in PanelViews)
        {
            RegisterPanel(view.GetChild(0).name, view.GetChild(0));
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(ButtonParent.GetComponent<RectTransform>());
    }

    void RegisterPanel(string name, Transform panel)
    {
        var button = Instantiate(togglePanelButton, ButtonParent);
        button.GetComponent<Interactable>().OnClick.AsObservable().Subscribe(_ => TogglePanel(name));
        button.GetComponent<SystemTrayButtonController>().SetPanelName(name);
                
        var panelData = new PanelData(panel, button);

        panels.Add(name, panelData);
    }

    void TogglePanel(string name)
    {
        var panel = panels[name];
        if(panel.state == PanelState.Maximized)
        {
            panel.state = PanelState.Minimized;
            panel.panelObject.gameObject.SetActive(false);
            panel.button.GetComponent<SystemTrayButtonController>().ToggleBackplate(true);
        }
        else if(panel.state == PanelState.Minimized)
        {
            panel.state = PanelState.Maximized;
            panel.panelObject.gameObject.SetActive(true);
            panel.button.GetComponent<SystemTrayButtonController>().ToggleBackplate(false);
        }
        panels[name] = panel;
    }

    enum PanelState
    {
        Maximized,
        Minimized,
    }
    struct PanelData
    {
        public Transform panelObject;
        public PanelState state;
        public GameObject button;

        public PanelData(Transform panel, GameObject button)
        {
            this.panelObject = panel;
            this.button = button;
            if(panel.gameObject.activeSelf)
            {
                this.state = PanelState.Maximized;
                this.button.GetComponent<SystemTrayButtonController>().ToggleBackplate(false);
            }
            else
            {
                this.state = PanelState.Minimized;
                this.button.GetComponent<SystemTrayButtonController>().ToggleBackplate(true);
            }
        }
    }
}
