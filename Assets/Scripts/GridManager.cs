using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int size = 21;
    int width;
    int height;
    public GameObject tilePrefab;
    private Camera mainCamera;
    private GameObject[,] grid;
    private bool isDragging = false;
    private Tile.TileType currentDragType;

    public float tileSize = 1f;

    void Start()
    {
        mainCamera = Camera.main;
        width = size; height = size;
        mainCamera.orthographicSize =  (float)size / 2;
        GenerateGrid();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            currentDragType = Tile.TileType.Path;
            HandleTileClick();
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
        if (tile != null && tile.type != Tile.TileType.BorderWall)
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
                    tile.type = Tile.TileType.Path;
                }

                grid[x, y] = tileObj;
            }
        }
    }
}
