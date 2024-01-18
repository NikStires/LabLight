using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.IO;

/// <summary>
/// Screen controller for selecting a procedure from a list of available procedures
/// </summary>
public class MenuScreen : ScreenViewController
{
    public GridObjectCollection tileObjectCollection;
    public ScrollingObjectCollection objectCollection;
    public UiTile tilePrefab;
    public RadialView radialView;

    private IDisposable modeSub;
    private IDisposable loadIndexSub;
    private Action disposeVoice;
    private float delaySpacing = 0.1f;
    private List<UiTile> tiles = new List<UiTile>();
    private bool pinned = false;

    private List<ProcedureDescriptor> procedures;
    
    private IAudio audioPlayer;

    private void Awake()
    {
        audioPlayer = ServiceRegistry.GetService<IAudio>();
    }

    private void OnEnable()
    {
        SubscribeModeChange();

        LoadProcedures();
    }

    private void OnDisable()
    {
        loadIndexSub?.Dispose();
        disposeVoice?.Invoke();
        disposeVoice = null;

        modeSub?.Dispose();
        modeSub = null;
        loadIndexSub = null;

        foreach (var tile in tiles)
        {
            if(tile != null)
            {
                Destroy(tile.gameObject);
            }
        }
        tiles.Clear();
    }

    void Build(List<ProcedureDescriptor> procedures, Action<ProcedureDescriptor> onSelected, Action<ProcedureDescriptor> onHeld)
    {
        // Destroy design time data,
        // Note that tileObjectCollection.UpdateCollection may run before the design time objects are destroyed
        // Making objects inactive so no one notices
        for (int i = 0; i < tileObjectCollection.transform.childCount; i++)
        {
            tileObjectCollection.transform.GetChild(i).gameObject.SetActive(false);
            Destroy(tileObjectCollection.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < procedures.Count; i++)
        {
            var proc = procedures[i];
            UiTile go = Instantiate(tilePrefab, tileObjectCollection.transform);
            go.Title = proc.title;
            go.description.text = proc.description;
            go.Enter(i * delaySpacing);
            go.OnClick.AsObservable().Subscribe(_ =>
            {
                onSelected(proc);
            }).AddTo(this);
            go.OnHold.AsObservable().Subscribe(_ =>
            {
                onHeld(proc);
                Destroy(go.gameObject);
            }).AddTo(this);

            tiles.Add(go);
        }

        objectCollection.UpdateContent();
        tileObjectCollection.UpdateCollection();
    }

    IEnumerator UpdateObjectCollection()
    {
        yield return new WaitForSeconds(2f);
        tileObjectCollection.UpdateCollection();
        objectCollection.UpdateContent();
    }

    void selectProcedure(string procedureName)
    {
        ServiceRegistry.Logger.Log("Select procedure " + procedureName);

        if (String.IsNullOrEmpty(procedureName))
        {
            ProtocolState.SetProcedureDefinition(null);
            return;
        }

        ServiceRegistry.GetService<IProcedureDataProvider>().GetOrCreateProcedureDefinition(procedureName).First().Subscribe(procedure =>
        {
            ProtocolState.SetProcedureDefinition(procedure);
        }, (e) =>
        {
            Debug.Log("Error fetching procedure");
            // TODO retry?!
        });
        SessionManager.Instance.GotoScreen(ScreenType.Procedure);
        ProtocolState.SetProcedureTitle(procedureName);
    }

    void deleteProcedure(string procedureName)
    {
        ServiceRegistry.Logger.Log("Delete procedure " + procedureName);

        // Save the .csv
        var lfdp = new LocalFileDataProvider();

        lfdp.DeleteProcedureDefinition(procedureName);
    }

    void checkModeChange(Mode mode)
    {
        if (mode == Mode.Observer)
        {
            ServiceRegistry.Logger.Log("Device is Slaved. Transitioning out of Procedure Selection Screen");
            SessionManager.Instance.GotoScreen(ScreenType.Procedure);
        }
    }

    void SubscribeModeChange()
    {
        if (SessionState.RunningMode == Mode.Observer)
        {
            checkModeChange(Mode.Observer);
        }
        else
        {
            modeSub = SessionState.modeStream.Subscribe(checkModeChange);
        }
    }

    void SetupVoiceCommands()
    {
        if (procedures == null)
            return;

        disposeVoice?.Invoke();
        disposeVoice = null;

        // Setup voice keywords for each procedure
        var commands = new Dictionary<string, Action>();
        foreach (var proc in procedures)
        {
            commands.Add(proc.name, () => selectProcedure(proc.name));
        }

        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(commands);
    }

    async void LoadProcedures()
    {
        // Load procedure list
        procedures = await ServiceRegistry.GetService<IProcedureDataProvider>()?.GetProcedureList();

        SetupVoiceCommands();

       // Build menu
        Build(procedures, pi => selectProcedure(pi.name), pi => deleteProcedure(pi.name));
    }  

    public async void DownloadWellPlateCsv()
    {
        await ServiceRegistry.GetService<IWellPlateCsvProvider>().DownloadWellPlateCsvAsync();    
        LoadProcedures();
    }
    public void TogglePin()
    {
        if(pinned)
        {
            radialView.enabled = true;
            pinned = false;
        }
        else
        {
            radialView.enabled = false;
            pinned = true;
        }
    }
}