using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SL.Lib;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System;

public class MazeManager : MonoBehaviour
{
    [SerializeField] private int mazeWidth = 20;
    [SerializeField] private int mazeHeight = 20;
    [SerializeField] private int minRouteArea = 20;
    [SerializeField] private float mapScale = 1f;
    [SerializeField] private bool useDebugStartGoal = false;
    [SerializeField] private List<Tilemap> tilemaps;
    [SerializeField] private List<TileBase> tileList; // 0: 通路, 1: 壁

    private Tensor<int> baseMap;
    private Tensor<float> lightTensor;
    public (int rows, int cols) mazeSize { get;private set; }
    private Tensor<int> gimmickMap;
    private Dictionary<int, (List<Indice[]>, List<Indice[]>)> startAndGoalPoints;
    private TensorLabel tensorLabel;
    private Vector2Int centerPosition;
    private Vector2 startPosition;
    private Vector2 goalPosition;
    private Vector2Int offset;

    public int firstEnemies {  get; private set; }

    public Vector2 StartPosition => startPosition;
    public Vector2 GoalPosition => goalPosition;

    public (int i,int j) GoalTensorPosition { get; private set; }

    private int currentRegion;

    public delegate void MazeGenerationCompleteHandler();
    public event MazeGenerationCompleteHandler OnMazeGenerationComplete;

    private IEnumerator GenerateBaseMaze()
    {
        (baseMap, tensorLabel) = MazeCreater.CreatePlainMaze(mazeWidth, mazeHeight, minRouteArea);
        gimmickMap = Tensor<int>.Zeros(mazeWidth, mazeHeight);
        yield return SetMazeTile(baseMap, Vector2Int.zero);
    }

    private IEnumerator SetMazeTile(Tensor<int> maze, Vector2Int centerPosition)
    {
        this.centerPosition = centerPosition;
        if (!maze.Is2D)
        {
            Debug.LogError("Invalid maze data. Must be a 2D Tensor.");
            yield break;
        }

        mazeSize = (maze.Shape[0], maze.Shape[1]);

        tilemaps[1].GetComponent<TilemapCollider2D>().maximumTileChangeCount = (uint)maze.Clip(0,1).Sum();

        // マップの中心を計算
        Vector2Int mapCenter = new Vector2Int(mazeSize.cols / 2, mazeSize.rows / 2);
        offset = centerPosition - mapCenter;

        for (int y = 0; y < mazeSize.rows; y++)
        {
            for (int x = 0; x < mazeSize.cols; x++)
            {
                int tileIndex = maze[y, x];
                if (tileIndex >= 0 && tileIndex < tileList.Count)
                {
                    // y座標を反転させてタイルを配置
                    Vector3Int tilePosition = new Vector3Int(x + offset.x, (mazeSize.rows - 1 - y) + offset.y, 0);
                    tilemaps[tileIndex].SetTile(tilePosition, tileList[tileIndex]);
                }
                else
                {
                    Debug.LogWarning($"Invalid tile index at ({x}, {y}): {tileIndex}");
                }
            }
        }
        SetFrame();
        yield return new WaitForEndOfFrame();
        tilemaps[1].GetComponent<CompositeCollider2D>().GenerateGeometry();
    }
    private void SetFrame()
    {
        // 上下の枠を設置
        for (int x = -1; x <= mazeSize.Item2; x++)
        {
            Vector3Int topPosition = new Vector3Int(x + offset.x, mazeSize.Item1 + offset.y, 0);
            Vector3Int bottomPosition = new Vector3Int(x + offset.x, -1 + offset.y, 0);
            tilemaps[1].SetTile(topPosition, tileList[1]);
            tilemaps[1].SetTile(bottomPosition, tileList[1]);
        }

        // 左右の枠を設置
        for (int y = 0; y < mazeSize.Item1; y++)
        {
            Vector3Int leftPosition = new Vector3Int(-1 + offset.x, y + offset.y, 0);
            Vector3Int rightPosition = new Vector3Int(mazeSize.Item2 + offset.x, y + offset.y, 0);
            tilemaps[1].SetTile(leftPosition, tileList[1]);
            tilemaps[1].SetTile(rightPosition, tileList[1]);
        }
    }
    public IEnumerator GenerateMazeAsync()
    {
        yield return GenerateBaseMaze();
        if (useDebugStartGoal)
        {
            SetDebugStartAndGoal();
            yield return null;
        }
        else
        {
            yield return StartCoroutine(CalculateStartAndGoalPointsAsync());
        }
        SetRandomStartAndGoal();
        OnMazeGenerationComplete?.Invoke();
    }

    private void SetDebugStartAndGoal()
    {
        startAndGoalPoints = new();
        foreach (var label in tensorLabel.RouteLabels) 
        {
            LongestPathFinder LPF = new(baseMap == 0, (tensorLabel.Label == label));
            var result = LPF.FindLongestShortestPaths();
            var starts = result.Select(r => new Indice[] {r.Item1.Item1, r.Item1.Item2});
            var goals = result.Select(r => new Indice[] { r.Item2.Item1, r.Item2.Item2 });
            startAndGoalPoints[label] = (starts.ToList(), goals.ToList());
        }
    }

