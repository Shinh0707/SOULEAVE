using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using SL.Lib;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField, Range(5, 30)] private int mazeWidth;
    [SerializeField, Range(5, 30)] private int mazeHeight;
    [SerializeField, Range(5, 200)] private int minRouteArea;
    [SerializeField] private Vector2Int playerStartPosition;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileBase> tileList; // 0: �ʘH, 1: ��

    private Tensor<int> mazeData;

    private void Start()
    {
        // �e�X�g�p��Tensor�f�[�^ (��ŊO������󂯎��悤�ɕύX�\��)
        (mazeData, _) = MazeCreater.CreatePlainMaze(mazeWidth, mazeHeight, minRouteArea);

        GenerateMaze(mazeData, playerStartPosition);
    }

    public void GenerateMaze(Tensor<int> maze, Vector2Int centerPosition)
    {
        if (!maze.Is2D)
        {
            Debug.LogError("Invalid maze data. Must be a 2D Tensor.");
            return;
        }

        int rows = maze.Shape[0];
        int cols = maze.Shape[1];

        // �}�b�v�̒��S���v�Z
        Vector2Int mapCenter = new Vector2Int(cols / 2, rows / 2);
        Vector2Int offset = centerPosition - mapCenter;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int tileIndex = maze[y, x];
                if (tileIndex >= 0 && tileIndex < tileList.Count)
                {
                    // y���W�𔽓]�����ă^�C����z�u
                    Vector3Int tilePosition = new Vector3Int(x + offset.x, (rows - 1 - y) + offset.y, 0);
                    tilemap.SetTile(tilePosition, tileList[tileIndex]);
                }
                else
                {
                    Debug.LogWarning($"Invalid tile index at ({x}, {y}): {tileIndex}");
                }
            }
        }
    }

    // �����I�ɊO������Tensor���󂯎�郁�\�b�h
    public void SetMazeData(Tensor<int> newMazeData)
    {
        mazeData = newMazeData;
        GenerateMaze(mazeData, playerStartPosition);
    }
}