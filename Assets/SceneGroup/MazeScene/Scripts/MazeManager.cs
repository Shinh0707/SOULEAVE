using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SL.Lib;
using System.Threading.Tasks;
using System.Linq;

public class MazeManager : MonoBehaviour
{
    [SerializeField] private int mazeWidth = 20;
    [SerializeField] private int mazeHeight = 20;
    [SerializeField] private int minRouteArea = 20;
    [SerializeField] private float mapScale = 1f;
    [SerializeField] private bool useDebugStartGoal = false;

    private Tensor<int> baseMap;
    private Tensor<int> gimmickMap;
    private Dictionary<int, (List<Indice[]>, List<Indice[]>)> startAndGoalPoints;
    private TensorLabel tensorLabel;
    private Vector2Int startPosition;
    private Vector2Int goalPosition;

    public int firstEnemies {  get; private set; }

    public Vector2Int StartPosition => startPosition;
    public Vector2Int GoalPosition => goalPosition;

    private int currentRegion;

    public delegate void MazeGenerationCompleteHandler();
    public event MazeGenerationCompleteHandler OnMazeGenerationComplete;

    private void GenerateBaseMaze()
    {
        (baseMap, tensorLabel) = MazeCreater.CreatePlainMaze(mazeWidth, mazeHeight, minRouteArea);
        gimmickMap = Tensor<int>.Zeros(mazeWidth, mazeHeight);
    }

    public IEnumerator GenerateMazeAsync()
    {
        GenerateBaseMaze();
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
        int region = Random.Range(0, startAndGoalPoints.Count);
        currentRegion = region;
        firstEnemies = Mathf.Max(1,tensorLabel.GetAreaSize(currentRegion)/12);
        var startCandidates = startAndGoalPoints[region].Item1;
        var goalCandidates = startAndGoalPoints[region].Item2;

        var start = startCandidates[Random.Range(0, startCandidates.Count)];
        var goal = goalCandidates[Random.Range(0, goalCandidates.Count)];

        startPosition = IndiceToPosition(start);
        goalPosition = IndiceToPosition(goal);
    }
    private Vector2Int IndiceToPosition(Indice[] indices)
    {
        var positions = baseMap.GetRealIndices(indices);
        var position = SLRandom.SelectRandom(positions);
        return new Vector2Int(position[1], position[0]);
    }

    public List<Vector2Int> GetPositions(int num)
    {
        var positions = SLRandom.Sample((tensorLabel.Label == currentRegion).ArgWhere(), num);
        return positions.Select(p => IndiceToPosition(p)).ToList();
    }
    public Vector2Int GetPositions()
    {
        return IndiceToPosition(SLRandom.Choice((tensorLabel.Label == currentRegion).ArgWhere()));
    }

    public bool IsWall(Vector2Int position)
    {
        return baseMap[position.y, position.x] == 1;
    }

    public bool IsInGoal(Vector2Int position) => Vector2Int.Distance(position, goalPosition) < 0.5f;

    public bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < mazeWidth && position.y >= 0 && position.y < mazeHeight;
    }

    public bool IsValidMove(Vector2Int position)
    {
        return IsInBounds(position) && !IsWall(position);
    }

    public Vector2 GetWorldPosition(Vector2Int mazePosition)
    {
        return new Vector2(mazePosition.x * mapScale, mazePosition.y * mapScale);
    }

    public Vector2Int GetMazePosition(Vector2 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / mapScale),
            Mathf.FloorToInt(worldPosition.y / mapScale)
        );
    }

    public void SetGimmick(Vector2Int position, int gimmickId)
    {
        if (IsInBounds(position))
        {
            gimmickMap[position.y, position.x] = gimmickId;
        }
    }

    public int GetGimmick(Vector2Int position)
    {
        if (IsInBounds(position))
        {
            return gimmickMap[position.y, position.x];
        }
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