    private IEnumerator CalculateStartAndGoalPointsAsync()
    {
        bool calculationComplete = false;
        Task.Run(() =>
        {
            startAndGoalPoints = MazeArrayHelper.GetStartAndGoal(baseMap, tensorLabel);
            calculationComplete = true;
        });

        while (!calculationComplete)
        {
            yield return null;
        }
    }

    private void SetRandomStartAndGoal()
    {
        int region = SLRandom.SelectRandom(startAndGoalPoints.Keys.ToList());
        currentRegion = region;
        firstEnemies = Mathf.Max(1,tensorLabel.GetAreaSize(currentRegion)/12);
        var startCandidates = startAndGoalPoints[region].Item1;
        var goalCandidates = startAndGoalPoints[region].Item2;

        var start = startCandidates[SLRandom.Random.Next(0, startCandidates.Count)];
        var goal = goalCandidates[SLRandom.Random.Next(0, goalCandidates.Count)];

        startPosition = GetWorldPosition(start);
        goalPosition = GetWorldPosition(goal);
        GoalTensorPosition = GetTensorPosition(goal);

    }
    private (int i, int j) GetTensorPosition(Indice[] indices)
    {
        var positions = baseMap.GetRealIndices(indices);
        var i = SLRandom.SelectRandom(positions[0]);
        var j = SLRandom.SelectRandom(positions[1]);
        return (i, j);
    }
    public (int i, int j) GetTensorPosition(Vector2 worldPosition)
    {
        return (-(int)(worldPosition.y - offset.y - mazeSize.rows), (int)(worldPosition.x - offset.x));
    }

    public Vector3Int GetTilePosition((int i, int j) tensorPosition)
    {
        return new Vector3Int(tensorPosition.j + offset.x, (mazeSize.rows - 1 - tensorPosition.i) + offset.y, 0);
    }

    public Vector3Int GetTilePosition(Vector2 worldPosition)
    {
        return GetTilePosition(GetTensorPosition(worldPosition));
    }

    public int GetMazeData(Vector2 worldPosition)
    {
        var indice = GetTensorPosition(worldPosition);
        return baseMap[indice.i, indice.j];
    }

    public (Vector3 worldPosition, int data) GetTileData((int i, int j) indices)
    {
        return (GetTilePosition(indices), baseMap[indices.i, indices.j]);
    }

    public void BoundRange(int si, int irange,int sj, int jrange, Action<Vector3,int> action)
    {
        int imax = Mathf.Min(si + irange, mazeSize.rows+1);
        int jmax = Mathf.Min(sj + jrange, mazeSize.cols+1);
        int istart = Mathf.Max(si, -1);
        int jstart = Mathf.Max(sj, -1);
        var halfOffset = Vector3.one * 0.5f;
        halfOffset.z = 0f;
        for (int i = istart; i < imax; i++)
        {
            for (int j = jstart; j < jmax; j++)
            {
                if (i < 0 || j < 0 || i >= mazeSize.rows || j >= mazeSize.cols)
                {
                    action.Invoke(GetTilePosition((i, j)) + halfOffset, 1);
                }
                else
                {
                    action.Invoke(GetTilePosition((i, j)) + halfOffset, baseMap[i, j]);
                }
            }
        }
    }

    public Vector2 GetWorldPosition(Indice[] indices)
    {
        return GetWorldPosition(GetTensorPosition(indices));
    }

    public Vector2 GetWorldPosition(int i, int j)
    {
        return new Vector2(j + offset.x + 0.5f, (mazeSize.rows - 1 - i) + offset.y + 0.5f);
    }
    public Vector2 GetWorldPosition((int i, int j) index) => GetWorldPosition(index.i, index.j);

    public List<Vector2> GetRandomPositions(int num)
    {
        var predIndices = (tensorLabel.Label == currentRegion).ArgWhere();
        var indices = SLRandom.Choices(predIndices, Mathf.Min(num, predIndices.Count));
        return indices.Select(p => GetWorldPosition(p)).ToList();
    }
    public Vector2 GetRandomPosition()
    {
        return GetWorldPosition(SLRandom.Choice((tensorLabel.Label == currentRegion).ArgWhere()));
    }

    public void SetGimmick(Vector2Int position, int gimmickId)
    {
    }

    public int GetGimmick(Vector2Int position)
    {
        return 0;
    }

    public Tensor<int> GetBaseMap()
    {
        return new Tensor<int>(baseMap);
    }

    public Tensor<int> GetGimmickMap()
    {
        return new Tensor<int>(gimmickMap);
    }

    // デバッグ用のメソッド
    public void PrintMazeToConsole()
    {
        string mazeString = "";
        for (int y = 0; y < mazeHeight; y++)
        {
            for (int x = 0; x < mazeWidth; x++)
            {
                if (new Vector2Int(x, y) == startPosition)
                    mazeString += "S";
                else if (new Vector2Int(x, y) == goalPosition)
                    mazeString += "G";
                else if (baseMap[y, x] == 1)
                    mazeString += "*";
                else
                    mazeString += " ";
            }
            mazeString += "\n";
        }
        Debug.Log(mazeString);
    }
}