using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single step in a protocol can contain multiple ArDefinitions that are active during that whole step
/// It can have substeps each with their own ArDefinitions
/// </summary>
[Serializable]
public class StepDefinition
{
    public bool isCritical = false;

    public List<ContentItem> contentItems = new List<ContentItem>();

    public List<CheckItemDefinition> checklist;

    //public void SetSelectedCheckItem(int index)
    //{
    //    ProtocolState.CheckItem = index;
    //}

    public string ListElementLabelName()
    {
        return "Step";
    }

    private void AddEmptyTextItem()
    {
        contentItems.Add(new TextItem());
    }
    
    private void AddEmptyImageItem()
    {
        contentItems.Add(new ImageItem());
    }

    //private void AddEmptyCheckitem()
    //{
    //    ProcedureExplorer.AddEmptyCheckitem();
    //}
}