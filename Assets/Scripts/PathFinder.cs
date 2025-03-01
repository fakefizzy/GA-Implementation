using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathFinder : MonoBehaviour
{
    public enum Algorithm { BFS, DFS, Dijkstra, AStar }
    private GridManager gridManager;
    public bool isSearching = false;
    private List<Tile> visitedTiles = new List<Tile>();
    private List<Tile> pathTiles = new List<Tile>();
    private Color pathColor = new Color(0.3f, 0.7f, 1f); //light blue
    private Color visitedColor = new Color(0.9f, 0.7f, 1f); //light purple
    private float visualizationDelay = 0.5f;

    private void Start()
    {
        gridManager = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();
    }

    public void StartPathfinding(Algorithm algorithm)
    {
        if (isSearching) return;
        if (!gridManager.startExists || !gridManager.endExists)
        {
            Debug.LogWarning("no start or end");
            return;
        }

        ClearPreviousPath();
        isSearching = true;

        switch (algorithm)
        {
            case Algorithm.BFS:
                StartCoroutine(BFS());
                break;
            case Algorithm.DFS:
                StartCoroutine(DFS());
                break;
            case Algorithm.Dijkstra:
                StartCoroutine(Dijkstra());
                break;
            case Algorithm.AStar:
                StartCoroutine(AStar());
                break;
        }
    }

    private void ClearPreviousPath()
    {
        foreach (var tile in visitedTiles)
        {
            if (tile != null && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
            {
                tile.ChangeTileType(Tile.TileType.Path);
            }
        }
        visitedTiles.Clear();

        foreach (var tile in pathTiles)
        {
            if (tile != null && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
            {
                tile.ChangeTileType(Tile.TileType.Path);
            }
        }
        pathTiles.Clear();
    }

    private (Tile startTile, Tile endTile, Vector2Int startPos, Vector2Int endPos) FindStartAndEndTiles()
    {
        Tile startTile = null;
        Tile endTile = null;
        Vector2Int startPos = Vector2Int.zero;
        Vector2Int endPos = Vector2Int.zero;

        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                GameObject tileObj = GetTileObjectAt(new Vector2Int(x, y));
                if (tileObj != null)
                {
                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile != null)
                    {
                        if (tile.type == Tile.TileType.Start)
                        {
                            startTile = tile;
                            startPos = new Vector2Int(x, y);
                        }
                        else if (tile.type == Tile.TileType.End)
                        {
                            endTile = tile;
                            endPos = new Vector2Int(x, y);
                        }
                    }
                }
            }
        }

        return (startTile, endTile, startPos, endPos);
    }

    private GameObject GetTileObjectAt(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return null;

        Transform parent = gridManager.transform;
        Vector2 worldPos = new Vector2(
            pos.x - gridManager.width / 2f + 0.5f,
            pos.y - gridManager.height / 2f + 0.5f
        );

        foreach (Transform child in parent)
        {
            if (Vector2.Distance(child.position, worldPos) < 0.1f)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    private Tile GetTileAt(Vector2Int pos)
    {
        GameObject tileObj = GetTileObjectAt(pos);
        if (tileObj == null) return null;
        return tileObj.GetComponent<Tile>();
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridManager.width && pos.y >= 0 && pos.y < gridManager.height;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (IsInBounds(neighbor))
            {
                Tile tile = GetTileAt(neighbor);
                if (tile != null && (tile.type == Tile.TileType.Path || tile.type == Tile.TileType.End))
                {
                    neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }

    private void VisualizePath(List<Vector2Int> path)
    {
        pathTiles.Clear();
        foreach (var pos in path)
        {
            Tile tile = GetTileAt(pos);
            if (tile != null && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
            {
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = pathColor;
                    pathTiles.Add(tile);
                }
            }
        }
    }

    private void VisualizeVisited(Vector2Int pos)
    {
        Tile tile = GetTileAt(pos);
        if (tile != null && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
        {
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = visitedColor;
                visitedTiles.Add(tile);
            }
        }
    }

    private IEnumerator BFS()
    {
        var (startTile, endTile, startPos, endPos) = FindStartAndEndTiles();

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            VisualizeVisited(current);

            yield return new WaitForSeconds(visualizationDelay);

            if (current == endPos)
            {
                List<Vector2Int> path = ReconstructPath(cameFrom, startPos, endPos);
                VisualizePath(path);
                isSearching = false;
                yield break;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        Debug.Log("no path with BFS");
        isSearching = false;
    }

    private IEnumerator DFS()
    {
        var (startTile, endTile, startPos, endPos) = FindStartAndEndTiles();

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        stack.Push(startPos);
        visited.Add(startPos);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            VisualizeVisited(current);

            yield return new WaitForSeconds(visualizationDelay);

            if (current == endPos)
            {
                List<Vector2Int> path = ReconstructPath(cameFrom, startPos, endPos);
                VisualizePath(path);
                isSearching = false;
                yield break;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    stack.Push(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        Debug.Log("no path with DFS");
        isSearching = false;
    }

    private IEnumerator Dijkstra()
    {
        var (startTile, endTile, startPos, endPos) = FindStartAndEndTiles();

        List<(Vector2Int pos, float cost)> frontier = new List<(Vector2Int, float)>();
        frontier.Add((startPos, 0));

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();

        cameFrom[startPos] = startPos;
        costSoFar[startPos] = 0;

        while (frontier.Count > 0)
        {
            frontier.Sort((a, b) => a.cost.CompareTo(b.cost));
            var current = frontier[0].pos;
            frontier.RemoveAt(0);

            VisualizeVisited(current);
            yield return new WaitForSeconds(visualizationDelay);

            if (current == endPos)
            {
                List<Vector2Int> path = ReconstructPath(cameFrom, startPos, endPos);
                VisualizePath(path);
                isSearching = false;
                yield break;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                float newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    cameFrom[neighbor] = current;
                    frontier.Add((neighbor, newCost));
                }
            }
        }

        Debug.Log("no path with Dijkstra");
        isSearching = false;
    }

    private IEnumerator AStar()
    {
        var (startTile, endTile, startPos, endPos) = FindStartAndEndTiles();

        List<(Vector2Int pos, float priority)> frontier = new List<(Vector2Int, float)>();
        frontier.Add((startPos, 0));

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();

        cameFrom[startPos] = startPos;
        costSoFar[startPos] = 0;

        while (frontier.Count > 0)
        {
            frontier.Sort((a, b) => a.priority.CompareTo(b.priority));
            var current = frontier[0].pos;
            frontier.RemoveAt(0);

            VisualizeVisited(current);
            yield return new WaitForSeconds(visualizationDelay);

            if (current == endPos)
            {
                List<Vector2Int> path = ReconstructPath(cameFrom, startPos, endPos);
                VisualizePath(path);
                isSearching = false;
                yield break;
            }

            foreach (var neighbor in GetNeighbors(current))
     
            {
                float newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    float priority = newCost + Heuristic(neighbor, endPos);
                    frontier.Add((neighbor, priority));
                    cameFrom[neighbor] = current;
                }
            }
        }

        Debug.Log("No path found with A*");
        isSearching = false;
    }

    //manhattan distance
    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = end;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }

    public void SetVisualizationSpeed(float speedMultiplier)
    {
        speedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 150f);
        visualizationDelay = 0.5f / speedMultiplier;
    }

    public void CancelPathfinding()
    {
        ClearPreviousPath();
        if (isSearching)
        {
            StopAllCoroutines();
            isSearching = false;
            Debug.Log("pathfinding cancelled");
        }
    }

    //todo double check algorithms, logging panel on the left
}