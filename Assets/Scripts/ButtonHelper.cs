using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ButtonHelper : MonoBehaviour
{
    private GridManager gridManager;
    public TMPro.TMP_InputField sizeInput;
    public Slider visualizationSlider;
    private PathFinder pathFinder;

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
        gridManager.GenerateGrid();
    }
    public void GenerateMaze()
    {
        CancelAlgorithm();
        gridManager.SetupCamera();
        gridManager.GenerateGrid();
        gridManager.GenerateMaze();
    }

    public void ChangeVisualizationSpeed(float sliderValue)
    {
        pathFinder.SetVisualizationSpeed(sliderValue);
    }

    public void ValidateAndSetSize(string inputText)
    {
        CancelAlgorithm();
        int newSize;

        if (!int.TryParse(inputText, out newSize))
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
        }

        sizeInput.text = gridManager.size.ToString();
    }

    public void ClearGrid()
    {
        CancelAlgorithm();
        gridManager.MakeAllTilesPath();
    }

    public void CancelAlgorithm() 
    { 
        pathFinder.CancelPathfinding();
    }

    public void StartBFS()
    {
        CancelAlgorithm();
        pathFinder.StartPathfinding(PathFinder.Algorithm.BFS);
    }

    public void StartDFS()
    {
        CancelAlgorithm();
        pathFinder.StartPathfinding(PathFinder.Algorithm.DFS);
    }

    public void StartDijkstra()
    {
        CancelAlgorithm();
        pathFinder.StartPathfinding(PathFinder.Algorithm.Dijkstra);
    }

    public void StartAStar()
    {
        CancelAlgorithm();
        pathFinder.StartPathfinding(PathFinder.Algorithm.AStar);
    }

}
