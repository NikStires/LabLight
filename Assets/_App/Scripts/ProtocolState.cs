using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using Unity.VisualScripting;

public class ProtocolState : MonoBehaviour
{
    public static ProtocolState Instance;

    //state data
    public static ProcedureDefinition procedureDef;
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

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple ProtocolState instances detected. Destroying duplicate (newest).");
            DestroyImmediate(gameObject);
        }

        if(SessionState.Instance.activeProtocol != null)
        {
            SetProcedureDefinition(SessionState.Instance.activeProtocol);
        }
        else
        {
            Debug.Log("No active protocol, returning to protocol selection");
            SceneLoader.Instance.LoadNewScene("ProtocolMenu");
        }
    }

    // Setters
    public static void SetProcedureDefinition(ProcedureDefinition procedureDefinition)
    {
        Steps = new List<StepState>();
        //create a fresh state for the selected protocol
        if (procedureDefinition != null && procedureDefinition.steps.Count > 0)
        {
            List<ModelArDefinition> arModels = procedureDefinition.globalArElements.Where(x => x.arDefinitionType == ArDefinitionType.Model).Cast<ModelArDefinition>().ToList();
            if(arModels.Count > 0)
            {   
                procedureDefinition.steps.Insert(0, Instance.createLockingStep(arModels));
            }
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
            procedureDef = procedureDefinition;
            step = 0;
            Debug.Log("Set procedure definition to " + procedureDefinition.title);
            ProcedureTitle = procedureDefinition.title;
            ServiceRegistry.GetService<ILighthouseControl>()?.SetProtocolStatus();
            InitCSV();
        }
    }

    private StepDefinition createLockingStep(List<ModelArDefinition> arModels)
    {
        StepDefinition step = new StepDefinition();
        step.checklist = new List<CheckItemDefinition>();
        step.checklist.Add(new CheckItemDefinition() {Text = "Place the items listed below on your workspace"});
        foreach(ModelArDefinition arModel in arModels)
        {
            step.checklist.Add(new CheckItemDefinition() { Text = arModel.name, operations = new List<ArOperation>() { new AnchorArOperation() { arDefinition = arModel } } });
        }
        return step;
    }

    public static int Step
    {
        set
        {
            step = value;
            stepStream.OnNext(Steps[Step]);

            //if the step has a checklist get the active item
            if (procedureDef != null && Steps != null && Steps[Step].Checklist != null)
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
                checklistStream.OnNext(0);
            }
            ServiceRegistry.GetService<ILighthouseControl>()?.SetProtocolStatus();
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
            if (procedureDef != null && Steps != null && Step < Steps.Count && Steps[Step].Checklist != null && value <= Steps[Step].Checklist.Count() - 1)
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
            procedureDef == null ||
            Steps == null ||
            step >= Steps.Count)
        {
            return;
        }

        Step = step;
    }

    public static void SetCheckItem(int index)
    {
        if (index < 0 ||
            procedureDef == null ||
            Steps == null ||
            Steps[Step] == null ||
            Steps[Step].Checklist == null || 
            index >= Steps[Step].Checklist.Count())
        {
            return;
        }

        CheckItem = index;
    }

    public static void SignOff()
    {
        if (procedureDef == null ||
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

    private static void InitCSV()
    {
        string fileName = procedureDef.title + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
        string csvPath = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(csvPath))
        {
            File.WriteAllText(csvPath, "Action,Result,Completion Time\n");
        }
        SetCsvPath(csvPath);
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
