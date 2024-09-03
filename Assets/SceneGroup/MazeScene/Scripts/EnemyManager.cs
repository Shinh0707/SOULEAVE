using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SL.Lib;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float enemyMoveInterval = 1f;
    private List<EnemyController> enemies = new List<EnemyController>();
    private float lastMoveTime;
    private float monsterAddingTime;
    private MazeManager mazeManager;
    private PlayerController playerController;
    private Tensor<int> mazeState;

    public void InitializeEnemies(List<Vector2Int> positions)
    {
        mazeManager = MazeGameScene.Instance.MazeManager;
        playerController = MazeGameScene.Instance.Player;
        mazeState = mazeManager.GetBaseMap();

        foreach (var position in positions)
        {
            SpawnEnemy(position);
        }

        // TODO: 迷路の状態をTensorで表現し、AIの学習に使用する準備をする
    }

    private void Update()
    {
        if (MazeGameScene.Instance.CurrentState == MazeGameScene.GameState.Playing)
        {
            if (Time.time - lastMoveTime >= enemyMoveInterval)
            {
                UpdateMazeState();
                MoveEnemies();
                lastMoveTime = Time.time;
            }

            monsterAddingTime += Time.deltaTime;
            if (monsterAddingTime >= MazeGameStats.Instance.MonsterAddingInterval)
            {
                monsterAddingTime = 0;
                SpawnNewEnemy();
            }
        }
    }

    private void SpawnEnemy(Vector2Int position)
    {
        Vector3 worldPosition = mazeManager.GetWorldPosition(position);
        GameObject enemyObject = Instantiate(enemyPrefab, worldPosition, Quaternion.identity);
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();
        enemy.Initialize(position, mazeState);
        enemies.Add(enemy);
    }

    private void SpawnNewEnemy()
    {
        Vector2Int spawnPosition = mazeManager.GetPositions();
        SpawnEnemy(spawnPosition);
    }

    private void UpdateMazeState()
    {
        // TODO: プレイヤーと敵の位置を反映して迷路の状態を更新
        // 例: mazeState[playerController.Position.y, playerController.Position.x] = 2;
        // 敵の位置も同様に更新
    }

    private void MoveEnemies()
    {
        foreach (EnemyController enemy in enemies)
        {
            Vector2Int newPosition = enemy.DecideNextMove(playerController.Position, mazeState);
            if (mazeManager.IsValidMove(newPosition))
            {
                enemy.MoveTo(newPosition);
            }
        }
    }

    public bool IsEnemyAtPosition(Vector2Int position)
    {
        return enemies.Any(e => e.Position == position);
    }

    public void RemoveEnemy(EnemyController enemy)
    {
        enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }

    // TODO: AIモデルの学習メソッドを実装
    // TODO: 学習済みモデルの保存と読み込み機能を実装
    // TODO: 敵の種類に応じた異なるAI戦略を実装
    // TODO: プレイヤーの行動パターンを学習し、適応する高度なAIを実装
}

public class EnemyController : MonoBehaviour
{
    private Vector2Int position;
    public Vector2Int Position => position;
    private Tensor<int> localMazeState;
    private EnemyAI ai;

    public void Initialize(Vector2Int startPosition, Tensor<int> mazeState)
    {
        position = startPosition;
        localMazeState = new Tensor<int>(mazeState);
        ai = new EnemyAI();
    }

    public void MoveTo(Vector2Int newPosition)
    {
        position = newPosition;
        transform.position = MazeGameScene.Instance.MazeManager.GetWorldPosition(position);
    }

    public Vector2Int DecideNextMove(Vector2Int playerPosition, Tensor<int> currentMazeState)
    {
        UpdateLocalMazeState(currentMazeState);
        return ai.GetNextMove(position, playerPosition, localMazeState);
    }

    private void UpdateLocalMazeState(Tensor<int> currentMazeState)
    {
        // TODO: 敵の視界内の迷路状態を更新
        // 例: localMazeState = currentMazeState.Slice(position.y-5, position.y+5, position.x-5, position.x+5);
    }

    // TODO: 敵の体力や特殊能力を実装
    // TODO: プレイヤーとの相互作用（攻撃、回避など）を実装
    // TODO: 敵の状態に応じたビジュアル効果を実装
}

public class EnemyAI
{
    // 簡単な経路探索アルゴリズム（A*やDijkstraなど）を実装
    public Vector2Int GetNextMove(Vector2Int currentPosition, Vector2Int playerPosition, Tensor<int> mazeState)
    {
        // TODO: 経路探索アルゴリズムを実装
        // 仮の実装として、プレイヤーに向かって1マス移動する
        Vector2Int direction = playerPosition - currentPosition;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return currentPosition + new Vector2Int(System.Math.Sign(direction.x), 0);
        }
        else
        {
            return currentPosition + new Vector2Int(0, System.Math.Sign(direction.y));
        }
    }

    // TODO: 強化学習を用いた意思決定メソッドを実装
    // TODO: 異なる難易度レベルに応じたAI戦略を実装
    // TODO: 協調行動を行う複数の敵のAIを実装
}