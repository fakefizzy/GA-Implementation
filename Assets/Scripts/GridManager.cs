using System.Collections.Generic;
using UnityEngine;
using System;

public class GridManager : MonoBehaviour
{
    public int size = 21;
    private int width, height;
    public GameObject tilePrefab;
    private Camera mainCamera;
    private GameObject[,] grid;
    private bool isDragging = false;
    private Tile.TileType currentDragType;
    private System.Random rand = new System.Random();
    public bool startExists;
    public bool endExists;

    public float tileSize = 1f;

    void Start()
    {
        mainCamera = Camera.main;
        width = size;
        height = size;
        mainCamera.orthographicSize = (float)size / 2;
        GenerateGrid();
        GenerateMaze();
    }

    void Update()
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
            foreach (GameObject tile in grid)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }
        }

        grid = new GameObject[width, height];
        Vector2 gridOffset = new Vector2(width / 2f - 0.5f, height / 2f - 0.5f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y) - gridOffset;
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity);
                tileObj.transform.parent = transform;

                Tile tile = tileObj.GetComponent<Tile>();
                if (tile == null)
                {
                    Debug.LogError("prefab missing");
                    continue;
                }

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

    void GenerateMaze()
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int startPos = new Vector2Int(1, 1);
        stack.Push(startPos);

        Tile startTile = GetTileAt(startPos);
        if (startTile != null)
        {
            startTile.ChangeTileType(Tile.TileType.Path);
        }

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            List<Vector2Int> neighbors = GetValidNeighbors(current);

            if (neighbors.Count > 0)
            {
                stack.Push(current);
                Vector2Int next = neighbors[rand.Next(neighbors.Count)];
                RemoveWallBetween(current, next);
                stack.Push(next);
            }
        }
    }

    List<Vector2Int> GetValidNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (IsInBounds(neighbor) && neighbor.x > 0 && neighbor.y > 0 && neighbor.x < width - 1 && neighbor.y < height - 1)
            {
                Tile tile = GetTileAt(neighbor);
                if (tile != null && tile.type == Tile.TileType.Wall)
                {
                    neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }

    void RemoveWallBetween(Vector2Int a, Vector2Int b)
    {
        Vector2Int wallPos = (a + b) / 2;

        Tile tileA = GetTileAt(a);
        Tile tileB = GetTileAt(b);
        Tile tileWall = GetTileAt(wallPos);

        if (tileA != null) tileA.ChangeTileType(Tile.TileType.Path);
        if (tileB != null) tileB.ChangeTileType(Tile.TileType.Path);
        if (tileWall != null) tileWall.ChangeTileType(Tile.TileType.Path);
    }

    void HandleBorderClick(RaycastHit2D hit)
    {
        if (hit.collider == null) return;

        Tile tile = hit.collider.GetComponent<Tile>();
        if (tile == null) return;

        Vector2Int tilePos = Vector2IntFromPosition(hit.transform.position);

        if (!IsTouchingPath(tilePos))
        {
            Debug.Log("Border not touching path");
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

    Tile GetTileAt(Vector2Int pos)
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
            Mathf.RoundToInt(position.x + width / 2f - 0.5f),
            Mathf.RoundToInt(position.y + height / 2f - 0.5f)
        );
    }
}