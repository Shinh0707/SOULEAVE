using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SL.Lib;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.Tilemaps;
using System;

public class MazeManager : MonoBehaviour
{
    [SerializeField] private bool useDebugStartGoal = false;
    [SerializeField] private List<Tilemap> tilemaps;
    private MazeData _mazeData;
    private Tensor<float> _mazeIntensity;
    public MazeData MazeData => _mazeData;
    public (int rows, int cols) mazeSize => _mazeData.mazeSize;
    private Vector2 startPosition;
    private Vector2 goalPosition;
    private Vector2Int offset;

    public int firstEnemies {  get; private set; }

    public Vector2 StartPosition => startPosition;
    public Vector2 GoalPosition => goalPosition;

    public (int i, int j) GoalTensorPosition;
    public (int i, int j) StartTensorPosition;

    private int currentRegion;

    public int CurrentRegion => currentRegion;

    public delegate void MazeGenerationCompleteHandler();
    public event MazeGenerationCompleteHandler OnMazeGenerationComplete;

    public void Initialize(MazeData mazeData)
    {
        _mazeData = mazeData;
        _mazeIntensity = Tensor<float>.Zeros(_mazeData.BaseMap);
    }
    public void Initialize()
    {
        _mazeData = DefaultMazeManagerData.Instance.Create();
        _mazeIntensity = Tensor<float>.Zeros(_mazeData.BaseMap);
    }

