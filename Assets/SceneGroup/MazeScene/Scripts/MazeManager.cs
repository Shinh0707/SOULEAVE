using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SL.Lib;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MazeManager : MonoBehaviour
{
    [SerializeField] private int mazeWidth = 20;
    [SerializeField] private int mazeHeight = 20;
    [SerializeField] private int minRouteArea = 20;
    [SerializeField] private float mapScale = 1f;
    [SerializeField] private bool useDebugStartGoal = false;
    [SerializeField] private List<Tilemap> tilemaps;
    [SerializeField] private Tilemap visibilityTilemap;
    [SerializeField] private List<TileBase> tileList; // 0: 通路, 1: 壁
    [SerializeField] private TileBase visibilityTile;

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

    public void UpdateVisibility(Vector2 playerPos, float intensity, Color baseColor) => SetMazeVisibilityTile(GetTensorPosition(playerPos), intensity, baseColor);
    private void SetMazeVisibilityTile((int,int) playerPos, float intensity, Color baseColor)
    {
        // マップの中心を計算
        Vector2Int mapCenter = new Vector2Int(mazeSize.cols / 2, mazeSize.rows / 2);
        offset = centerPosition - mapCenter;
        SimulateLightPropagation(playerPos, intensity);

        for (int y = 0; y < mazeSize.rows; y++)
        {
            for (int x = 0; x < mazeSize.cols; x++)
            {
                Vector3Int tilePosition = new Vector3Int(x + offset.x, (mazeSize.rows - 1 - y) + offset.y, 0);
                if (lightTensor[y, x] > 0 && baseMap[y, x] == 0)
                {
                    Color tileColor = Color.Lerp(Color.black, baseColor, lightTensor[y, x]);
                    visibilityTilemap.SetTile(tilePosition, visibilityTile);
                    visibilityTilemap.SetTileFlags(tilePosition, TileFlags.None);
                    visibilityTilemap.SetColor(tilePosition, tileColor);
                }
                else
                {
                    visibilityTilemap.SetTile(tilePosition, null);
                }
            }
        }
    }

    private void SimulateLightPropagation((int,int) indices, float intensity)
    {
        lightTensor = Tensor<float>.Zeros(baseMap);
        lightTensor[indices.Item1,indices.Item2] = Mathf.Max(0f, intensity);
        Tensor<bool> mazeMask = baseMap == 0;
        Tensor<float> mazeFloatMask = mazeMask.Cast<float>();
        mazeMask = !mazeMask;
        Tensor<float> propKernel = Tensor<float>.FromArray(new[,]
        {
            {0f,0.5f,0f},
            {0.5f,1f,0.5f},
            {0f,0.5f,0f},
        });
        propKernel /= propKernel.Sum();

        int numIterations = Mathf.CeilToInt(Mathf.Sqrt(Mathf.Abs(intensity)));

        for (int i = 0; i < numIterations; i++) {
            lightTensor = (lightTensor * mazeFloatMask).Convolve(propKernel, ConvolveMode.SAME);
            lightTensor[indices.Item1,indices.Item2] = Mathf.Max(0f, intensity);
            lightTensor[mazeMask] = 0f;
        }

        lightTensor = lightTensor.Clip(0f, 1f);
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
            var sgArea = (tensorLabel.Label == label).ArgWhere();
            startAndGoalPoints[label] = (sgArea, sgArea);
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

        var start = startCandidates[Random.Range(0, startCandidates.Count)];
        var goal = goalCandidates[Random.Range(0, goalCandidates.Count)];

        startPosition = GetWorldPosition(start);
        goalPosition = GetWorldPosition(goal);
    }
    private (int i, int j) GetTensorPosition(Indice[] indices)
    {
        var positions = baseMap.GetRealIndices(indices);
        var i = SLRandom.SelectRandom(positions[0]);
        var j = SLRandom.SelectRandom(positions[1]);
        return (i, j);
    }
    public (int i, int j) GetTensorPosition(Vector2 position)
    {
        return (-(int)(position.y - offset.y - mazeSize.rows), (int)(position.x - offset.x));
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

    public List<Vector2> GetPositions(int num)
    {
        var predIndices = (tensorLabel.Label == currentRegion).ArgWhere();
        var indices = SLRandom.Choices(predIndices, Mathf.Min(num, predIndices.Count));
        return indices.Select(p => GetWorldPosition(p)).ToList();
    }
    public Vector2 GetPosition()
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