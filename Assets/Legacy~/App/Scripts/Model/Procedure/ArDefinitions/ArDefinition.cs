using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ArDefinitionType
{
    Line = 0,
    Outline = 1,
    Overlay = 2,
    Model = 3,              // Spatially positioned model
    Container = 4,           // Container with one or more content items
    Mask = 5,
    Arrow = 6,
    BoundingBox = 7
}

public abstract class ArDefinition
{
    [HideInInspector]
    public ArDefinitionType arDefinitionType;

    // [HideReferenceObjectPicker]
    /// <summary>
    /// Condition for creating and instance of this visualization
    /// </summary>
    public Condition condition;

    public virtual string ListElementLabelName()
    {
        return "Definition";
    }

    /// <summary>
    /// A specific AR definition creates a single instance and expects a SPECIFIC tracked object class
    /// </summary>
    /// <returns></returns>
    public bool IsSpecific()
    {
        if (condition != null)
        {
            return condition.IsSpecific();
        }

        return true;
    }

    /// <summary>
    /// A generic AR definition makes a new instance for EVERY tracked object
    /// </summary>
    /// <returns></returns>
    public bool IsGeneric()
    {
        if (condition != null)
        {
            return !condition.IsSpecific();
        }

        return false;
    }

    /// <summary>
    /// A targeted AR definition expects at least one detection
    /// </summary>
    /// <returns></returns>
    public bool IsTargeted()
    {
        if (condition != null)
        {
            return condition.IsTargeted();
        }

        return false;
    }

    /// <summary>
    /// The list of target Ids
    /// </summary>
    /// <returns></returns>
    public string[] Targets()
    {
        if (condition != null)
        {
            return condition.Targets();
        }

        return null;
    }

    /// <summary>
    /// Determines if this definition is interested in given trackedObjectId
    /// </summary>
    /// <param name="trackedObjectId"></param>
    /// <returns></returns>
    public bool IsOfInterest(string trackedObjectId)
    {
        var targets = Targets();
        if (targets == null)
        {
            return false;
        }

        foreach (var target in targets)
        {
            if (target == trackedObjectId)
            {
                return true;
            }
        }
        return false;
    }
}