    private IEnumerator SetMazeTile(Tensor<int> maze, Vector2Int centerPosition)
    {
        if (!maze.Is2D)
        {
            Debug.LogError("Invalid maze data. Must be a 2D Tensor.");
            yield break;
        }

        tilemaps[1].GetComponent<TilemapCollider2D>().maximumTileChangeCount = (uint)maze.Clip(0,1).Sum();

        // マップの中心を計算
        Vector2Int mapCenter = new(mazeSize.cols / 2, mazeSize.rows / 2);
        offset = centerPosition - mapCenter;

        for (int y = 0; y < mazeSize.rows; y++)
        {
            for (int x = 0; x < mazeSize.cols; x++)
            {
                int tileIndex = maze[y, x];
                if (tileIndex >= 0 && tileIndex < _mazeData.tileList.Count)
                {
                    // y座標を反転させてタイルを配置
                    Vector3Int tilePosition = new Vector3Int(x + offset.x, (mazeSize.rows - 1 - y) + offset.y, 0);
                    tilemaps[tileIndex].SetTile(tilePosition, _mazeData.tileList[tileIndex]);
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
        foreach(var tilemap in tilemaps)
        {
            if(tilemap.TryGetComponent(out TilemapShadowCaster tilemapShadowCaster))
            {
                StartCoroutine(tilemapShadowCaster.ApplyShadowCasters());
            }
        }
    }
    private void SetFrame()
    {
        // 上下の枠を設置
        for (int x = -1; x <= mazeSize.cols; x++)
        {
            Vector3Int topPosition = new Vector3Int(x + offset.x, mazeSize.rows + offset.y, 0);
            Vector3Int bottomPosition = new Vector3Int(x + offset.x, -1 + offset.y, 0);
            tilemaps[1].SetTile(topPosition, _mazeData.tileList[1]);
            tilemaps[1].SetTile(bottomPosition, _mazeData.tileList[1]);
        }

        // 左右の枠を設置
        for (int y = 0; y < mazeSize.rows; y++)
        {
            Vector3Int leftPosition = new Vector3Int(-1 + offset.x, y + offset.y, 0);
            Vector3Int rightPosition = new Vector3Int(mazeSize.cols + offset.x, y + offset.y, 0);
            tilemaps[1].SetTile(leftPosition, _mazeData.tileList[1]);
            tilemaps[1].SetTile(rightPosition, _mazeData.tileList[1]);
        }
    }
    public IEnumerator GenerateMazeAsync()
    {
        yield return SetMazeTile(_mazeData.GetBaseMap(), Vector2Int.zero);
        if (useDebugStartGoal)
        {
            yield return null;
        }
        else
        {
            yield return StartCoroutine(CalculateStartAndGoalPointsAsync());
        }
        SetRandomStartAndGoal();
        OnMazeGenerationComplete?.Invoke();
    }

    private IEnumerator CalculateStartAndGoalPointsAsync()
    {
        yield break;
    }

    private void SetRandomStartAndGoal()
    {
        _mazeData.GetRandomStartAndGoal(out currentRegion, out StartTensorPosition, out GoalTensorPosition);
        firstEnemies = Mathf.Max(1, (_mazeData.GetRegionSize(currentRegion)-2) / 12);
        startPosition = GetWorldPosition(StartTensorPosition);
        goalPosition = GetWorldPosition(GoalTensorPosition);
    }

    public EnemySpawnData[] GetEnemySpawnData()
    {
        EnemySpawnData[] enemySpawnDatas = new EnemySpawnData[firstEnemies];
        MazeData.GetEnemySpawnData(enemySpawnDatas, currentRegion, StartTensorPosition, GoalTensorPosition);
        for(int i = 0;i < firstEnemies;i++)
        {
            enemySpawnDatas[i].Position = GetWorldPosition(enemySpawnDatas[i].TensorIndex);
        }
        return enemySpawnDatas;
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
    public void ResetIntensityMap()
    {
        _mazeIntensity = Tensor<float>.Zeros(_mazeIntensity);
    }

    public static (int di, int dj)[] NeighborDirections = new (int, int)[] { (1, 0), (0, 1), (-1, 0), (0, -1), (1, 1), (-1, 1), (-1, -1), (1, -1) }; // (i % 4)*2 + i/4 , i != 0 => (4-i%4)*2 - i/4
    public static int DirecClockwise(int i) => i == 0 ? 0 : ((4 - i % 4) * 2 - i / 4);
    public static int DirecCounterClockwise(int i) => (i % 4) * 2 + i / 4;

    public List<int> ValidDirections(Vector2 worldPosition)
    {
        List<int> valids = new();
        (int i, int j) = GetTensorPosition(worldPosition);
        (int rows, int cols) = _mazeData.mazeSize;
        var baseMap = _mazeData.BaseMap;
        int di, dj;
        for (int n = 0; n < 8; n++)
        {
            (di, dj) = NeighborDirections[n];
            if (IsValidTensorDirection(i, j, di, dj, rows, cols, baseMap, out _, out _))
            {
                valids.Add(n);
            }
        }
        return valids;
    }

    public Vector2 SelectRandomValidDirection(Vector2 worldPosition)
    {
        var vdirec = ValidDirections(worldPosition);
        var select = SLRandom.Random.NextDouble() * vdirec.Count;
        var index = (int)select;
        var rate = select - index;
        return VectorExtensions.RadToVector2((float)(0.125 * (vdirec[index] + rate)- 0.0625) * Mathf.PI);
    }

    public static float[] NeighborDirectionDists = new float[] { 1,  Mathf.Sqrt(2) };
    public static ((int di, int dj), float) GetDirectionData(int n) => (NeighborDirections[n], NeighborDirectionDists[n / 4]);
    public bool IsValidTensorIndex(int i, int j, int rows, int cols, Tensor<bool> baseMap) => 0 <= i && i < rows && 0 <= j && j < cols && baseMap[i, j];

    public bool IsValidTensorDirection(int i, int j, int di, int dj, int rows, int cols, Tensor<bool> baseMap, out int ni, out int nj)
    {
        ni = i + di;
        nj = j + dj;
        if (IsValidTensorIndex(ni, nj, rows, cols, baseMap))
        {
            if (di != 0 && dj != 0)
            {
                if (!IsValidTensorIndex(ni, j, rows, cols, baseMap) && !IsValidTensorIndex(i, nj, rows, cols, baseMap)) return false;
            }
            return true;
        }
        return false;
    }
    public void SetIntensity(Vector2 worldPosition, float value) 
    {
        HashSet<(int i, int j)> visited = new();
        var baseMap = _mazeData.BaseMap;
        Queue<((int i, int j) , float)> queue = new();
        queue.Enqueue((GetTensorPosition(worldPosition), 0));
        (int rows, int cols) = _mazeData.mazeSize;
        int i, j, n, di, dj, ni, nj;
        float currentValue, nd;
        while (queue.Count > 0)
        {
            ((i, j), currentValue) = queue.Dequeue();
            if (currentValue >= value) continue;
            if (visited.Contains((i, j))) continue;
            _mazeIntensity[i, j] = value-currentValue;
            visited.Add((i, j));
            for (n = 0; n < 8; n++)
            {
                ((di, dj), nd) = GetDirectionData(n);
                if (IsValidTensorDirection(i, j, di, dj, rows, cols, baseMap, out ni, out nj))
                {
                    queue.Enqueue(((ni, nj), currentValue + nd));
                }
            }
        }
    }

    public bool IsVisiblePosition(Vector2 worldPosition)
    {
        return GetIntensity(worldPosition) > 0.025f;
    }
    public bool IsInMaze(Vector2 worldPosition)
    {
        return IsInMaze(GetTensorPosition(worldPosition));
    }
    public bool IsInMaze((int i, int j) pos)
    {
        var size = _mazeData.mazeSize;
        return 0 <= pos.i && pos.i < size.rows && 0 <= pos.j && pos.j < size.cols;
    }
    public float GetIntensity(Vector2 worldPosition)
    {
        var pos = GetTensorPosition(worldPosition);
        if (!IsInMaze(pos)) return 0;
        return _mazeIntensity[pos.i, pos.j];
    }
    public Vector2 GetLightenDirection(Vector2 worldPosition, int maxDepth) 
    {
        var NextVolumes = Tensor<float>.Zeros(_mazeData.BaseMap);
        var CurrentVolumes = new Tensor<float>(_mazeIntensity);
        var baseMap = _mazeData.BaseMap;
        HashSet<(int i, int j)> reachable = new();
        Queue<((int i,int j), float)> queue = new();
        (int si, int sj) = GetTensorPosition(worldPosition);
        queue.Enqueue(((si,sj), 0));
        (int rows, int cols) = _mazeData.mazeSize;
        float nd;
        int di, dj, ni, nj, n;
        {
            int i, j;
            float depth;
            while (queue.Count > 0)
            {
                ((i, j), depth) = queue.Dequeue();
                if (depth > maxDepth) continue;
                if (reachable.Contains((i, j))) continue;
                reachable.Add((i, j));
                for (n = 0; n < 8; n++)
                {
                    ((di, dj), nd) = GetDirectionData(n);
                    if (IsValidTensorDirection(i, j, di, dj, rows, cols, baseMap, out ni, out nj))
                    {
                        queue.Enqueue(((ni, nj), depth + nd));
                    }
                }
            }
        }
        float maxValue;
        for(int depth = 0;depth < maxDepth; depth++)
        {
            foreach((int i,int j) in reachable)
            {
                maxValue = 0;
                for (n = 0; n < 8; n++)
                {
                    ((di, dj), nd) = GetDirectionData(n);
                    if (IsValidTensorDirection(i, j, di, dj, rows, cols, baseMap, out ni, out nj))
                    {
                        maxValue = Mathf.Max(maxValue, CurrentVolumes[ni, nj]);
                        NextVolumes[ni, nj] = _mazeIntensity[ni, nj] + maxValue;
                    }
                }
            }
            CurrentVolumes = NextVolumes;
            NextVolumes = Tensor<float>.Zeros(CurrentVolumes);
        }
        (int bdi, int bdj) = (0, 0);
        maxValue = 0;
        for (n = 0; n < 8; n++)
        {
            (di, dj) = NeighborDirections[n];
            if (IsValidTensorDirection(si, sj, di, dj, rows, cols, baseMap, out ni, out nj))
            {
                if (CurrentVolumes[ni, nj] > maxValue)
                {
                    maxValue = CurrentVolumes[ni, nj];
                    (bdi, bdj) = (di, dj);
                }
            }
        }
        return new Vector2(bdj, -bdi);
    }

    public List<Vector2> GetPath(Vector2 worldPosition, Vector2 targetPosition)
    {
        var res = GetPath(GetTensorPosition(worldPosition), GetTensorPosition(targetPosition)).Select(p => GetWorldPosition(p)).ToList();
        if (res.Count >= 2)
        {
            res.RemoveAt(0);
            res[^1] = targetPosition;
        }
        return res;
    }
    public List<(int i, int j)> GetPath((int si, int sj) start, (int ti, int tj) target)
    {
        Tensor<bool> baseMap = _mazeData.BaseMap;
        (int rows, int cols) = _mazeData.mazeSize;

        Queue<((int i, int j) index, float depth)> openSet = new();
        var closedSet = new HashSet<(int i, int j)>();
        var cameFrom = new Dictionary<(int i, int j), (int i, int j)>();
        var gScore = new Dictionary<(int i, int j), float>();
        var fScore = new Dictionary<(int i, int j), float>();

        openSet.Enqueue((start, 0));
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current.index == target)
            {
                return ReconstructPath(cameFrom, current.index);
            }

            closedSet.Add(current.index);

            for (int n = 0; n < 8; n++)
            {
                var ((di, dj), nd) = GetDirectionData(n);
                (int i, int j) neighbor = (current.index.i + di, current.index.j + dj);

                if (!IsValidTensorIndex(neighbor.i, neighbor.j, rows, cols, baseMap))
                    continue;

                if (closedSet.Contains(neighbor))
                    continue;

                if (di != 0 && dj != 0)
                {
                    if (!IsValidTensorIndex(neighbor.i, current.index.j, rows, cols, baseMap) &&
                          !IsValidTensorIndex(current.index.i, neighbor.j, rows, cols, baseMap))
                        continue;
                }

                var tentativeGScore = gScore[current.index] + nd;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current.index;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, target);

                    if (!openSet.Any(x => x.index == neighbor))
                    {
                        openSet.Enqueue((neighbor, fScore[neighbor]));
                    }
                }
            }
        }

        return new List<(int i, int j)>(); // パスが見つからない場合は空のリストを返す
    }

