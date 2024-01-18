using System.Runtime.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ACAM2.MessagePack
{
    public enum ChessPieceEnum
    {
        no_piece = 32,

        b_pawn = 112,   // p
        b_rook = 114,   // r
        b_knight = 110, // n
        b_bishop = 98,  // b
        b_queen = 113,  // q
        b_king = 107,   // k

        w_pawn = 80,    // P
        w_rook = 82,    // R
        w_knight = 78,  // N
        w_bishop = 66,  // B
        w_queen = 81,   // Q
        w_king = 75,    // K
    };

    [DataContract]
    public class ChessState
    {
        public byte Type { get; set; }
        public byte PacketVersion { get; set; }
        public uint ServerId { get; set; }
        public bool IsWhiteTurn { get; set; }
        public bool IsLegal { get; set; }
        public bool IsFromPieceDetector { get; set; }
        public string IllegalReason { get; set; }
        public ChessPieceEnum[] Pieces { get; set; }

        public static explicit operator ChessState(object[] fields) => new ChessState()
        {
            Type = (byte)fields[0],
            PacketVersion = (byte)fields[1],
            ServerId = (uint)fields[2],
            IsWhiteTurn = (byte)fields[3] > 0,
            IsLegal = (byte)fields[4] > 0,
            IsFromPieceDetector = (byte)fields[5] > 0,
            IllegalReason = (string)fields[6],
            Pieces = convertIntArray(fields, 7, 64)
        };

        private static ChessPieceEnum[] convertIntArray(object[] fields, int start, int count)
        {
            var array = new ChessPieceEnum[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = (ChessPieceEnum)(byte)fields[start + i];
            }
            return array;
        }

        
        private ChessPieceEnum[] StartPiecesChessEngine = {    ChessPieceEnum.b_rook, ChessPieceEnum.b_knight, ChessPieceEnum.b_bishop, ChessPieceEnum.b_queen, ChessPieceEnum.b_king, ChessPieceEnum.b_bishop, ChessPieceEnum.b_knight, ChessPieceEnum.b_rook,
                                                    ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn,
                                                    ChessPieceEnum.no_piece, ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,
                                                    ChessPieceEnum.no_piece, ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,
                                                    ChessPieceEnum.no_piece, ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,
                                                    ChessPieceEnum.no_piece, ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,ChessPieceEnum.no_piece,
                                                    ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn,
                                                    ChessPieceEnum.w_rook, ChessPieceEnum.w_knight, ChessPieceEnum.w_bishop, ChessPieceEnum.w_queen, ChessPieceEnum.w_king, ChessPieceEnum.w_bishop, ChessPieceEnum.w_knight, ChessPieceEnum.w_rook };
        
        //empty squares in piece detector show up as 0 instead of ChessPieceEnum.no_piece (32)
        private ChessPieceEnum[] StartPiecesPieceDetector = {    ChessPieceEnum.b_rook, ChessPieceEnum.b_knight, ChessPieceEnum.b_bishop, ChessPieceEnum.b_queen, ChessPieceEnum.b_king, ChessPieceEnum.b_bishop, ChessPieceEnum.b_knight, ChessPieceEnum.b_rook,
                                                    ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn, ChessPieceEnum.b_pawn,
                                                    0,0,0,0,0,0,0,0,
                                                    0,0,0,0,0,0,0,0,
                                                    0,0,0,0,0,0,0,0,
                                                    0,0,0,0,0,0,0,0,
                                                    ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn, ChessPieceEnum.w_pawn,
                                                    ChessPieceEnum.w_rook, ChessPieceEnum.w_knight, ChessPieceEnum.w_bishop, ChessPieceEnum.w_queen, ChessPieceEnum.w_king, ChessPieceEnum.w_bishop, ChessPieceEnum.w_knight, ChessPieceEnum.w_rook };

        private ChessPieceEnum[] blackPieces = { ChessPieceEnum.b_rook, ChessPieceEnum.b_knight, ChessPieceEnum.b_bishop, ChessPieceEnum.b_queen, ChessPieceEnum.b_king, ChessPieceEnum.b_pawn };
        private ChessPieceEnum[] whitePieces = { ChessPieceEnum.w_rook, ChessPieceEnum.w_knight, ChessPieceEnum.w_bishop, ChessPieceEnum.w_queen, ChessPieceEnum.w_king, ChessPieceEnum.w_pawn };


        //Create custom starting positions here
        public bool IsStartPosition()
        {
            //if no pieces return false
            if (Pieces == null || Pieces.Length != 64)
            {
                return false;
            }

            for(int i = 0; i < 64; i++)
            {
                if (IsFromPieceDetector && StartPiecesPieceDetector[i] != Pieces[i] && !blackPieces.Contains(StartPiecesPieceDetector[i]))
                {
                    return false;
                } 
                else if (!IsFromPieceDetector && StartPiecesChessEngine[i] != Pieces[i])
                {
                    return false;
                }
            }
            
            //If the board is in starting position and pieces are from the piece detector
            if (IsFromPieceDetector)
            {
                for (int i = 0; i < 16; i++)
                {
                    //setup black pieces in default starting position
                    Pieces[i] = StartPiecesPieceDetector[i];
                }
            }
            return true;
        }

        /* to be used with custom starting positions
        public bool IsStartPosition(ChessPieceEnum[] newStartPos)
        {
            if(Pieces == null || Pieces.Length != 64)
            {
                return false;
            }
            ChessPieceEnum emptySquare = IsFromPieceDetector == true ? 0 : ChessPieceEnum.no_piece;
            for(int i = 0; i < 64; i++)
            {
                if(newStartPos[i] != Pieces[i] && newStartPos[i] != emptySquare && Pieces[i] != emptySquare)
                {
                    return false;
                }
            }

            return true;
        }
        */
    }
}
