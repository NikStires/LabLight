using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// Generates a grid of prefabs and corresponding HighlightGroups on the selected Model AR (ModelElementViewController)
/// </summary>
public class GridGeneratorWindow : OdinEditorWindow
{
    [InfoBox("Select a prefab with a ModelElementViewController to operate on (e.g. WellPlate)\n" +
             "Next select the prefab to instantiate and the parent container.")]
    
    
    [PropertyTooltip("Prefab to instantiate in grid (e.g. well)")]
    public GameObject Prefab;
    [PropertyTooltip("The transform that will be the parent of the instances")]
    public Transform Container;
    [PropertyTooltip("By column first?")]
    public bool ByColumn = true;
    [PropertyTooltip("Enables grouping higlights by column or by row")]
    public bool Group = false;
    [PropertyTooltip("The number of columns to generate")]
    public int NumberColumns = 12;
    [PropertyTooltip("The number of rows to generate")]
    public int NumberRows = 8;
    [PropertyTooltip("Height offset for each instance in meters")]
    public float CellHeightMeters = 0.008f;
    [PropertyTooltip("Offset between each instance in meters")]
    public Vector2 CellOffsetMeters = new Vector2(-0.009f, 0.009f);
    [PropertyTooltip("Offset Odd Rows")]
    public bool OffsetOddRows = false;
    [PropertyTooltip("WellPlate naming")]
    public bool WellPlateNaming = true;
    [PropertyTooltip("Intance Name Prefix")]
    public string InstanceNamePrefix = "Well";

    [MenuItem("LabLightAR/ModelGridGenerator")]
    private static void OpenWindow()
    {
        GetWindow<GridGeneratorWindow>().Show();
    }

    [DisableIf("@this.Prefab == null")]
    [DisableIf("@this.Container == null")]
    [Button("Rebuild Highlight Groups")]
    public void RebuildHighlightGroups()
    {
        Rebuild();
    }

    private void Rebuild()
    {
        var selectedObject = Selection.activeObject as GameObject;
        var model = selectedObject.GetComponent<ModelElementViewController>();
        
        if (model == null)
        {
            return;
        }

        for (int i = 0; i < Container.childCount; i++)
        {
            GameObject.DestroyImmediate(Container.GetChild(i).gameObject);
        }

        model.HighlightGroups.Clear();


        if (ByColumn)
        {
            for (int i = 0; i < NumberColumns; i++)
            {
                var group = new List<GameObject>();
                for (int j = 0; j < NumberRows; j++)
                {
                    var go = GenerateCell(j, i);
                    group.Add(go);

                    // Create a highlight for each cell
                    if (!Group)
                    {
                        GenerateHighlightGroup(model, go.name, new List<GameObject>() { go });
                    }
                }

                // Create highlight groups by column
                if (Group)
                {
                    GenerateHighlightGroup(model, string.Format("Column{0}", i), group);
                }
            }
        }
        else
        {
            for (int i = 0; i < NumberRows; i++)
            {
                var group = new List<GameObject>();
                for (int j = 0; j < NumberColumns; j++)
                {
                    var go = GenerateCell(i, j);
                    group.Add(go);

                    // Create a highlight for each cell
                    if (!Group)
                    {
                        GenerateHighlightGroup(model, go.name, new List<GameObject>() { go });
                    }
                }

                // Create highlight groups by row
                if (Group)
                {
                    GenerateHighlightGroup(model, string.Format("Row{0}", i), group);
                }
            }
        }
    }

    private GameObject GenerateCell(int i, int j)
    {
        var go = Instantiate(Prefab, Container);

        if (OffsetOddRows && i % 2 == 1)
        {
            go.transform.localPosition = new Vector3((j + 0.5f) * CellOffsetMeters.x, CellHeightMeters, i * CellOffsetMeters.y);
        }
        else
        {
            go.transform.localPosition = new Vector3(j * CellOffsetMeters.x, CellHeightMeters, i * CellOffsetMeters.y);
        }        
        go.name = WellPlateNaming ? string.Format("Well{0}{1}", Number2String(i, true), j+1) : string.Format("{0}{1}X{2}", InstanceNamePrefix, i, j);
        return go;
    }

    private string Number2String(int number, bool isCaps)
    {
        char c = (char)((isCaps ? 65 : 97) + number);

        return c.ToString();

    }

    private void GenerateHighlightGroup(ModelElementViewController model, string name, List<GameObject> objects)
    {
        model.HighlightGroups.Add(new HighlightGroup()
        {
            Name = name,
            SubParts = objects
        });
    }
}