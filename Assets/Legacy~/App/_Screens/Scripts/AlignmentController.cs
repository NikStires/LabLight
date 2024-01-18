using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentController : MonoBehaviour
{
    public static void TriggerAlignment(ArElementViewController model)
    {
        if(model != null && model is ModelElementViewController)
        {
            ((ModelElementViewController)model).AlignmentGroup();
        }
    }

    public static void ResetModel(ArElementViewController model)
    {
        if(model != null && model is ModelElementViewController)
        {
            ((ModelElementViewController)model).ResetToCurrentHighlights();
        }
    }

    public static void TriggerAlignment(List<ArElementViewController> models)
    {
        foreach(var model in models)
        {
            if(model is ModelElementViewController)
            {
                ((ModelElementViewController)model).AlignmentGroup();
            }
        }
    }

    public static void ResetModel(List<ArElementViewController> models)
    {
        foreach(var model in models)
        {
            if(model is ModelElementViewController)
            {
                ((ModelElementViewController)model).ResetToCurrentHighlights();
            }
        }
    }
}
