using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class PathFinder : MonoBehaviour
{
    public enum Algorithm { BFS, DFS, Dijkstra, AStar }
    private GridManager gridManager;
    public bool isSearching = false;
    private readonly List<Tile> visitedTiles = new();
    private readonly List<Tile> pathTiles = new();
    private readonly Color pathColor = new(0.3f, 0.7f, 1f); //light blue
    private readonly Color visitedColor = new(0.9f, 0.7f, 1f); //light purple
    private float visualizationDelay = 0.5f;
    private ButtonHelper buttonHelper;

    private void Start()
    {
        buttonHelper = FindObjectOfType<ButtonHelper>();
        gridManager = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ClearPreviousPath();
        isSearching = false;
    }

    public void StartPathfinding(Algorithm algorithm)
    {
        if (isSearching) return;
        if (!gridManager.startExists || !gridManager.endExists)
        {
            buttonHelper.LogWarning($"[{System.DateTime.Now}] Start or end missing!");
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

    private IEnumerator BFS()
    {
        var (startPos, endPos) = FindStartAndEndTiles();

        Queue<Vector2Int> queue = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        HashSet<Vector2Int> visited = new();
        List<Vector2Int> path = null;
        bool pathFound = false;

        queue.Enqueue(startPos);
        visited.Add(startPos);
        cameFrom[startPos] = startPos;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            VisualizeVisited(current);
            yield return new WaitForSeconds(visualizationDelay);

            if (current == endPos && !pathFound)
            {
                pathFound = true;
                path = ReconstructPath(cameFrom, startPos, endPos);
            }

            foreach (var neighbor in gridManager.GetValidNeighbors(current, Tile.TileType.Path))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        if (pathFound)
        {
            yield return StartCoroutine(VisualizePath(path));
        }
        else
        {
            buttonHelper.LogWarning($"[{System.DateTime.Now}] No path found with BFS");
        }

        isSearching = false;
    }

    private IEnumerator DFS()
    {
        var (startPos, endPos) = FindStartAndEndTiles();

        Stack<Vector2Int> stack = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        HashSet<Vector2Int> visited = new();

        List<Vector2Int> path = null;
        bool pathFound = false;

        stack.Push(startPos);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();

            if (!visited.Contains(current))
            {
                visited.Add(current);
                VisualizeVisited(current);

                yield return new WaitForSeconds(visualizationDelay);

                if (current == endPos && !pathFound)
                {
                    pathFound = true;
                    path = ReconstructPath(cameFrom, startPos, endPos);
                }

                List<Vector2Int> neighbors = gridManager.GetValidNeighbors(current, Tile.TileType.Path);

                for (int i = neighbors.Count - 1; i >= 0; i--)
                {
                    Vector2Int neighbor = neighbors[i];
                    if (!visited.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }
        }

        if (pathFound)
        {
            yield return StartCoroutine(VisualizePath(path));
        }
        else
        {
            buttonHelper.LogWarning($"[{System.DateTime.Now}] No path found with DFS");
        }

        isSearching = false;
    }


    private IEnumerator Dijkstra() //basically BFS in an unweighted graph
    {
        var (startPos, endPos) = FindStartAndEndTiles();

        List<(Vector2Int pos, float cost)> frontier = new() { (startPos, 0) };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> costSoFar = new();

        cameFrom[startPos] = startPos;
        costSoFar[startPos] = 0;

        while (frontier.Count > 0)
        {
            int lowestIndex = 0;
            for (int i = 1; i < frontier.Count; i++)
            {
                if (frontier[i].cost < frontier[lowestIndex].cost)
                    lowestIndex = i;
            }

            Vector2Int current = frontier[lowestIndex].pos;
            frontier.RemoveAt(lowestIndex);

            VisualizeVisited(current);
            yield return new WaitForSeconds(visualizationDelay);

            if (current == endPos)
            {
                yield return StartCoroutine(VisualizePath(ReconstructPath(cameFrom, startPos, endPos)));
                isSearching = false;
                yield break;
            }

            foreach (Vector2Int neighbor in gridManager.GetValidNeighbors(current, Tile.TileType.Path))
            {
                float movementCost = GetMovementCost(current, neighbor);
                float newCost = costSoFar[current] + movementCost;

                if (costSoFar.ContainsKey(neighbor) && newCost >= costSoFar[neighbor])
                {
                    continue;
                }

                costSoFar[neighbor] = newCost;
                frontier.Add((neighbor, newCost));
                cameFrom[neighbor] = current;
            }
        }

        buttonHelper.LogWarning($"[{System.DateTime.Now}] No path found with Dijkstra");
        isSearching = false;
    }

    private IEnumerator AStar()
    {
        var (startPos, endPos) = FindStartAndEndTiles();

        List<(Vector2Int pos, float priority)> frontier = new() { (startPos, 0) };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new() { [startPos] = startPos };
        Dictionary<Vector2Int, float> costSoFar = new() { [startPos] = 0 };

        while (frontier.Count > 0)
        {
             frontier.Sort((a, b) => a.priority.CompareTo(b.priority));
            Vector2Int current = frontier[0].pos;
            frontier.RemoveAt(0);

            VisualizeVisited(current);
            yield return new WaitForSeconds(visualizationDelay);

            if (current == endPos)
            {
                yield return StartCoroutine(VisualizePath(ReconstructPath(cameFrom, startPos, endPos)));
                isSearching = false;
                yield break;
            }

            foreach (Vector2Int neighbor in gridManager.GetValidNeighbors(current, Tile.TileType.Path))
            {
                float movementCost = GetMovementCost(current, neighbor);
                float newCost = costSoFar[current] + movementCost;

                if (costSoFar.ContainsKey(neighbor) && newCost >= costSoFar[neighbor])
                {
                    continue;
                }

                costSoFar[neighbor] = newCost;
                float priority = newCost + Heuristic(neighbor, endPos);

                frontier.RemoveAll(x => x.pos == neighbor);
                frontier.Add((neighbor, priority));
                cameFrom[neighbor] = current;
            }
        }

        buttonHelper.LogWarning($"[{System.DateTime.Now}] No path found with A*");
        isSearching = false;
    }

    private float GetMovementCost(Vector2Int from, Vector2Int to)
    {
        //could have tiles with different movement costs
        return 1f;
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        //manhattan distance
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private (Vector2Int startPos, Vector2Int endPos) FindStartAndEndTiles()
    {
        Vector2Int startPos = Vector2Int.zero;
        Vector2Int endPos = Vector2Int.zero;

        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                Tile tile = gridManager.GetTileAt(new Vector2Int(x, y));
                if (tile != null)
                {
                    if (tile.type == Tile.TileType.Start)
                    {
                        startPos = new Vector2Int(x, y);
                    }
                    else if (tile.type == Tile.TileType.End)
                    {
                        endPos = new Vector2Int(x, y);
                    }
                }
            }
        }
        return (startPos, endPos);
    }


    private IEnumerator VisualizePath(List<Vector2Int> path)
    {
        pathTiles.Clear();
        foreach (var pos in path)
        {
            Tile tile = gridManager.GetTileAt(pos);
            if (tile != null && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
            {
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = pathColor;
                    pathTiles.Add(tile);
                    yield return new WaitForSeconds(visualizationDelay * 0.5f);
                }
            }
        }
    }

    private void VisualizeVisited(Vector2Int pos)
    {
        Tile tile = gridManager.GetTileAt(pos);
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

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new();
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

    private void ClearPreviousPath()
    {
        bool typeChanged = false;

        foreach (var tile in visitedTiles)
        {
            if (tile != null && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
            {
                tile.ChangeTileType(Tile.TileType.Path);
                typeChanged = true;
            }
        }
        visitedTiles.Clear();

        foreach (var tile in pathTiles)
        {
            if (tile != null && tile.type != Tile.TileType.Start && tile.type != Tile.TileType.End)
            {
                tile.ChangeTileType(Tile.TileType.Path);
                typeChanged = true;
            }
        }
        pathTiles.Clear();
        if (typeChanged)
        {
            buttonHelper.LogWarning($"[{System.DateTime.Now}] Previous path cleaned");
        }
    }

    public void CancelPathfinding()
    {
        ClearPreviousPath();
        if (isSearching)
        {
            StopAllCoroutines();
            isSearching = false;
            buttonHelper.LogWarning($"[{System.DateTime.Now}] Pathfinding cancelled");
        }
    }
}