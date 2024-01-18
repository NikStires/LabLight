using ACAM2.MessagePack;
using Battlehub.Dispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class Networking (previously AcamNetworkDiscovery)
/// 
// IChessControl implementation
public partial class Networking : MonoBehaviour, IChessControl
{
    //AM
    //internally keep track of whites turn
    private bool whiteTurn;


    private int pieceRemoved;
    private ChessPieceEnum pieceRemovedChessPiece;

    private const int ChessboardId = 99999;
    private const int ChessPieceStartId = 9000;
    private const int LegalMoveStartId = 18000;
    private const int SuggestedMoveStartId = 20000;
    private const int IllegalMoveStartId = 22000;
    private const int CheckMoveStartId = 24000;
    private const int CheckmateMoveStartId = 26000;


    // 11/15 NS
    //only allow updates when requested
    private bool AllowChessStateUpdate;

    private ChessBoard _lastChessBoard;
    private ChessState _lastChessState;
    private LegalMoves _lastLegalMoves;

    /// <summary>
    /// Converts a chessboard into a regular trackedobject
    /// </summary>
    /// <param name="chessBoard"></param>
    private void UpdatedChessboard(ChessBoard chessBoard)
    {
        _lastChessBoard = chessBoard;

        //get center of incoming chessboard
        Vector3 center = Vector3.zero;
        foreach (var p in chessBoard.Corners)
        {
            center += p;
        }
        center *= .25f;

        center.y = 0.004f;
        Quaternion centerRotation = Quaternion.FromToRotation(Vector3.forward, chessBoard.Corners[2] - chessBoard.Corners[1]);

        //create new tracked chessboard object or update exisiting
        TrackedObject trackedObject;
        if (!TrackedObjectDictionary.TryGetValue(ChessboardId, out trackedObject))
        {
            trackedObject = new TrackedObject()
            {
                id = ChessboardId,
                classId = -1,
                label = "Chessboard",
                position = center,
                //rotation = centerRotation,
                mask = chessBoard.Corners,
                lastUpdate = DateTime.Now + TimeSpan.FromHours(1),     // Let it live for an hour for better reuse
                color = Color.yellow,
                bounds = chessBoard.Corners
            };

            lock (TrackedObjectDictionary)
            {
                TrackedObjectDictionary[ChessboardId] = trackedObject;
            }

            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Add(trackedObject);
            });
        }
        else
        {
            // Reuse
            trackedObject.position = center;
            //trackedObject.rotation = centerRotation;
            trackedObject.mask = chessBoard.Corners;
            trackedObject.lastUpdate = DateTime.Now + TimeSpan.FromHours(1);     // Let it live for an hour
            trackedObject.bounds = chessBoard.Corners;
        }
    }

    private string ChessPieceName(Piece piece)
    {
        return piece.chessPiece.ToString();
    }

    private ChessPieceEnum[] blackPieces = { ChessPieceEnum.b_rook, ChessPieceEnum.b_knight, ChessPieceEnum.b_bishop, ChessPieceEnum.b_queen, ChessPieceEnum.b_king, ChessPieceEnum.b_pawn };
    private ChessPieceEnum[] whitePieces = { ChessPieceEnum.w_rook, ChessPieceEnum.w_knight, ChessPieceEnum.w_bishop, ChessPieceEnum.w_queen, ChessPieceEnum.w_king, ChessPieceEnum.w_pawn };

    

    private void UpdatedChessState(ChessState IncomingChessState)
    {
        if (!AllowChessStateUpdate)
        {
            return;
        }

        if (!IncomingChessState.IsLegal)
        {
            Debug.Log("Chess state illegal");

            //get position of king, if white turn, white king ...
            int kingPos = whiteTurn == true ? GetPiecePos(ChessPieceEnum.w_king) : GetPiecePos(ChessPieceEnum.b_king);
            //TO DO get position of piece checking king

            //if checkmate, update checkmate indicator, end game (How work?)
            
            if (IncomingChessState.IllegalReason.Contains("checkmate"))
            {
                Debug.Log("Checkmate");
                UpdatedCheckmateMoves(kingPos);

                //Display Message
                Dispatcher.Current.BeginInvoke(()   => 
                {
                    if(whiteTurn)
                    {
                        ChessSessionState.ChessMessage = "Your king is in checkmate, you lose this time :(";
                    }
                    else
                    {
                        ChessSessionState.ChessMessage = "Opposing king is in checkmate, victory has been siezed!";
                    }
                });

                //end game
            }
            //if check, update check indicator
            else if (IncomingChessState.IllegalReason.Contains("check"))
            {
                Debug.Log("In Check");
                UpdatedCheckMoves(kingPos);
                
                //Display Message
                Dispatcher.Current.BeginInvoke(()   => 
                {
                    if(whiteTurn)
                    {
                        ChessSessionState.ChessMessage = "Your king is in check.";
                    }
                    else
                    {
                        ChessSessionState.ChessMessage = "Opposing king is in check.";
                    }
                });
            }
        }

        //Should we disable anything after checkmate?
        
        // Gather changed fields with new pieces
        Dictionary<ChessPieceEnum, int> changedFields = new Dictionary<ChessPieceEnum, int>();

        if (_lastChessState != null)
        {
            //look at each position
            for (int i = 0; i < 64; i++)
            {
                //if the new state has a change log the piece type (IncomingChessState.Pieces[i] -> pieceEnum) and fieldID (i)
                if (IncomingChessState.Pieces[i] != _lastChessState.Pieces[i] && IncomingChessState.Pieces[i] != ChessPieceEnum.no_piece)
                {
                    if(whiteTurn && !blackPieces.Contains(IncomingChessState.Pieces[i]))
                        changedFields[IncomingChessState.Pieces[i]] = i;
                }
            }
        }
        /*
        if (changedFields.Count > 0)
        {
            foreach (var cf in changedFields)
            {
                Debug.Log("changed field:" + cf.Value + " to " + cf.Key);
                if (whiteTurn && blackPieces.Contains(cf.Key))
                {
                    changedFields.Remove(cf.Key);
                }
            }
        }*/
        //if only 1 field was changed in the incomingChessBoard
        if (changedFields.Count == 1)
        {
            Piece tempPiece;
            var cfVal = changedFields.FirstOrDefault();
            Debug.Log(cfVal.Key);
            Debug.Log(cfVal.Value);

            Debug.Log("Piece captured at:" + pieceRemoved + " of type:" + pieceRemovedChessPiece);
            //If piece exists at changed space
            if (Pieces.TryGetValue(cfVal.Value, out tempPiece))
            {
                Debug.Log("Piece value: " + tempPiece.chessPiece);
                //if the piece is white and the incoming chess state reports the space as empty
                if(whitePieces.Contains(tempPiece.chessPiece) && IncomingChessState.Pieces[cfVal.Value] == 0 && pieceRemoved != -1)
                {
                    Debug.Log("last chessboard had piece:" + tempPiece.chessPiece + " removed at " + pieceRemoved);
                    Debug.Log("Changed Fields now equals:");
                    if(pieceRemovedChessPiece == tempPiece.chessPiece)
                        changedFields[tempPiece.chessPiece] = pieceRemoved;
                    foreach(var ff in changedFields)
                    {
                        Debug.Log(ff.Key);
                        Debug.Log(ff.Value);
                    }
                }
                else if (whitePieces.Contains(tempPiece.chessPiece) && IncomingChessState.Pieces[cfVal.Value] == 0)
                {
                    Debug.Log("Lost detection on:" + tempPiece.chessPiece + " at " + cfVal.Value);
                    changedFields.Clear();
                    IncomingChessState = _lastChessState;
                }else if(whitePieces.Contains(tempPiece.chessPiece))
                {
                    Debug.Log("Regained detection on:" + tempPiece.chessPiece + " at " + cfVal.Value);
                    IncomingChessState = _lastChessState;
                    IncomingChessState.Pieces[cfVal.Value] = tempPiece.chessPiece;
                }
            }
        }

        bool moveMade = false;
        //check if there has been a move and we are not in the starting position
        if (changedFields.Count > 0 && _lastChessState != null)
        {
            int to = -1;
            int from = -1;
            bool validMove = false;
            //get move, TODO account for castling

            /* Legacy implementation
            foreach (var cf in changedFields)
            {
                Debug.Log("Field " + cf.Value + " changed to " + cf.Key);
                Piece tempPiece;
                //if there is a piece on the square marked empty
                if (cf.Key == 0 && Pieces.TryGetValue(cf.Value, out tempPiece))
                {
                    //if that piece is white, we set the from as that space
                    if (whitePieces.Contains(tempPiece.chessPiece))
                    {
                        from = cf.Value;
                        movedPiece = tempPiece.chessPiece;
                    }

                }
                //if the incoming square marks that space as containing a piece
                else if (whitePieces.Contains(cf.Key))
                    to = cf.Value;
            }
            //if the moved piece matches the from piece
            var cfValid = changedFields.Where(cf => cf.Key == movedPiece).FirstOrDefault();
            if (movedPiece == cfValid.Key)
            {
                Debug.Log("Move validated Piece:" + movedPiece + " from: " + from + " to:" + cfValid.Value);
                validMove = true;
                to = cfValid.Value;
            }
            */



            foreach (var cf in changedFields.Where(cf => cf.Key == 0))
            {
                Piece tempPiece;
                if(Pieces.TryGetValue(cf.Value, out tempPiece))
                {
                    if(whitePieces.Contains(tempPiece.chessPiece))
                    {
                        var cfValid = changedFields.Where(cfa => cfa.Key == tempPiece.chessPiece).FirstOrDefault();

                        if(cfValid.Key == tempPiece.chessPiece && cf.Value != pieceRemoved)
                        {
                            validMove = true;
                            from = cf.Value;
                            to = cfValid.Value;
                        }
                    }
                }
            }

            
            Debug.Log("To: " + to);
            Debug.Log("From: " + from);
            if (!validMove)
            {
                IncomingChessState = _lastChessState;
            }

            moveMade = false;
            //if there is a move
            if (from > -1 && to > -1 && legalChessMoveDictionary != null && validMove)
            {
                for (int i = 0; i < _lastLegalMoves.Moves.Count(); i++)
                {
                    //if move is legal
                    if (_lastLegalMoves.Moves[i].fromFieldId == from && _lastLegalMoves.Moves[i].toFieldId == to)
                    {
                        moveMade = true;
                        //play move

                        //play sound for legal move
                        Dispatcher.Current.BeginInvoke(()   =>
                        {
                            ChessSessionState.ChessLegalMove = true;
                        });

                        //Remove previous legal and suggested moves from tracked objects & memory
                        RemoveSuggestedMovesFromTrackedObjects();
                        suggestedMoveDictionary.Clear();
                        RemoveIllegalMovesFromTrackedObjects();
                        illegalMoveDictionary.Clear();
                        RemoveCheckMovesFromTrackedObjects();
                        checkChessMoveDictionary.Clear();

                        Piece tempPiece;

                        if(Pieces.TryGetValue(to, out tempPiece))
                        {
                            if(blackPieces.Contains(tempPiece.chessPiece))
                            {
                                Dispatcher.Current.BeginInvoke(()   => 
                                {
                                    ChessSessionState.ChessPieceCaptured = true;
                                });
                            }
                        }
                        Debug.Log("Move detected from:" + from + " to:" + to + " Piece:" + Pieces[from].chessPiece);
                        RequestChessPlayMove(_lastLegalMoves.Moves[i].fromString, _lastLegalMoves.Moves[i].toString);
                        whiteTurn = false;
                    }
                }
            }
            //if no move made, move is illegal
            if (!moveMade && to != -1 && from != -1 && validMove)
            {
                //prevent chess state from updating constantly
                UpdateIllegalMove(from, to);
                if(illegalMoveDictionary.Count() > 0)
                {
                    Debug.Log("Illegal move made from:" + from + "to:" + to);
                    //Display Message
                    Dispatcher.Current.BeginInvoke(()   => 
                    {
                        ChessSessionState.ChessMessage = "Illegal move, please make a legal move.";
                    });
                }   
                AllowChessStateUpdate = false;
                return;
            }
        }

        // Pieces are only added when everything is in start position
        // otherwise pieces are only moved or removed
        if (IncomingChessState.IsStartPosition())
        {
            // Initialize ids for all pieces 
            Pieces.Clear();
            int pieceId = ChessPieceStartId;
            for (int i = 0; i < 64; i++)
            {
                if (IncomingChessState.Pieces[i] != ChessPieceEnum.no_piece && IncomingChessState.Pieces[i] != 0)
                {
                    Pieces[i] = new Piece(IncomingChessState.Pieces[i], pieceId++);
                }
            }
            //Debug.Log("Start position");
        }
        else
        {
            // Check for changes
            // 1 change => simple move
            // 2 changes => piece capture or castling

            // Gather changed pieces
            Dictionary<Piece, int> changedPieces = new Dictionary<Piece, int>();
            for (int i = 0; i < 64; i++)
            {
                Piece piece;
                //retrieve pieceEnum at position 
                if (Pieces.TryGetValue(i, out piece))
                {
                    //if piece changed on current board
                    if (piece.chessPiece != IncomingChessState.Pieces[i])
                    {
                        changedPieces[piece] = i;
                    }
                }
            }


            // Now find new location for each changed piece

            //if there was no move made, we do not need to worry about updating the piece position fields, only the illegal move fields
            if(moveMade)
            {
                foreach (var changedPiece in changedPieces)
                {
                    int newFieldId = 0;
                // Remove piece from previous position
                //if removing black piece, ignore because we do not get black pieces from chess detector (when black is virtual)
                    if (!blackPieces.Contains(changedPiece.Key.chessPiece))
                    {
                        Pieces.Remove(changedPiece.Value);
                    }

                    if (changedFields.TryGetValue(changedPiece.Key.chessPiece, out newFieldId))
                    {
                        //change pieceEnum at new position to pieceEnum from incoming board
                        Pieces[newFieldId] = changedPiece.Key;
                        Debug.Log("Moving " + changedPiece.Key.chessPiece + " to " + newFieldId);
                    }
                }
            }
        }

        if (autoPlayBlack && !whiteTurn)
        {
            //requests suggested move + auto play suggested move for black
            RequestChessSuggestedMove();
        }
        _lastChessState = IncomingChessState;
        RequestChessLegalMoves();

        UpdatePiecePositions();

        Dispatcher.Current.BeginInvoke(() =>
        {
            ChessSessionState.ChessIsWhiteTurn = whiteTurn;
        });        

        //prevent constant chess state update, only allow this function to be run once per completed turn
        AllowChessStateUpdate = false;
    }


    private Dictionary<int, CheckChessMove> checkChessMoveDictionary = new Dictionary<int, CheckChessMove>();

    //private void UpdatedCheckMoves(int kingPos, int attackerPiecePos)
    private void UpdatedCheckMoves(int kingPos)
    {
        RemoveCheckMovesFromTrackedObjects();
        checkChessMoveDictionary.Clear();

        
        if(whiteTurn)
        {
            Dispatcher.Current.BeginInvoke(()   =>
            {
                ChessSessionState.ChessWhiteInCheck = true;
            });
        }
        else
        {
            Dispatcher.Current.BeginInvoke(()   =>
            {
               ChessSessionState.ChessBlackInCheckmate = true;
           });
        }

        var defenderCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[kingPos] : Vector3.zero;
        //var attackerCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[attackerPiecePos] : Vector3.zero;

        int idDefender = CheckMoveStartId + kingPos;
        //int idAttacker = CheckMoveStartId + attackerPiecePos;

        CheckChessMove checkChessMoveDefender;
        //CheckChessMove checkChessMoveAttacker;

        //align

        defenderCenter.y = 0.005f;
        //attackerCenter.y = 0.005f;

        checkChessMoveDefender = new CheckChessMove()
        {
            id = idDefender,
            classId = -1,
            label = "InCheckDefender",
            position = defenderCenter,
            lastUpdate = DateTime.Now + TimeSpan.FromHours(1),
            color = Color.yellow,
            targetPosition = defenderCenter
        };
        /*
        checkChessMoveAttacker = new CheckChessMove()
        {
            id = idAttacker,
            classId = -1,
            label = "InCheckAttacker",
            position = attackerCenter,
            lastUpdate = DateTime.Now + TimeSpan.FromHours(1),
            color = Color.yellow,
            targetPosition = attackerCenter
        };
        */
        checkChessMoveDictionary[idDefender] = checkChessMoveDefender;
        //checkChessMoveDictionary[idAttacker] = checkChessMoveAttacker;

        AddCheckMovesToTrackedObjects();
    }

    private Dictionary<int, CheckmateChessMove> checkmateChessMoveDictionary = new Dictionary<int, CheckmateChessMove>();

    //private void UpdatedCheckmateMoves(int kingPos, int attackerPiecePos)
    private void UpdatedCheckmateMoves(int kingPos)
    {
        RemoveCheckmateMovesFromTrackedObjects();
        checkmateChessMoveDictionary.Clear();

        //Play sounds for checkmate
        if(whiteTurn)
        {
            Dispatcher.Current.BeginInvoke(()   =>
            {
                ChessSessionState.ChessWhiteInCheckmate = true;
            });  
        }
        else
        {
            Dispatcher.Current.BeginInvoke(()   =>
            {
                ChessSessionState.ChessBlackInCheckmate = true;
            });
        }
        

        var defenderCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[kingPos] : Vector3.zero;
        //var attackerCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[attackerPiecePos] : Vector3.zero;

        int idDefender = CheckmateMoveStartId + kingPos;
        //int idAttacker = CheckmateMoveStartId + attackerPiecePos;

        CheckmateChessMove checkmateChessMoveDefender;
        //CheckmateChessMove checkmateChessMoveAttacker;

        //align

        defenderCenter.y = 0.005f;
        //attackerCenter.y = 0.005f;

        checkmateChessMoveDefender = new CheckmateChessMove()
        {
            id = idDefender,
            classId = -1,
            label = "InCheckmateDefender",
            position = defenderCenter,
            lastUpdate = DateTime.Now + TimeSpan.FromHours(1),
            color = Color.yellow,
            targetPosition = defenderCenter
        };
        /*
        checkmateChessMoveAttacker = new CheckmateChessMove()
        {
            id = idAttacker,
            classId = -1,
            label = "InCheckmateAttacker",
            position = attackerCenter,
            lastUpdate = DateTime.Now + TimeSpan.FromHours(1),
            color = Color.yellow,
            targetPosition = attackerCenter
        };
        */
        checkmateChessMoveDictionary[idDefender] = checkmateChessMoveDefender;
        //checkmateChessMoveDictionary[idAttacker] = checkmateChessMoveAttacker;

        AddCheckmateMovesToTrackedObjects();
    }
    
    // Keep a dictionary of LegalChessMove (special purpose TrackedObjects) with their id
    private Dictionary<int, LegalChessMove> legalChessMoveDictionary = new Dictionary<int, LegalChessMove>();

    private void UpdatedLegalMoves(LegalMoves legalMoves)
    {
        // Remove all previous legalMoves from trackedobjects
        RemoveLegalMovesFromTrackedObjects();
        legalChessMoveDictionary.Clear();

        for (int i = 0; i < legalMoves.Moves.Count(); i++)
        {

            var fromCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[legalMoves.Moves[i].fromFieldId] : Vector3.zero;
            var toCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[legalMoves.Moves[i].toFieldId] : Vector3.zero;
            int id = LegalMoveStartId + (int)legalMoves.Moves[i].fromFieldId;

            LegalChessMove legalChessMove;

            //Align legal moves to table plane regardless of position
            fromCenter.y = 0.005f;
            toCenter.y = 0.005f;

            if (!legalChessMoveDictionary.TryGetValue(id, out legalChessMove))
            {
                legalChessMove = new LegalChessMove()
                {
                    id = id,
                    classId = -1,
                    label = legalMoves.Moves[i].isWhite ? "WhiteLegalMove" : "BlackLegalMove",
                    position = fromCenter,
                    lastUpdate = DateTime.Now + TimeSpan.FromHours(1),
                    color = legalMoves.Moves[i].isWhite ? Color.yellow : Color.blue,
                    targetPositions = new List<Vector3>()
                };
                legalChessMoveDictionary[id] = legalChessMove;
            }
            legalChessMove.targetPositions.Add(toCenter);
        }

        // Add legal chess moves
        AddLegalMovesToTrackedObjects();
        if (legalChessMoveDictionary.Count <= 0)
            Debug.Log("No legal moves, (update legal moves)");

        _lastLegalMoves = legalMoves;
    }

    private Dictionary<int, IllegalChessMove> illegalMoveDictionary = new Dictionary<int, IllegalChessMove>();

    private void UpdateIllegalMove(int from, int to)
    {
        RemoveIllegalMovesFromTrackedObjects();
        illegalMoveDictionary.Clear();

        var fromCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[from] : Vector3.zero;
        var toCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[to] : Vector3.zero;
        int idFrom = IllegalMoveStartId + from;

        IllegalChessMove illegalChessMoveFrom;

        //Align illegal moves to table plane regardless of position
        fromCenter.y = 0.005f;
        toCenter.y = 0.005f;

        illegalChessMoveFrom = new IllegalChessMove()
        {
            id = idFrom,
            classId = -1,
            label = "IllegalMoveFrom",
            position = fromCenter,
            lastUpdate = DateTime.Now + TimeSpan.FromHours(1),
            color = Color.yellow,
            targetPosition = toCenter
        };

        illegalMoveDictionary[idFrom] = illegalChessMoveFrom;

        AddIllegalMovesToTrackedObjects();

    }

    private Dictionary<int, SuggestedChessMove> suggestedMoveDictionary = new Dictionary<int, SuggestedChessMove>();
    bool autoPlayBlack = true;
    private void UpdatedSuggestedMove(SuggestedMove suggestedMove)
    {
        //Debug.Log("Suggest " + (suggestedMove.isWhite ? "white" : "black") + " from " + suggestedMove.fromFieldId + " to " + suggestedMove.toFieldId);
        int to = (int)suggestedMove.toFieldId;
        int from = (int)suggestedMove.fromFieldId;
        if (autoPlayBlack && !whiteTurn)
        {
            Debug.Log("Playing blacks move");

            Dispatcher.Current.BeginInvoke(()   => 
            {
                ChessSessionState.ChessLegalMove = true;
            });
            
            //try to find piece in pieces, if piece exists and is white, then piece was captured by black
            /*

            11/30 GeorgeZipp:
            if there is no piece occupying the square and the board is coming from piece detector, the squares are read in as 0 instead of ChessPieceEnum.no_piece
            because of this, this code does not move the black side as expected, since when it does the TryGetValue and returns with 0 it assumes that there is no existing piece on the square

            Piece tempPiece;
            if (Pieces.TryGetValue(to, out tempPiece))
            {
                if (whitePieces.Contains(tempPiece.chessPiece))
                {
                    Debug.Log("White piece captured, please remove:" + tempPiece.chessPiece + " from board");
                    Dispatcher.Current.BeginInvoke(()   => 
                    {
                        ChessSessionState.ChessPieceCaptured = true;
                    });
                }

                Pieces[to] = Pieces[from];
                Pieces.Remove(from);
            }
            else
            {
                Debug.Log($"To field {to} was not found in existing Pieces");
            }*/

            //Display Message
            Dispatcher.Current.BeginInvoke(()   => 
            {
                ChessSessionState.ChessMessage = "Black moves " + ChessPieceName(Pieces[to]).Substring(2) +  " from " + suggestedMove.fromString + " to " + suggestedMove.toString;
            });

            Piece tempPiece;
            if(Pieces.TryGetValue(to, out tempPiece))
            {
                if (whitePieces.Contains(tempPiece.chessPiece))
                {
                    Dispatcher.Current.BeginInvoke(() =>
                    {
                        //white piece captured by black
                        ChessSessionState.ChessPieceCaptured = true;
                    });

                    //Display Message
                    Dispatcher.Current.BeginInvoke(() =>
                    {
                        ChessSessionState.ChessMessage = "Your " + ChessPieceName(tempPiece).Substring(2) + " at position " + suggestedMove.toString + " has been captured please remove it from the board.";
                    });

                    pieceRemoved = to;
                    pieceRemovedChessPiece = tempPiece.chessPiece;
                    //if captured, remove piece from board
                }
                else
                {
                    pieceRemoved = -1;
                    pieceRemovedChessPiece = ChessPieceEnum.no_piece;
                }
            }

            Pieces[to] = Pieces[from];
            Pieces.Remove(from);

            RequestChessPlayMove(suggestedMove.fromString, suggestedMove.toString);
            RequestChessCompareBoards();

            whiteTurn = true;
        }
        
        if(suggestedMove.isWhite && whiteTurn)
        {
            Debug.Log("Suggested move from:" + from + " to:" + to);

            //Display Message
            Dispatcher.Current.BeginInvoke(()   => 
            {
                ChessSessionState.ChessMessage = "Suggested move from " + suggestedMove.fromString + " to " + suggestedMove.toString;
            });

            // Remove all previous suggestedMoves from trackedobjects
            RemoveSuggestedMovesFromTrackedObjects();
            suggestedMoveDictionary.Clear();

            var fromCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[from] : Vector3.zero;
            var toCenter = (_lastChessBoard != null) ? _lastChessBoard.Centers[to] : Vector3.zero;
            int idFrom = SuggestedMoveStartId + from;

            //Align suggested moves to table plane regardless of position
            fromCenter.y = 0.005f;
            toCenter.y = 0.005f;

            // RS A single suggested move contains the information of it is source location and it's target location
            SuggestedChessMove suggestedChessMoveFrom;
            suggestedChessMoveFrom = new SuggestedChessMove()
            {
                id = idFrom,
                classId = -1,
                label = "SuggestedMoveFrom",
                position = fromCenter,
                lastUpdate = DateTime.Now + TimeSpan.FromHours(1),
                color = Color.yellow,
                targetPosition = toCenter
            };
            suggestedMoveDictionary[idFrom] = suggestedChessMoveFrom;

            AddSuggestedMovesToTrackedObjects();
        }
    }

    private void AddSuggestedMovesToTrackedObjects()
    {
        foreach (var smi in suggestedMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Add(smi.Value);
            });
        }
    }

    private void RemoveSuggestedMovesFromTrackedObjects()
    {
        foreach (var smi in suggestedMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Remove(smi.Value);
            });
        }
    }

    private void AddIllegalMovesToTrackedObjects()
    {
        foreach (var icm in illegalMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Add(icm.Value);
            });
        }
    }

    private void RemoveIllegalMovesFromTrackedObjects()
    {
        foreach (var icm in illegalMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Remove(icm.Value);
            });
        }
    }

    private void AddLegalMovesToTrackedObjects()
    {
        foreach(var lcm in legalChessMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Add(lcm.Value);
            });
        }
    }

    private void RemoveLegalMovesFromTrackedObjects()
    {
        foreach(var lcm in legalChessMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Remove(lcm.Value);
            });
        }
    }

    private void AddCheckMovesToTrackedObjects()
    {
        foreach (var cmm in checkChessMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Add(cmm.Value);
            });
        }
    }

    private void RemoveCheckMovesFromTrackedObjects()
    {
        foreach (var cmm in checkChessMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Remove(cmm.Value);
            });
        }
    }


    private void AddCheckmateMovesToTrackedObjects()
    {
        foreach(var cmcm in checkmateChessMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Add(cmcm.Value);
            });
        }
    }

    private void RemoveCheckmateMovesFromTrackedObjects()
    {
        foreach(var cmcm in checkmateChessMoveDictionary)
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                SessionState.TrackedObjects.Remove(cmcm.Value);
            });
        }
    }

    private int GetPiecePos(ChessPieceEnum targetPiece)
    {
        for (int i = 0; i < 64; i++)
        {
            Piece tempPiece;
            if (Pieces.TryGetValue(i, out tempPiece))
            {
                if (tempPiece.chessPiece == targetPiece)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    //Unneeded?
    /*
    bool playDetectedWhiteMove = true;
    private void UpdatedDetectorMoves(DetectorMoves detectorMoves)
    {
        Debug.Log("********** Detected moves: " + detectorMoves.Moves.Count());

        foreach (var move in detectorMoves.Moves)
        {
            Debug.Log("Detected " + ( move.isWhite ? "white" : "black") + " from  " + move.fromString + " to  " + move.toString);
        }

        // Find the a legal move in the detectorMoves and play it
        if (playDetectedWhiteMove && _lastChessState.IsWhiteTurn)
        {
            Debug.Log("Move detected on whites turn");
            foreach (var detectorMove in detectorMoves.Moves)
            {
                // Debug.Log("From " + detectorMove.fromString + " to  " + detectorMove.toString);

                foreach (var legalMove in _lastLegalMoves.Moves)
                {
                    if (detectorMove.isWhite && legalMove.isWhite && legalMove.fromString == detectorMove.fromString && legalMove.toString == detectorMove.toString)
                    {
                        Debug.Log("Legal Move Detected");
                        //RequestChessPlayMove(detectorMove.fromString, detectorMove.toString);
                        return;
                    }
                }
            }
        }

        Debug.Log("No legal move detected");
    }
    */

    private class Piece
    {
        public ChessPieceEnum chessPiece;
        public int pieceId;

        public Piece(ChessPieceEnum chessPiece, int pieceId)
        {
            this.chessPiece = chessPiece;
            this.pieceId = pieceId;
        }
    }

    // Maps from location id to piece
    private Dictionary<int, Piece> Pieces = new Dictionary<int, Piece>();

    private void UpdatePiecePositions()
    {
        //Debug.Log("updating piece positions");
        bool[] pieceActive = new bool[32];

        // Go through list of active pieces 
        foreach (var piece in Pieces)
        {
            pieceActive[piece.Value.pieceId - ChessPieceStartId] = true;

            var center = (_lastChessBoard != null) ? _lastChessBoard.Centers[piece.Key] : Vector3.zero;

            TrackedObject trackedObject;

            //Align pieces to table plane regardless of position
            center.y = 0.005f;

            if (!TrackedObjectDictionary.TryGetValue(piece.Value.pieceId, out trackedObject))
            {
                trackedObject = new TrackedObject()
                {
                    id = piece.Value.pieceId,
                    classId = -1,
                    label = ChessPieceName(piece.Value),
                    position = center,
                    lastUpdate = DateTime.Now + TimeSpan.FromHours(1),     // Let it live for an hour for better reuse
                    color = (piece.Value.chessPiece >= ChessPieceEnum.b_bishop) ? Color.blue : Color.red,
                };

                lock (TrackedObjectDictionary)
                {
                    TrackedObjectDictionary[piece.Value.pieceId] = trackedObject;
                }

                Dispatcher.Current.BeginInvoke(() =>
                {
                    SessionState.TrackedObjects.Add(trackedObject);
                });
                //Debug.Log("instantiating piece at:" + piece.Key + " piece name:" + ChessPieceName(piece.Value) + " position:" + center);
            }
            else
            {
                // Reuse
                trackedObject.position = center;
                trackedObject.lastUpdate = DateTime.Now + TimeSpan.FromHours(1);     // Let it live for an hour
            }
        }

        // Remove inactive pieces
        
        for (int i = 0; i < 32; i++)
        {
            
            if (!pieceActive[i])
            {
                TrackedObject trackedObject;
                if (TrackedObjectDictionary.TryGetValue(ChessPieceStartId + i, out trackedObject))
                {
                    Debug.Log("removing piece at:" + i + " of type:" + trackedObject.label);
                    lock (TrackedObjectDictionary)
                    {
                        TrackedObjectDictionary.Remove(ChessPieceStartId + i);
                    }
                    Dispatcher.Current.BeginInvoke(() =>
                    {
                        SessionState.TrackedObjects.Remove(trackedObject);
                    });
                }
            }
        }

    }

   
    

    void IChessControl.RequestChessNewGame(int skillLevel, int thinkTime)
    {
        //permit 1 state and board update per call
        AllowChessStateUpdate = true;
        whiteTurn = true;
        RemoveSuggestedMovesFromTrackedObjects();
        suggestedMoveDictionary.Clear();
        RemoveIllegalMovesFromTrackedObjects();
        illegalMoveDictionary.Clear();
        RemoveCheckMovesFromTrackedObjects();
        checkChessMoveDictionary.Clear();
        RemoveCheckmateMovesFromTrackedObjects();
        checkmateChessMoveDictionary.Clear();

        //if new game, _lastChessState should not exist
        _lastChessState = null;

        PingServer(packet_type.packet_client_chess_start_new_game, 10, 350, 0);
        // RS We need an initial detection before we can compare
        PingServer(packet_type.packet_client_chess_request_compare_boards);
    }

    public void RequestChessPlayMove(string from, string to)
    {
        PingServer(packet_type.packet_client_chess_play_move, from, to);
    }

    void IChessControl.RequestChessDetectBoard()
    {
        PingServer(packet_type.packet_client_chess_request_detect_board);
    }

    public void RequestChessLegalMoves()
    {
        PingServer(packet_type.packet_client_chess_request_legal_moves);
    }

    public void RequestChessSuggestedMove()
    {
        PingServer(packet_type.packet_client_chess_request_suggested_move);
    }

    public void RequestChessCompareBoards()
    {
        //permit 1 state and board update per call
        AllowChessStateUpdate = true;
        PingServer(packet_type.packet_client_chess_request_compare_boards);
    }
}