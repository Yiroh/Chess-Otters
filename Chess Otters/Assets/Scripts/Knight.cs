using System;
using UnityEngine;
using System.Collections.Generic;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Up 2 and Right
        int x = currentX + 1;
        int y = currentY + 2;
        if(x < tileCountX && y < tileCountY)
        {
            if(board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        // Right 2 and Up
        x = currentX + 2;
        y = currentY + 1;
        if(x < tileCountX && y < tileCountY)
        {
            if(board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        // Up 2 and Left
        x = currentX - 1;
        y = currentY + 2;
        if(x >= 0 && y < tileCountY)
        {
            if(board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        // Left 2 and Up
        x = currentX - 2;
        y = currentY + 1;
        if(x >= 0 && y < tileCountY)
        {
            if(board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        // Down 2 and Right
        x = currentX + 1;
        y = currentY - 2;
        if(x < tileCountX && y >= 0)
        {
            if(board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        // Right 2 and Down
        x = currentX + 2;
        y = currentY - 1;
        if(x < tileCountX && y >= 0)
        {
            if(board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        // Down 2 and Left
        x = currentX - 1;
        y = currentY - 2;
        if(x >= 0 && y >= 0)
        {
            if(board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

         // Left 2 and Down
        x = currentX - 2;
        y = currentY - 1;
        if(x >= 0 && y >= 0)
        {
            if(board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        return r;
    }
}
