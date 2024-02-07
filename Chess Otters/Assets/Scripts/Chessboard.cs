using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}

public class Chessboard : MonoBehaviour
{
    [Header("Art Variables")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.15f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deadSize = 0.45f;
    [SerializeField] private float deadSpacing = 0.55f;
    [SerializeField] private float dragOffset = 1f;
    [SerializeField] private GameObject victoryScreen;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // LOGIC
    private ChessPiece[,] chessPieces; // All active pieces on the board.
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadBlueTeam = new List<ChessPiece>();
    private List<ChessPiece> deadRedTeam = new List<ChessPiece>();
    private const int tileCountX = 8;
    private const int tileCountY = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isBlueTurn;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    private void Awake()
    {
        GenerateAllTiles(tileSize, tileCountX, tileCountY);
        SpawnAllPieces();
        PositionAllPieces();
        isBlueTurn = true;
    }


    public void Update()
    {
        if(!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        // Raycast
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // Get the index of the tile hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            if(currentHover == -Vector2Int.one)
            {
                // Nothing hovered to one tile hovered
                currentHover = hitPosition;
                // Change new tile
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = hoverMaterial;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If already hovering a tile:
            if (currentHover != hitPosition)
            {
                // Change old tile
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                int highlightLayer = LayerMask.NameToLayer("Highlight");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = (tiles[currentHover.x, currentHover.y].layer == highlightLayer) ? highlightMaterial : tileMaterial;
                currentHover = hitPosition;
                // Change new tile
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = hoverMaterial;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // if left mouse click
            if(Input.GetMouseButtonDown(0))
            {
                // If there is a chess piece at the location
                if(chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    //Is it our turn?
                    if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isBlueTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isBlueTurn))
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                        
                        // List of tiles to highlight
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, tileCountX, tileCountY);
                        // Also get special moves:
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);
                        HighlightTiles();
                    }
                }
            }

            // if left mouse released
            if(currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if(!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                } 

                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            if(currentHover != -Vector2Int.one)
            {
                // Change old tile
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                int highlightLayer = LayerMask.NameToLayer("Highlight");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = (tiles[currentHover.x, currentHover.y].layer == highlightLayer) ? highlightMaterial : tileMaterial;
                currentHover = -Vector2Int.one;
            }

            if(currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        
        // If dragging a piece
        if(currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }


    // Generates grid of chess board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        // To spawn pieces above and not inside of the board.
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x,y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }
    // Generates a single chess tile
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        // To render the piece
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        // Generate the geometry
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        // Triangle array to generate from vertices
        int[] tris = new int[] {0, 1, 2, 1, 3, 2 };

        // Time to actually generate it
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        // Collider
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }


    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[tileCountX, tileCountY];

        int blueTeam = 0;
        int redTeam = 1;

        // White Team
        chessPieces[0,0] = SpawnSinglePiece(ChessPieceType.Rook, blueTeam);
        chessPieces[1,0] = SpawnSinglePiece(ChessPieceType.Knight, blueTeam);
        chessPieces[2,0] = SpawnSinglePiece(ChessPieceType.Bishop, blueTeam);
        chessPieces[3,0] = SpawnSinglePiece(ChessPieceType.Queen, blueTeam);
        chessPieces[4,0] = SpawnSinglePiece(ChessPieceType.King, blueTeam);
        chessPieces[5,0] = SpawnSinglePiece(ChessPieceType.Bishop, blueTeam);
        chessPieces[6,0] = SpawnSinglePiece(ChessPieceType.Knight, blueTeam);
        chessPieces[7,0] = SpawnSinglePiece(ChessPieceType.Rook, blueTeam);
        for(int i = 0; i < tileCountX; i++)
        {
            chessPieces[i,1] = SpawnSinglePiece(ChessPieceType.Pawn, blueTeam);
        }

