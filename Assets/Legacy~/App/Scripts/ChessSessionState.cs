using UniRx;

/// <summary>
/// This one is similar to SessionState, but specific for chess related state
/// </summary>
public class ChessSessionState
{
    private static bool _chessIsWhiteTurn;
    public static Subject<bool> chessIsWhiteTurnStream = new Subject<bool>();

    private static bool _chessWhiteInCheck;
    public static Subject<bool> chessWhiteInCheckStream = new Subject<bool>();

    private static bool _chessWhiteInCheckmate;
    public static Subject<bool> chessWhiteInCheckmateStream = new Subject<bool>();

    private static bool _chessBlackInCheck;
    public static Subject<bool> chessBlackInCheckStream = new Subject<bool>();

    private static bool _chessBlackInCheckmate;
    public static Subject<bool> chessBlackInCheckmateStream = new Subject<bool>();

    private static bool _chessIllegalMove;
    public static Subject<bool> chessIllegalMoveStream = new Subject<bool>();

    private static bool _chessLegalMove;
    public static Subject<bool> chessLegalMoveStream = new Subject<bool>();

    private static bool _chessPieceCaptured;
    public static Subject<bool> chessPieceCapturedStream = new Subject<bool>();

    private static string _chessMessage;
    public static Subject<string> chessMessageStream = new Subject<string>();

    // Setters
    public static bool ChessIsWhiteTurn
    {
        set
        {
            if (_chessIsWhiteTurn != value) 
            {
                _chessIsWhiteTurn = value;
                chessIsWhiteTurnStream.OnNext(value);
            }
        }
        get
        {
            return _chessIsWhiteTurn;
        }
    }

    public static bool ChessWhiteInCheck
    {
        set
        {
            if (_chessWhiteInCheck != value)
            {
                _chessWhiteInCheck = value;
                chessWhiteInCheckStream.OnNext(value);
            }
        }
        get
        {
            return _chessWhiteInCheck;
        }
    }

    public static bool ChessWhiteInCheckmate
    {
        set
        {
            if (_chessWhiteInCheckmate != value)
            {
                _chessWhiteInCheckmate = value;
                chessWhiteInCheckmateStream.OnNext(value);
            }
        }
        get
        {
            return _chessWhiteInCheckmate;
        }
    }

    public static bool ChessBlackInCheck
    {
        set
        {
            if (_chessBlackInCheck != value)
            {
                _chessBlackInCheck = value;
                chessBlackInCheckStream.OnNext(value);
            }
        }
        get
        {
            return _chessBlackInCheck;
        }
    }

    public static bool ChessBlackInCheckmate
    {
        set
        {
            if (_chessBlackInCheckmate != value)
            {
                _chessBlackInCheckmate = value;
                chessBlackInCheckmateStream.OnNext(value);
            }
        }
        get
        {
            return _chessBlackInCheckmate;
        }
    }

    public static bool ChessIllegalMove
    {
        set
        {
            if (_chessIllegalMove != value)
            {
                _chessIllegalMove = value;
                chessIllegalMoveStream.OnNext(value);
            }
        }
        get
        {
            return _chessIllegalMove;
        }
    }

    
    public static bool ChessLegalMove
    {
        set
        {
            if (_chessLegalMove != value)
            {
                _chessLegalMove = value;
                chessLegalMoveStream.OnNext(value);
            }
        }
        get
        {
            return _chessLegalMove;
        }
    }

    public static bool ChessPieceCaptured
    {
        set
        {
            if (_chessPieceCaptured != value)
            {
                _chessPieceCaptured = value;
                chessPieceCapturedStream.OnNext(value);
            }
        }
        get
        {
            return _chessPieceCaptured;
        }
    }

    public static string ChessMessage
    {
        set
        {
            if(value == "clear")
            {
                _chessMessage = "";
                chessMessageStream.OnNext(value);
            }
            else if (_chessMessage != value)
            {
                _chessMessage = value + '\n' + '\n' + _chessMessage;
                chessMessageStream.OnNext(value);
            }
        }
        get
        {
            return _chessMessage;
        }
    }
}
