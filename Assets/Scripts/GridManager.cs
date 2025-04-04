using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    private GameObject[,] grid;

    private PathFinder pathFinder;
    private Camera mainCamera;
    private ButtonHelper buttonHelper;

    public int size = 21;
    public int width, height;

    private bool isDragging = false;
    public bool startExists;
    public bool endExists;

    private readonly System.Random rand = new();
    private Tile.TileType currentDragType;

    void Start()
    {
        pathFinder = FindObjectOfType<PathFinder>();
        buttonHelper = FindObjectOfType<ButtonHelper>();
        mainCamera = Camera.main;

        width = size;
        height = size;

        SetupCamera();
        GenerateGrid();
        GenerateMaze();
    }

    public void SetupCamera()
    {
        CameraController existingController = mainCamera.gameObject.GetComponent<CameraController>();
        if (existingController != null)
        {
            Destroy(existingController);
        }

        mainCamera.orthographicSize = (size / 2f) + 2f;

        float centerX = (width - 1) / 2f;
        float centerY = (height - 1) / 2f;

        mainCamera.transform.position = new Vector3(centerX, centerY, -10);

        CameraController controller = mainCamera.gameObject.AddComponent<CameraController>();
        controller.minZoom = 2.0f;
        controller.maxZoom = (size / 2f) + 2f;
    }

    void Update()
    {
        if (!pathFinder.isSearching)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    Tile tile = hit.collider.GetComponent<Tile>();

                    if (tile != null)
                    {
                        if (tile.type == Tile.TileType.BorderWall || tile.type == Tile.TileType.Start || tile.type == Tile.TileType.End)
                        {
                            HandleBorderClick(hit);
                        }
                        else
                        {
                            isDragging = true;
                            currentDragType = Tile.TileType.Path;
                            HandleTileClick();
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                currentDragType = Tile.TileType.Wall;
                HandleTileClick();
            }

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                HandleTileClick();
            }
        }
    }

    void HandleTileClick()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null && tile.type != Tile.TileType.BorderWall && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
            {
                tile.ChangeTileType(currentDragType);
            }
        }
    }

    public void GenerateGrid()
    {
        if (grid != null)
        {
            startExists = false;
            endExists = false;
            foreach (GameObject tile in grid)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }
        }

        grid = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity);
                tileObj.transform.parent = transform;

                Tile tile = tileObj.GetComponent<Tile>();

                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    tile.SetAsBorderWall();
                }
                else
                {
                    tile.ChangeTileType(Tile.TileType.Wall);
                }

                grid[x, y] = tileObj;
            }
        }
    }

    public void GenerateMaze()
    {
        Stack<Vector2Int> stack = new();
        Vector2Int startPos = new(1, 1);
        stack.Push(startPos);

        Tile startTile = GetTileAt(startPos);
        if (startTile != null)
        {
            startTile.ChangeTileType(Tile.TileType.Path);
        }

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            List<Vector2Int> neighbors = GetValidNeighbors(current, Tile.TileType.Wall);

            if (neighbors.Count > 0)
            {
                stack.Push(current);
                Vector2Int next = neighbors[rand.Next(neighbors.Count)];

                Tile currentTile = GetTileAt(current);
                Tile nextTile = GetTileAt(next);
                Tile tileWall = GetTileAt((current + next) / 2);
                if (currentTile != null) currentTile.ChangeTileType(Tile.TileType.Path);
                if (nextTile != null) nextTile.ChangeTileType(Tile.TileType.Path);
                if (tileWall != null) tileWall.ChangeTileType(Tile.TileType.Path);

                stack.Push(next);
            }
        }
    }

    void HandleBorderClick(RaycastHit2D hit)
    {
        if (hit.collider == null) return;

        Tile tile = hit.collider.GetComponent<Tile>();
        if (tile == null) return;

        Vector2Int tilePos = Vector2IntFromPosition(hit.transform.position);

        if (!IsTouchingPath(tilePos))
        {
            buttonHelper.LogWarning($"[{System.DateTime.Now}] Selected border wall not touching path!");
            return;
        }

        if (!startExists)
        {
            startExists = true;
            if (tile.type == Tile.TileType.End)
            {
                endExists = false;
            }
            tile.ChangeTileType(Tile.TileType.Start);
        }
        else if (tile.type == Tile.TileType.Start)
        {
            startExists = false;
            tile.ChangeTileType(Tile.TileType.BorderWall);
        }
        else if (!endExists)
        {
            endExists = true;
            if (tile.type == Tile.TileType.Start)
            {
                startExists = false;
            }
            tile.ChangeTileType(Tile.TileType.End);
        }
        else if (tile.type == Tile.TileType.End)
        {
            endExists = false;
            tile.ChangeTileType(Tile.TileType.BorderWall);
        }
    }

    public void MakeAllTilesPath()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tileObj = grid[x, y];
                if (tileObj != null)
                {
                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile != null && tile.type != Tile.TileType.BorderWall && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
                    {
                        tile.ChangeTileType(Tile.TileType.Path);
                    }
                }
            }
        }
    }

    bool IsTouchingPath(Vector2Int pos)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = pos + dir;

            if (IsInBounds(neighborPos))
            {
                Tile neighborTile = GetTileAt(neighborPos);
                if (neighborTile != null && neighborTile.type == Tile.TileType.Path)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<Vector2Int> GetValidNeighbors(Vector2Int pos, Tile.TileType tileType)
    {
        List<Vector2Int> neighbors = new();
        Vector2Int[] directions;

        if (tileType == Tile.TileType.Wall)
        {
            //for maze generation
            directions = new[] { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };
        }
        else
        {
            //for path exploration
            directions = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        }


        foreach (var dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (IsInBounds(neighbor) && neighbor.x > 0 && neighbor.y > 0 && neighbor.x < width - 1 && neighbor.y < height - 1)
            {
                Tile tile = GetTileAt(neighbor);
                if (tile != null && tile.type == tileType)
                {
                    neighbors.Add(neighbor);
                }
            }
            else //For pathfinding
            {
                Tile tile = GetTileAt(neighbor);
                //tileType being the type of tile being looked for (path encompassing end as well), tile.type is the type of the neighbor tile
                if (tile != null && tile.type == Tile.TileType.End && tileType == Tile.TileType.Path) 
                {
                    neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }

    public Tile GetTileAt(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return null;

        GameObject tileObj = grid[pos.x, pos.y];
        if (tileObj == null) return null;

        return tileObj.GetComponent<Tile>();
    }

    bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    Vector2Int Vector2IntFromPosition(Vector3 position)
    {
        return new Vector2Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y)
        );
    }
}
