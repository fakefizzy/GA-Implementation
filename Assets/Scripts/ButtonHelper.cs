using UnityEngine;
using UnityEngine.UI;

public class ButtonHelper : MonoBehaviour
{
    private GridManager gridManager;
    public TMPro.TMP_InputField sizeInput;
    public Slider visualizationSlider;
    private PathFinder pathFinder;
    public TMPro.TMP_Text logField;

    void Start()
    {
        pathFinder = FindObjectOfType<PathFinder>();
        gridManager = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();

        if (sizeInput != null)
        {
            sizeInput.text = gridManager.size.ToString();

            sizeInput.onEndEdit.AddListener(ValidateAndSetSize);
        }
        visualizationSlider.onValueChanged.AddListener(ChangeVisualizationSpeed);
    }

    public void ResetGrid()
    {
        CancelAlgorithm();
        gridManager.SetupCamera();
        LogWarning($"[{System.DateTime.Now}] Grid reset");
        gridManager.GenerateGrid();
    }
    public void GenerateMaze()
    {
        CancelAlgorithm();
        gridManager.SetupCamera();
        gridManager.GenerateGrid();
        LogWarning($"[{System.DateTime.Now}] Maze generated");
        gridManager.GenerateMaze();
    }

    public void ChangeVisualizationSpeed(float sliderValue)
    {
        LogWarning($"[{System.DateTime.Now}] New visualization speed {sliderValue} set");
        pathFinder.SetVisualizationSpeed(sliderValue);
    }

    public void ValidateAndSetSize(string inputText)
    {
        CancelAlgorithm();

        if (!int.TryParse(inputText, out int newSize))
        {
            sizeInput.text = gridManager.size.ToString();
            return;
        }

        if (newSize < 5)
        {
            newSize = 5;
        }
        else if (newSize > 201)
        {
            newSize = 201;
        }

        if (newSize % 2 == 0)
        {
            newSize++;
        }

        if (gridManager != null)
        {
            gridManager.size = newSize;
            gridManager.width = newSize;
            gridManager.height = newSize;
            gridManager.SetupCamera();
            gridManager.GenerateGrid();
            LogWarning($"[{System.DateTime.Now}] Grid size changed to {newSize}");
        }

        sizeInput.text = gridManager.size.ToString();
    }

    public void ClearGrid()
    {
        CancelAlgorithm();
        LogWarning($"[{System.DateTime.Now}] Grid cleared");
        gridManager.MakeAllTilesPath();
    }

    public void CancelAlgorithm() 
    { 
        pathFinder.CancelPathfinding();
    }

    public void StartBFS()
    {
        CancelAlgorithm();
        LogWarning($"[{System.DateTime.Now}] BFS started");
        pathFinder.StartPathfinding(PathFinder.Algorithm.BFS);
    }

    public void StartDFS()
    {
        CancelAlgorithm();
        LogWarning($"[{System.DateTime.Now}] DFS started");
        pathFinder.StartPathfinding(PathFinder.Algorithm.DFS);
    }

    public void StartDijkstra()
    {
        CancelAlgorithm();
        LogWarning($"[{System.DateTime.Now}] Dijkstra started");
        pathFinder.StartPathfinding(PathFinder.Algorithm.Dijkstra);
    }

    public void StartAStar()
    {
        CancelAlgorithm();
        LogWarning($"[{System.DateTime.Now}] A* started");
        pathFinder.StartPathfinding(PathFinder.Algorithm.AStar);
    }


    public void LogWarning(string warning)
    {
        logField.text = warning + "\n"+ logField.text;
    }

}
