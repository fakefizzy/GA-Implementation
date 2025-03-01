using System.Collections.Generic;
using UnityEngine;
using static Tile;

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
        width = size; height = size;
        mainCamera.orthographicSize = (float)size / 2;
        GenerateGrid();
        GenerateMaze();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Tile tile = hit.collider.GetComponent<Tile>();

                if (tile != null && tile.type == Tile.TileType.BorderWall || tile.type == Tile.TileType.Start || tile.type == Tile.TileType.End)
                {
                    HandleBorderClick();
                }
                else
                {
                    isDragging = true;
                    currentDragType = Tile.TileType.Path;
                    HandleTileClick();
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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null && tile.type != Tile.TileType.BorderWall && tile.type != TileType.Start && tile.type != TileType.End)
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
                Destroy(tile);
            }
        }

        grid = new GameObject[width, height];
        Vector2 gridOffset = new Vector2(width / 2f - 0.5f, height / 2f - 0.5f); //offset the grid to be centered

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y) - gridOffset;
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity);
                tileObj.transform.parent = transform;

                Tile tile = tileObj.GetComponent<Tile>();

                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    tile.SetAsBorderWall();
                }
                else
                {
                    tile.type = Tile.TileType.Wall;
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
            if (neighbor.x > 0 && neighbor.y > 0 && neighbor.x < width - 1 && neighbor.y < height - 1)
            {
                Tile tile = grid[neighbor.x, neighbor.y].GetComponent<Tile>();
                if (tile.type == Tile.TileType.Wall)
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
        grid[a.x, a.y].GetComponent<Tile>().ChangeTileType(Tile.TileType.Path);
        grid[b.x, b.y].GetComponent<Tile>().ChangeTileType(Tile.TileType.Path);
        grid[wallPos.x, wallPos.y].GetComponent<Tile>().ChangeTileType(Tile.TileType.Path);
    }

    void HandleBorderClick()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        Tile tile = hit.collider.GetComponent<Tile>();

        if (!startExists)
        {
            startExists = true;
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
            tile.ChangeTileType(Tile.TileType.End);
        }
        else if (tile.type == Tile.TileType.End)
        {
            endExists = false;
            tile.ChangeTileType(Tile.TileType.BorderWall);
        }
    }
}
