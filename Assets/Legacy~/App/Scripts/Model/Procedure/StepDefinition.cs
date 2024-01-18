using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single step in a procedure can contain multiple ArDefinitions that are active during that whole step
/// It can have substeps each with their own ArDefinitions
/// </summary>
[Serializable]
public class StepDefinition
{
    public bool isCritical = false;

    [HideReferenceObjectPicker]
    [HideIf("@this.contentItems == null")]
    [ListDrawerSettings(Expanded = true, ShowIndexLabels = true, ShowPaging = false, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "ListElementLabelName")]
    public List<ContentItem> contentItems = new List<ContentItem>();

    [ListItemSelector("SetSelectedCheckItem")]
    [HideReferenceObjectPicker]
    [HideIf("@this.checklist == null")]
    [ListDrawerSettings(Expanded = true, ShowIndexLabels = true, ShowPaging = false, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "ListElementLabelName")]
    public List<CheckItemDefinition> checklist;

    public void SetSelectedCheckItem(int index)
    {
        ProtocolState.CheckItem = index;
    }

    public string ListElementLabelName()
    {
        return "Step";
    }

    [Button("Add Empty Text Item To Step")]
    private void AddEmptyTextItem()
    {
        contentItems.Add(new TextItem());
    }
    
    [Button("Add Empty Image Item To Step")]
    private void AddEmptyImageItem()
    {
        contentItems.Add(new ImageItem());
    }

    [Button("Add Empty Checklist Item")]
    private void AddEmptyCheckitem()
    {
        ProcedureExplorer.AddEmptyCheckitem();
    }
}