    private float Heuristic((int i, int j) a, (int i, int j) b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.i - b.i, 2) + Mathf.Pow(a.j - b.j, 2));
    }

    private List<(int i, int j)> ReconstructPath(Dictionary<(int i, int j), (int i, int j)> cameFrom, (int i, int j) current)
    {
        var path = new List<(int i, int j)> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
    public List<Vector2> GetMostLightPath(Vector2 worldPosition, int maxDepth)
    {
        List<Vector2> lightPath = new();
        var baseMap = _mazeData.BaseMap;
        Queue<((int i, int j), float)> queue = new();
        (int si, int sj) = GetTensorPosition(worldPosition);
        queue.Enqueue(((si, sj), 0));
        Dictionary<(int i,int j),((int pi, int pj), float nodeDepth)> nodeMap = new();
        nodeMap[(si, sj)] = ((-1,-1),0);
        (int rows, int cols) = _mazeData.mazeSize;
        List<(int mi, int mj)> maxIntensityPositions = new();
        float mv = 0;
        float nd;
        int di, dj, ni, nj, n;
        {
            int i, j;
            float depth, mpv;
            while (queue.Count > 0)
            {
                ((i, j), depth) = queue.Dequeue();
                if (depth > maxDepth) continue;
                mpv = _mazeIntensity[i, j];
                if (mpv >= mv)
                {
                    if (mpv > mv)
                    {
                        maxIntensityPositions.Clear();
                        mv = mpv;
                    }
                    maxIntensityPositions.Add((i, j));
                }
                for (n = 0; n < 8; n++)
                {
                    ((di, dj), nd) = GetDirectionData(n);
                    (ni, nj) = (i + di, j + dj);
                    if (IsValidTensorIndex(ni, nj, rows, cols, baseMap))
                    {
                        if (di != 0 && dj != 0)
                        {
                            if (!(IsValidTensorIndex(ni, j, rows, cols, baseMap) && IsValidTensorIndex(i, nj, rows, cols, baseMap))) continue;
                        }
                        if (nodeMap.ContainsKey((ni, nj))){
                            if (depth + nd >= nodeMap[(ni, nj)].nodeDepth) continue;
                        }
                        else
                        {
                            queue.Enqueue(((ni, nj), depth + nd));
                        }
                        nodeMap[(ni, nj)] = ((i, j), depth + nd);
                    }
                }
            }
        }
        if (maxIntensityPositions.Count == 0) return new();
        var selected = SLRandom.Choice(maxIntensityPositions);
        ((int sni, int snj), float snd) = nodeMap[selected];
        while(snd > 0)
        {
            lightPath.Add(GetWorldPosition(sni, snj));
            ((sni, snj), snd) = nodeMap[(sni, snj)];
        }
        lightPath.Reverse();
        return lightPath;
    }
    public Vector2 GetWorldPosition(int[] indices)
    {
        return GetWorldPosition(indices[0], indices[1]);
    }
    public Vector2 GetWorldPosition(int i, int j)
    {
        return new Vector2(j + offset.x + 0.5f, (mazeSize.rows - 1 - i) + offset.y + 0.5f);
    }
    public Vector2 GetWorldPosition((int i, int j) index) => GetWorldPosition(index.i, index.j);

    public List<Vector2> GetRandomPositions(int num)
    {
        return _mazeData.GetRandomPositions(currentRegion, num, StartTensorPosition, GoalTensorPosition).Select(p => GetWorldPosition(p)).ToList();
    }
    public Vector2 GetRandomPosition()
    {
        return GetWorldPosition(_mazeData.GetRandomPosition(currentRegion, new[] {StartTensorPosition,GoalTensorPosition}));
    }

    public Tensor<int> GetBaseMap() => _mazeData.GetBaseMap();
}