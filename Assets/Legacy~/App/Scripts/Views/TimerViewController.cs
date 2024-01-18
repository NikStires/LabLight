using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;

/// <summary>
/// Draggable TimerViewController
/// </summary>

public class TimerViewController : MonoBehaviour
{
    public GameObject View;
    public double StartingTime = 10.0f;
    public TMP_Text TimeDisplay;
    public GameObject StartButton;
    public GameObject StopButton;
    public GameObject SetButton;
    public GameObject ResetButton;
    public GameObject TimeSetControls;

    private double TimeLeft;
    private bool timerRunning = false;
    private IAudio audioPlayer;
    private Action disposeVoice;

    // Start is called before the first frame update
    void Start()
    {
        audioPlayer = ServiceRegistry.GetService<IAudio>();

        TimeLeft = StartingTime;

        TimeDisplay.text = GetTimeString();
    }

    // Update is called once per frame
    void Update()
    {
        //if timer has not finished and is running decrement time
        if(TimeLeft > 0 & timerRunning)
        {
            TimeLeft -= Time.deltaTime;
            TimeDisplay.text = GetTimeString();
        }
        //if timer is about to finish play sound effect (takes a second to start playing)
        if(TimeLeft < 1 & timerRunning)
        {
            audioPlayer.Play(AudioEventEnum.TimerComplete);
        }
        if(TimeLeft < 0 & timerRunning)
        {
            StopTimer();
        }
    }

    void Awake()
    {
        ProtocolState.checklistStream.Subscribe(_ => OnCheckItemChange()).AddTo(this);
    }

    private void OnEnable()
    {
        SetupVoiceCommands();
    }

    private void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    private void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            {"start timer", () => StartTimer() },
            {"resume timer", () => StartTimer() },
            {"stop timer", () => StopTimer() },
            {"pause timer", () =>  StopTimer() },
            {"reset timer", () => ResetTimer() },
            {"restart timer",() => RestartTimer() },
            {"hide timer", () => HideTimer() },
            {"show timer", () => ShowTimer() }
        });
    }

    //Event functions

    public void IncrementHour()
    {
        if(StartingTime < 352800f)
        {
            StartingTime += 3600f;
            TimeLeft = StartingTime;
            TimeDisplay.text = GetTimeString();
        }
    }

    public void DecrementHour()
    {
        if(StartingTime > 3600f)
        {
            StartingTime -= 3600f;
            TimeLeft = StartingTime;
            TimeDisplay.text = GetTimeString();
        }
    }

    public void IncrementMinute()
    {
        if (StartingTime < 356340f)
        {
            StartingTime += 60f;
            TimeLeft = StartingTime;
            TimeDisplay.text = GetTimeString();
        }
    }

    public void DecrementMinute()
    {
        if(StartingTime > 60f)
        {
            StartingTime -= 60f;
            TimeLeft = StartingTime;
            TimeDisplay.text = GetTimeString();
        }
    }

    public void IncrementSecond()
    {
        if(StartingTime < 356399f)
        {
            StartingTime += 1f;
            TimeLeft = StartingTime;
            TimeDisplay.text = GetTimeString();
        }
    }

    public void DecrementSecond()
    {
        if(StartingTime > 1f)
        {
            StartingTime -= 1f;
            TimeLeft = StartingTime;
            TimeDisplay.text = GetTimeString();
        }
    }

    public void SetTimer()
    {
        StopTimer();
        StartingTime = TimeLeft;
        TimeSetControls.SetActive(!TimeSetControls.activeSelf);
        StartButton.SetActive(!TimeSetControls.activeSelf);
        StopButton.SetActive(!TimeSetControls.activeSelf);
        ResetButton.SetActive(!TimeSetControls.activeSelf);
        if (!TimeSetControls.activeSelf)
        {
            StopButton.SetActive(false);
        }
    }

    public void ResetTimer()
    {
        TimeLeft = StartingTime;
        StopTimer();
        TimeDisplay.text = GetTimeString();
    }

    public void StartTimer()
    {
        ServiceRegistry.GetService<ILighthouseControl>()?.StartTimer((int)StartingTime);
        timerRunning = true;
        StartButton.SetActive(false);
        StopButton.SetActive(true);
    }

    public void StopTimer()
    {
        timerRunning = false;
        StartButton.SetActive(true);
        StopButton.SetActive(false);
    }

    public void HideTimer()
    {
        View.SetActive(false);
    }

    public void ShowTimer()
    {
        View.SetActive(true);
    }

    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    //util
    private string GetTimeString()
    {
        double roundTime = System.Math.Round(TimeLeft, 1);
        TimeSpan ts = TimeSpan.FromSeconds(roundTime);
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
                    ts.Hours,
                    ts.Minutes,
                    ts.Seconds);
    }

    private void OnCheckItemChange()
    {
        if (ProtocolState.procedureDef.Value != null && ProtocolState.Steps != null && ProtocolState.Steps[ProtocolState.Step] != null)
        {
            if(ProtocolState.Steps[ProtocolState.Step].Checklist != null)
            {
                var currentCheckItem = ProtocolState.procedureDef.Value.steps[ProtocolState.Step].checklist[ProtocolState.CheckItem];
                if(currentCheckItem.activateTimer)
                {
                    int timeSeconds = (currentCheckItem.hours * 60 * 60) + (currentCheckItem.minutes * 60) + currentCheckItem.seconds;
                    StartingTime = timeSeconds;
                    ResetTimer();
                    ShowTimer();
                }
            }
        }
    }
}
