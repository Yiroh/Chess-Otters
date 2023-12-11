using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Art Variables")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // LOGIC
    private ChessPiece[,] chessPieces; // All active pieces on the board.
    private const int tileCountX = 8;
    private const int tileCountY = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private void Awake()
    {
        GenerateAllTiles(tileSize, tileCountX, tileCountY);
        SpawnAllPieces();
        PositionAllPieces();
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
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
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
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                // Change new tile
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = hoverMaterial;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }
        else
        {
            if(currentHover != -Vector2Int.one)
            {
                // Change old tile
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                currentHover = -Vector2Int.one;
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
        Debug.Log("Spawning piece type: " + type + " for team: " + team);

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
        chessPieces[x, y].transform.position = GetTileCenter(x, y);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, 0, y * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2);
    }


    // Helper Functions / Operations
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
