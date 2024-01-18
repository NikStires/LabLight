using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for 
/// -chess control
/// </summary>
public interface IChessControl
{
    /// <summary>
    /// Request a new chess game from lighthouse
    /// </summary>
    public void RequestChessNewGame(int skillLevel = 1, int thinkTime = 999);

    /// <summary>
    /// Request chess move
    /// </summary>
    public void RequestChessPlayMove(string from, string to);

    /// <summary>
    /// Request chess board detection
    /// </summary>
    public void RequestChessDetectBoard();

    /// <summary>
    /// Request legal chess moves
    /// </summary>
    public void RequestChessLegalMoves();

    /// <summary>
    /// Request a suggested chess move
    /// </summary>
    public void RequestChessSuggestedMove();

    /// <summary>
    /// Request a comparison to detect if lighthouse can extract a move 
    /// </summary>
    public void RequestChessCompareBoards();
}