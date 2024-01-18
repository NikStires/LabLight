using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>\
/// Complete procedure with sequence of steps
/// Each step can contain multiple AR definitions
/// </summary>
[Serializable]
public class ProcedureDefinition
{
    [HideInPlayMode]
    [HideInEditorMode]
    /// Version to help identify differences in file formats
    public int version;

    [Multiline(2)]
    public string title;

    [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Resources")]
    public string pdfPath;

    /// <summary>
    /// Ar definitions that are active during the whole procedure (not tied to specifc steps)
    /// These definitions typically have more generic targetting (for instance render masks for all detections)
    /// </summary>
    [HideReferenceObjectPicker]
    [ListDrawerSettings(Expanded = true, ShowIndexLabels = true, ShowPaging = false, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "ListElementLabelName")]
    public List<ArDefinition> globalArElements = new List<ArDefinition>();

    [ListItemSelector("SetSelectedStep")]
    [HideReferenceObjectPicker]
    [ListDrawerSettings(Expanded = true, ShowIndexLabels = true, ShowPaging = false, ShowItemCount = true, HideRemoveButton = false, ListElementLabelName = "ListElementLabelName")]
    public List<StepDefinition> steps = new List<StepDefinition>();

    // The root path for image/video/model media (not networked)
    [HideInPlayMode]
    [HideInEditorMode]
    public string mediaBasePath;

    public void SetSelectedStep(int index)
    {
        ProtocolState.Step = index;
    }
}