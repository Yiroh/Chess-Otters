using System;
using UnityEngine;
using System.Collections.Generic;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Diagonal if enemy lays on that tile
        
        // If Blue = Go Up     If Red = Go Down
        int direction = (team == 0) ? 1 : -1;

        // Go one space forward
        if(board[currentX, currentY + direction] == null)
        {
            r.Add(new Vector2Int(currentX, currentY + direction));
        }

        // Go two spaces forward (on first move)
        if(board[currentX, currentY + direction] == null)
        {
            if(team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
            {
                // @ Initial Position
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
            if(team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
            {
                // @ Initial Position
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
        }

        // Diagonal (Kill Move)
        if(currentX != (tileCountX - 1))
        {
            if(board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
            {
                r.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }
        if(currentX != 0)
        {
            if(board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
            {
                r.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;
        if(team == 0 && currentY == 6 || team == 1 && currentY == 1)
        {
            return SpecialMove.Promotion;
        }

        // En Passant
        if(moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            // If last piece was a pawn
            if(board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn)
            {
                // Two up as a pawn
                if(Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)
                {
                    if(board[lastMove[1].x, lastMove[1].y].team != team)
                    {
                        // On same row
                        if(lastMove[1].y == currentY)
                        {
                            if(lastMove[1].x == currentX - 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                            if(lastMove[1].x == currentX + 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.None;
    }
}
