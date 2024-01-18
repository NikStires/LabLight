using System;
using System.Collections.Generic;
using UniRx;
using TMPro;

/// <summary>
/// Small controller to be able to instantate UI prefabs in the procedure that can access the special purpose chess interfaces
/// </summary>
public class ChessController : WorldPositionController
{
    private IAudio audioPlayer;
    Action disposeVoice;

    public TMP_Text ChessMessage;

    void Start()
    {
        audioPlayer = ServiceRegistry.GetService<IAudio>();
    }

    private void OnEnable()
    {
        SetupVoiceCommands();

        // Play sfx by subscribing to changes
        ChessSessionState.chessWhiteInCheckStream.Subscribe(value =>
        {
            InCheck();
        }).AddTo(this);

        ChessSessionState.chessWhiteInCheckmateStream.Subscribe(value =>
        {
            WhiteCheckmate();
        }).AddTo(this);

        ChessSessionState.chessBlackInCheckStream.Subscribe(value =>
        {
            InCheck();
        }).AddTo(this);

        ChessSessionState.chessBlackInCheckmateStream.Subscribe(value =>
        {
            BlackCheckmate();
        }).AddTo(this);

        ChessSessionState.chessIllegalMoveStream.Subscribe(value =>
        {
            IllegalMoveMade();
        }).AddTo(this);

        ChessSessionState.chessLegalMoveStream.Subscribe(value =>
        {
            LegalMoveMade();
        }).AddTo(this);

        ChessSessionState.chessPieceCapturedStream.Subscribe(value =>
        {
            PieceCaptured();
        }).AddTo(this);

        //Update chess message by subscribing to changes
        ChessSessionState.chessMessageStream.Subscribe(value =>
        {
            UpdateMessageText();
        }).AddTo(this);
    }

    private void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    private void InCheck()
    {
        if(ChessSessionState.ChessWhiteInCheck || ChessSessionState.ChessBlackInCheck){
            audioPlayer.Play(AudioEventEnum.ChessCheck);
        }
    }

    private void WhiteCheckmate()
    {
        if(ChessSessionState.ChessWhiteInCheckmate)
        {
            audioPlayer.Play(AudioEventEnum.ChessCheckmateWhite);
        }
    }

    private void BlackCheckmate()
    {
        if(ChessSessionState.ChessBlackInCheckmate)
        {
            audioPlayer.Play(AudioEventEnum.ChessCheckmateBlack);
        }
    }

    private void IllegalMoveMade()
    {
        if(ChessSessionState.ChessIllegalMove)
        {
            audioPlayer.Play(AudioEventEnum.ChessIllegalMove);
            ChessSessionState.ChessIllegalMove = false;
        }
    }

    private void LegalMoveMade()
    {
        if(ChessSessionState.ChessLegalMove)
        {
            audioPlayer.Play(AudioEventEnum.ChessSendMove);
            ChessSessionState.ChessLegalMove = false;
        }
    }

    private void PieceCaptured()
    {
        if(ChessSessionState.ChessPieceCaptured)
        {
            audioPlayer.Play(AudioEventEnum.ChessPieceCaptured);
            ChessSessionState.ChessPieceCaptured = false;
        }
    }

    private void UpdateMessageText()
    {
        ChessMessage.text = ChessSessionState.ChessMessage;
    }

    private void UpdateVisualState()
    {

    }

    private void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            { "new game", () => RequestNewChessGame() },
            { "move", () => RequestChessCompareBoards() },
            { "play", () => RequestChessCompareBoards() },
            { "suggest a move", () => RequestSuggestedMove() },
            { "help", () => RequestSuggestedMove() },
        });
    }
    public void RequestNewChessGame()
    {
        ServiceRegistry.GetService<IChessControl>()?.RequestChessNewGame();
        audioPlayer.Play(AudioEventEnum.ChessNewGame);
        //clear messages
        ChessSessionState.ChessMessage = "clear";
    }

    public void RequestChessPlayMove()
    {
        ServiceRegistry.GetService<IChessControl>()?.RequestChessPlayMove("f2", "f3");
    }

    public void RequestLegalMoves()
    {
        ServiceRegistry.GetService<IChessControl>()?.RequestChessLegalMoves();
    }

    public void RequestSuggestedMove()
    {
        ServiceRegistry.GetService<IChessControl>()?.RequestChessSuggestedMove();
        audioPlayer.Play(AudioEventEnum.ChessSuggestMove);
    }

    public void RequestChessCompareBoards()
    {
        audioPlayer.Play(AudioEventEnum.ChessPlayMove);
        ServiceRegistry.GetService<IChessControl>()?.RequestChessCompareBoards();
    }
}
