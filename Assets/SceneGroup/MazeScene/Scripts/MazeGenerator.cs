using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using SL.Lib;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileBase> tileList; // 0: 通路, 1: 壁
    [SerializeField] private Vector2Int playerStartPosition;

    private Tensor<int> mazeData;

    private void Start()
    {
        // テスト用のTensorデータ (後で外部から受け取るように変更予定)
        (mazeData, _) = MazeCreater.CreatePlainMaze(10, 10, 5);

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

        // マップの中心を計算
        Vector2Int mapCenter = new Vector2Int(cols / 2, rows / 2);
        Vector2Int offset = centerPosition - mapCenter;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int tileIndex = maze[y, x].item;
                if (tileIndex >= 0 && tileIndex < tileList.Count)
                {
                    // y座標を反転させてタイルを配置
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

    // 将来的に外部からTensorを受け取るメソッド
    public void SetMazeData(Tensor<int> newMazeData)
    {
        mazeData = newMazeData;
        GenerateMaze(mazeData, playerStartPosition);
    }
}