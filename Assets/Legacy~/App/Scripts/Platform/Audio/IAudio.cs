using UnityEngine;

public enum AudioEventEnum
{
    EnterNewScreen,
    CalibrationCompleted,
    NextStep, 
    PreviousStep,
    NextSubStep,
    PreviousSubStep,
    StartRecording,
    StopRecording,
    Hidden,
    Shown,
    Check,
    Uncheck,
    SignOff,
    Error,
    TimerTick,
    TimerComplete,
    ChessNewGame,
    ChessPlayMove,
    ChessSendMove,
    ChessSuggestMove,
    ChessCheck,
    ChessCheckmateWhite,
    ChessCheckmateBlack,
    ChessIllegalMove,
    ChessPieceCaptured,
    StartReplay,
    StopReplay,
    Fetch,
    DownloadComplete,
    Close,
    lastItemCompleted
}

public interface IAudio
{    
    void Play(AudioEventEnum key);
}