        // Black Team
        chessPieces[0,7] = SpawnSinglePiece(ChessPieceType.Rook, redTeam);
        chessPieces[1,7] = SpawnSinglePiece(ChessPieceType.Knight, redTeam);
        chessPieces[2,7] = SpawnSinglePiece(ChessPieceType.Bishop, redTeam);
        chessPieces[3,7] = SpawnSinglePiece(ChessPieceType.Queen, redTeam);
        chessPieces[4,7] = SpawnSinglePiece(ChessPieceType.King, redTeam);
        chessPieces[5,7] = SpawnSinglePiece(ChessPieceType.Bishop, redTeam);
        chessPieces[6,7] = SpawnSinglePiece(ChessPieceType.Knight, redTeam);
        chessPieces[7,7] = SpawnSinglePiece(ChessPieceType.Rook, redTeam);
        for(int i = 0; i < tileCountX; i++)
        {
            chessPieces[i,6] = SpawnSinglePiece(ChessPieceType.Pawn, redTeam);
        }

    }
    // Spawning a single piece
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1]).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        GameObject child = cp.transform.GetChild(1).gameObject;
        child.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }


    // Positioning all of the pieces onto the board properly
    private void PositionAllPieces()
    {
        for(int x = 0; x < tileCountX; x++)
        {
            for(int y = 0; y < tileCountY; y++)
            {
                if(chessPieces[x, y] != null) // Check if a piece is there
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }
    // Positioning a single piece onto the board
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, 0, y * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2);
    }


    // Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].GetComponent<MeshRenderer>().material = highlightMaterial;
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].GetComponent<MeshRenderer>().material = tileMaterial;
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        availableMoves.Clear();
    }


    // CheckMate
    private void CheckMate (int team)
    {
        DisplayVictory(team);
    }
    private void DisplayVictory (int winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }
    public void OnResetButton()
    {
        // Reset UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        // Field Reset
        currentlyDragging = null;
        availableMoves.Clear();
        moveList.Clear();

        // Clean up Chess Pieces
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                if(chessPieces[x, y] != null)
                {
                    Destroy(chessPieces[x,y].gameObject);
                }

                chessPieces[x, y] = null;
            }
        }
        // Destroy dead pieces too
        for (int i = 0; i < deadBlueTeam.Count; i++)
        {
            Destroy(deadBlueTeam[i].gameObject);
        }
        for (int i = 0; i < deadRedTeam.Count; i++)
        {
            Destroy(deadRedTeam[i].gameObject);
        }
        deadBlueTeam.Clear();
        deadRedTeam.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isBlueTurn = true;
    }
    public void OnExitButton()
    {
        Application.Quit();
    }

    
    // Special Moves
    private void ProcessSpecialMove()
    {
        if(specialMove == SpecialMove.EnPassant)
        {
            Vector2Int[] newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            Vector2Int[] targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if(myPawn.currentX == enemyPawn.currentX)
            {
                if(myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    if(enemyPawn.team == 0)
                    {
                        deadBlueTeam.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deadSize);
                        enemyPawn.SetPosition(
                            new Vector3(8 * tileSize, yOffset/2, -1 * tileSize) - bounds + 
                            new Vector3(tileSize / 3, 0, tileSize / 2) + 
                            (Vector3.forward * deadSpacing) * deadBlueTeam.Count);
                    }
                    else {
                        deadRedTeam.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deadSize);
                        enemyPawn.SetPosition(
                            new Vector3(-1 * tileSize, yOffset/2, 8 * tileSize) - bounds + 
                            new Vector3((tileSize * 2)/3, 0, tileSize/2) + 
                            (Vector3.back * deadSpacing) * deadRedTeam.Count);
                    }
                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        if(specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            // Extra check, might not need this since Promoting is Pawn only.
            if(targetPawn.type == ChessPieceType.Pawn)
            {
                if(targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y, true);
                }
                if(targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y, true);
                }
            }
        }

        if(specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            // Left Rook
            if(lastMove[1].x == 2)
            {
                // Blue Team
                if(lastMove[1].y == 0)
                {
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null; 
                }
                // Red Team
                else if(lastMove[1].y == 7)
                {
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            // Right Rook
            else if(lastMove[1].x == 6)
            {
                // Blue Team
                if(lastMove[1].y == 0)
                {
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null; 
                }
                // Red Team
                else if(lastMove[1].y == 7)
                {
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }


    // Helper Functions / Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if(moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if(!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
        {
            return false;
        }

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        // is there another piece on top of where we want to go?
        if (chessPieces[x,y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            if (cp.team == ocp.team)
            {
                return false;
            } else {
                // if moving on top of an enemy team's piece:
                if(ocp.team == 0)
                {

                    // If Blue King
                    if(ocp.type == ChessPieceType.King)
                    {
                        CheckMate(1);
                    }

                    // Blue Team piece
                    deadBlueTeam.Add(ocp);
                    ocp.SetScale(Vector3.one * deadSize);
                    ocp.SetPosition(
                        new Vector3(8 * tileSize, yOffset/2, -1 * tileSize) - bounds + 
                        new Vector3(tileSize / 3, 0, tileSize / 2) + 
                        (Vector3.forward * deadSpacing) * deadBlueTeam.Count);
                }
                else 
                {

                    // If Red King
                    if(ocp.type == ChessPieceType.King)
                    {
                        CheckMate(0);
                    }

                    //Red Team piece
                    deadRedTeam.Add(ocp);
                    ocp.SetScale(Vector3.one * deadSize);
                    ocp.SetPosition(
                        new Vector3(-1 * tileSize, yOffset/2, 8 * tileSize) - bounds + 
                        new Vector3((tileSize * 2)/3, 0, tileSize/2) + 
                        (Vector3.back * deadSpacing) * deadRedTeam.Count);
                }
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isBlueTurn = !isBlueTurn;
        moveList.Add(new Vector2Int[] {previousPosition, new Vector2Int(x, y)} );

        ProcessSpecialMove();

        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                if(tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        // Fail-safe
        return -Vector2Int.one; // -1 -1  (Invalid)
    }
}
