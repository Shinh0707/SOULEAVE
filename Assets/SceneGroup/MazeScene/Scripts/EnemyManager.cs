using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float enemyMoveInterval = 1f;

    private List<EnemyController> enemies = new List<EnemyController>();
    private float lastMoveTime;
    private float monsterAddingTime;

    private MazeManager mazeManager;
    private PlayerController playerController;

    public void InitializeEnemies(List<Vector2Int> positions)
    {
        mazeManager = MazeGameScene.Instance.MazeManager;
        playerController = MazeGameScene.Instance.Player;

        foreach (var position in positions)
        {
            SpawnEnemy(position);
        }
    }

    private void Update()
    {
        if (MazeGameScene.Instance.CurrentState == MazeGameScene.GameState.Playing)
        {
            if (Time.time - lastMoveTime >= enemyMoveInterval)
            {
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
        enemy.Initialize(position);
        enemies.Add(enemy);
    }

    private void SpawnNewEnemy()
    {
        Vector2Int spawnPosition = mazeManager.GetPositions();
        SpawnEnemy(spawnPosition);
    }

    private void MoveEnemies()
    {
        foreach (EnemyController enemy in enemies)
        {
            Vector2Int newPosition = CalculateEnemyMove(enemy);
            if (mazeManager.IsValidMove(newPosition))
            {
                enemy.MoveTo(newPosition);
            }
        }
    }

    private Vector2Int CalculateEnemyMove(EnemyController enemy)
    {
        Vector2Int playerPosition = playerController.Position;
        Vector2Int enemyPosition = enemy.Position;
        Vector2Int direction = playerPosition - enemyPosition;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return enemyPosition + new Vector2Int(System.Math.Sign(direction.x), 0);
        }
        else
        {
            return enemyPosition + new Vector2Int(0, System.Math.Sign(direction.y));
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
}

public class EnemyController : MonoBehaviour
{
    private Vector2Int position;
    public Vector2Int Position => position;

    public void Initialize(Vector2Int startPosition)
    {
        position = startPosition;
    }

    public void MoveTo(Vector2Int newPosition)
    {
        position = newPosition;
        transform.position = MazeGameScene.Instance.MazeManager.GetWorldPosition(position);
    }
}