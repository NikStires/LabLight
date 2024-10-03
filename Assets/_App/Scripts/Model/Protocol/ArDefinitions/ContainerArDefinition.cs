using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerArDefinition : ArDefinition //, IList<ContentItem> RS need to hide to IList to make Odin show the whole ArDefinition
{
    public LayoutItem layout = new LayoutItem();

    public ContainerArDefinition()
    {
        arDefinitionType = ArDefinitionType.Container;
    }

    public ContentItem this[int index] 
    { 
        get => layout.contentItems[index];
        set => layout.contentItems[index] = value; 
    }

    public int Count 
    { 
        get => layout.contentItems.Count;  
    }

    public bool IsReadOnly
    {
        get => false;
    }

    public void Add(ContentItem item)
    {
        layout.contentItems.Add(item);
    }

    public void Clear()
    {
        layout.contentItems.Clear();
    }

    public bool Contains(ContentItem item)
    {
        return layout.contentItems.Contains(item);
    }

    public void CopyTo(ContentItem[] array, int arrayIndex)
    {
        layout.contentItems.CopyTo(array, arrayIndex);
    }

    public IEnumerator<ContentItem> GetEnumerator()
    {
        return layout.contentItems.GetEnumerator();
    }

    public int IndexOf(ContentItem item)
    {
        return layout.contentItems.IndexOf(item);
    }

    public void Insert(int index, ContentItem item)
    {
        layout.contentItems.Insert(index, item);
    }

    public bool Remove(ContentItem item)
    {
        return layout.contentItems.Remove(item);
    }

    public void RemoveAt(int index)
    {
        layout.contentItems.RemoveAt(index);
    }

    //IEnumerator IEnumerable.GetEnumerator()
    //{
    //    return layout.contentItems.GetEnumerator();
    //}

    public override string ListElementLabelName()
    {
        return "Container AR Definition";
    }

    //[Button("Create Anchor Operation in New Checklist Item")]
    //private void CreateAnchorOperationNewCheckItem()
    //{
    //    var al = new AnchorArOperation();
    //    al.arDefinition = this;
    //    ProcedureExplorer.AddOperationInCheckItem(al);
    //}

    //[Button("Create Anchor Operation in Current Checklist Item")]
    //private void CreateAnchorOperationCurrentCheckItem()
    //{
    //    var al = new AnchorArOperation();
    //    al.arDefinition = this;
    //    ProcedureExplorer.AddOperationToCheckItem(al);
    //}
}
