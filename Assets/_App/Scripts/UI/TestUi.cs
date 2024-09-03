using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUi : MonoBehaviour
{
    private void Awake()
    {

        ProtocolState.SetProcedureDefinition(CreateTestProtocol());
        ProtocolState.ProcedureTitle = ProtocolState.procedureDef.title;
        Debug.Log("Test protocol set");
    }

    private List<CheckItemDefinition> CreateCheckItems(int num)
    {
        //create check items
        List<CheckItemDefinition> checkItems = new List<CheckItemDefinition>();
        for (int i = 0; i < num; i++)
        {
            CheckItemDefinition newCheckItem = new CheckItemDefinition();
            newCheckItem.Text = "checkItem " + i;
            checkItems.Add(newCheckItem);
        }
        return checkItems;
    }

    private List<StepDefinition> CreateSteps(int num)
    {
        List<StepDefinition> steps = new List<StepDefinition>();
        for(int i = 0; i < num; i++)
        {
            StepDefinition newStep = new StepDefinition();
            TextItem newTextItem = new TextItem();
            newTextItem.text = "text item on step " + i;
            newStep.contentItems.Add(newTextItem);
            newStep.checklist = CreateCheckItems(10);
            steps.Add(newStep);
        }
        return steps;
    }

    private ProcedureDefinition CreateTestProtocol()
    {
        ProcedureDefinition testProcedure = new ProcedureDefinition();
        testProcedure.title = "Test Protocol";
        testProcedure.steps = CreateSteps(10);
        return testProcedure;
    }
}
