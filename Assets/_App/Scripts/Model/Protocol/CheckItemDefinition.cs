using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

/// <summary>
/// A checkItem in a procedure step
/// Basically the same as a stepdefinition but with the extra substeps to prevent endless nesting
/// 
/// A single checklist can have many check items
/// 
/// A single check item can have:
/// -add one or more ArViews as specified by the ArDefinitions, for instance show arrow, draw line etc.
/// -add one or more operations that are performed on the ArDefinitions living in the parent step, for instance highlight well A1
/// 
/// To walk through a series of highlights separate check items need to be defined manually
/// </summary>
[Serializable]
public class CheckItemDefinition
{
    public string Text;

    public bool activateTimer = false;

    public int hours;
    public int minutes;
    public int seconds;

    public List<ContentItem> contentItems = new List<ContentItem>();

    // Operations that are applied to ArDefinitions specified in parent step definition
    public List<ArOperation> operations = new List<ArOperation>();

    //public void SetSelectedOperation(int index)
    //{
    //    ProcedureExplorer.UpdateSelectedOperation((index < 0) ? null : operations[index]);
    //}

    private void AddEmptyTextItem()
    {
        contentItems.Add(new TextItem());
    }
    
    private void AddEmptyImageItem()
    {
        contentItems.Add(new ImageItem());
    }
    
    public virtual string ListElementLabelName()
    {
        return "Check Item";
    }
}