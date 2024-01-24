using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ProtocolState
{
    //state data
    public static ReactiveProperty<ProcedureDefinition> procedureDef = new ReactiveProperty<ProcedureDefinition>();
    private static string procedureTitle;
    private static DateTime startTime; 
    public static List<StepState> Steps = new List<StepState>();
    private static int step;
    private static string csvPath;

    //data streams
    public static Subject<string> procedureStream = new Subject<string>();
    public static Subject<StepState> stepStream = new Subject<StepState>();
    public static Subject<int> checklistStream = new Subject<int>();


    //locking and alignment bools
    public static ReactiveProperty<bool> LockingTriggered = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> AlignmentTriggered = new ReactiveProperty<bool>();

    // Setters
    public static void SetProcedureDefinition(ProcedureDefinition procedureDefinition)
    {
        procedureDef.Value = procedureDefinition;
        Steps = new List<StepState>();

        //create a fresh state for the selected protocol
        if (procedureDefinition != null && procedureDefinition.steps.Count > 0)
        {
            for (int i = 0; i < procedureDefinition.steps.Count; i++)
            {
                Steps.Add(new StepState());
                if(procedureDefinition.steps[i].checklist != null)
                {
                    Steps[i].Checklist = new List<CheckItemState>();
                    foreach(var check in procedureDefinition.steps[i].checklist)
                    {
                        Steps[i].Checklist.Add(new CheckItemState() { Text = check.Text});
                    }
                }
            }
            Step = 0;
            stepStream.OnNext(Steps[0]);
            CheckItem = 0;
            ServiceRegistry.GetService<ILighthouseControl>()?.SetProtocolStatus();
        }
    }

    public static int Step
    {
        set
        {
            if (step != value)
            {
                step = value;
                stepStream.OnNext(Steps[Step]);

                //if the step has a checklist get the active item
                if (procedureDef.Value != null && Steps != null && Steps[Step].Checklist != null)
                {
                    var firstUncheckedItem = (from item in Steps[Step].Checklist
                                      where !item.IsChecked.Value
                                      select item).FirstOrDefault();

                    if (firstUncheckedItem == null)
                    {
                        //if there is a checklist but the there is no unchecked item set the current checkItem to the last item;
                        CheckItem = Steps[Step].Checklist.Count - 1;
                    }
                    else
                    {
                        CheckItem = Steps[Step].Checklist.IndexOf(firstUncheckedItem);
                    }
                }
                else if (Steps[Step].Checklist == null)
                {
                    CheckItem = 0;
                }
                ServiceRegistry.GetService<ILighthouseControl>()?.SetProtocolStatus();
            }
        }
        get
        {
            return step;
        }
    }

    public static int CheckItem
    {
        set
        {
            if (procedureDef.Value != null && Steps != null && Step < Steps.Count && Steps[Step].Checklist != null && value <= Steps[Step].Checklist.Count() - 1)
            {
                Steps[Step].CheckNum = value; 
                checklistStream.OnNext(value);
                ServiceRegistry.GetService<ILighthouseControl>()?.SetProtocolStatus();
            }
            // else
            // {
            //     Steps[Step].CheckNum = 0; 
            //     checklistStream.OnNext(0);
            // }
        }
        get
        {
            if(Steps != null && Step < Steps.Count && Steps[Step] != null)
                return Steps[Step].CheckNum;
            else
                return 0;
        }
    }


    public static string ProcedureTitle
    {
        set
        {
            if (procedureTitle != value)
            {
                procedureTitle = value;
                
                procedureStream.OnNext(value);
            }
        }
        get
        {
            return procedureTitle;
        }
    }

    public static DateTime StartTime
    {
        set
        {
            if (startTime != value)
            {
                startTime = value;
            }
        }
        get
        {
            return startTime;
        }
    }

    public static string CsvPath
    {
        set
        {
            if (csvPath != value)
            {
                csvPath = value;
            }
        }
        get
        {
            return csvPath;
        }
    }

    public static void SetStep(int step)
    {
        if (step < 0 ||
            procedureDef.Value == null ||
            Steps == null ||
            step >= Steps.Count)
        {
            return;
        }

        Step = step;

        ServiceRegistry.GetService<ISharedStateController>().SetStep(SessionState.deviceId, step);
    }

    public static void SetCheckItem(int index)
    {
        if (index < 0 ||
            procedureDef.Value == null ||
            Steps == null ||
            Steps[Step] == null ||
            Steps[Step].Checklist == null || 
            index >= Steps[Step].Checklist.Count())
        {
            return;
        }

        CheckItem = index;
        ServiceRegistry.GetService<ISharedStateController>().SetCheckItem(SessionState.deviceId, index);
    }

    public static void SignOff()
    {
        if (procedureDef.Value == null ||
            Steps == null ||
            Steps[Step] == null ||
            Steps[Step].Checklist == null ||
            Steps[Step].SignedOff)
        {
            return;
        }

        Steps[Step].SignedOff = true;
        ServiceRegistry.GetService<ILighthouseControl>()?.SetProtocolStatus();
    }

    public static void SetProcedureTitle(string procedureTitle)
    {
        //ServiceRegistry.GetService<ISharedStateController>().SetProcedure(SessionState.deviceId, procedureTitle);
        ProcedureTitle = procedureTitle;
    }

    public static void SetStartTime(DateTime time)
    {
        StartTime = time;
    }

    public static void SetCsvPath(string path)
    {
        CsvPath = path;
    }

    //Data Structures
    public class StepState
    {
        public bool SignedOff = false;
        public int CheckNum = 0;
        public List<CheckItemState> Checklist = null;
    }

    public class CheckItemState
    {   
        public DateTime CompletionTime;
        public ReactiveProperty<bool> IsChecked = new ReactiveProperty<bool>();
        public string Text;
    }
}
