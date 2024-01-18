using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Special purpose tracked object that contains the targets of the legal move
/// </summary>
public class LegalChessMove : TrackedObject
{
    public List<Vector3> targetPositions;
}

//TODO, 11/30 AM
//a lot of overlap here, when suggested and illegal moves are handled the same
//refactor SuggestedChessMove & IllegalChessMove to be "indicatedChessMoves",
//refactor IllegalChessMoveController, SuggestedChessMoveController & Networking.Chess to handle such

public class SuggestedChessMove : TrackedObject
{
    public Vector3 targetPosition; //where the suggested move indicates
}

// The TrackedObject position is used as source location 
public class IllegalChessMove : TrackedObject
{
    public Vector3 targetPosition; //where the illegal move occured
}

public class CheckChessMove : TrackedObject
{
    public Vector3 targetPosition; //piece putting check
}

public class CheckmateChessMove : TrackedObject
{
    public Vector3 targetPosition; //piece putting in checkmate
}