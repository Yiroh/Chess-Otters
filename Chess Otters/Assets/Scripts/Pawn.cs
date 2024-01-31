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
}
