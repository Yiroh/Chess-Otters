using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Art Variables")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float tileSize = 1.0f;

    // LOGIC
    private const int tileCountX = 8;
    private const int tileCountY = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;

    private void Awake()
    {
        GenerateAllTiles(tileSize, tileCountX, tileCountY);
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
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0, (y+1) * tileSize);
        vertices[2] = new Vector3((x+1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x+1) * tileSize, 0, (y+1) * tileSize);

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
