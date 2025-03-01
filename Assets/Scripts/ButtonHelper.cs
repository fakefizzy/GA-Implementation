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

    void Start()
    {
        gridManager = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();

        if (sizeInput != null)
        {
            sizeInput.text = gridManager.size.ToString();

            sizeInput.onEndEdit.AddListener(ValidateAndSetSize);
        }
    }

    public void ResetGrid()
    {
        gridManager.SetupCamera();
        gridManager.GenerateGrid();
    }
    public void GenerateMaze()
    {
        gridManager.SetupCamera();
        gridManager.GenerateGrid();
        gridManager.GenerateMaze();
    }

    public void ValidateAndSetSize(string inputText)
    {
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
}
