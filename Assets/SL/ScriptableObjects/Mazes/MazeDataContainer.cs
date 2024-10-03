using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SL.Lib;

[CreateAssetMenu(fileName = "MazeData", menuName = "Maze/CreateMazeData")]
public class MazeDataContainer : ScriptableObject
{
    [SerializeField] private MazeData mazeData;
    [SerializeField] private int minRouteAreaSize;

    [ContextMenu("GenerateBaseMaze")]
    public void GenerateBaseMaze()
    {
        mazeData.GenerateBaseMaze(minRouteAreaSize);
    }
    [ContextMenu("UpdateMazeData")]
    public void UpdateMazeData()
    {
        mazeData.UpdateMazeData();
    }
}