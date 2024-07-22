using UnityEngine;

/// <summary>
/// Stores model data 
/// </summary>
public class ContentController<T> : MonoBehaviour where T : ContentItem
{
    public virtual T ContentItem
    {
        get;
        set;
    }
}