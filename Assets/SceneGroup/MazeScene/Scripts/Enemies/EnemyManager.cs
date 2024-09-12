using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SL.Lib;

public class EnemyManager : DynamicObject
{
    [SerializeField] private GameObject enemyPrefab;
    private List<EnemyController> enemies = new List<EnemyController>();

    public override void UpdateState()
    {
        foreach(var enemy in enemies)
        {
            enemy.UpdateState();
        }
    }
    public void HandleInput()
    {
        foreach (var enemy in enemies)
        {
            enemy.HandleInput();
        }
    }

    public void InitializeEnemies(List<Vector2> positions, (int,int) mazeSize)
    {
        foreach (var position in positions)
        {
            SpawnEnemy(position, mazeSize);
        }
    }

    private void SpawnEnemy(Vector2 position, (int,int) mazeSize)
    {
        GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();
        enemy.Initialize(position, mazeSize);
        enemies.Add(enemy);
    }

    private void SpawnNewEnemy()
    {
        Vector2 spawnPosition = MazeGameScene.Instance.MazeManager.GetRandomPosition();
        SpawnEnemy(spawnPosition, MazeGameScene.Instance.MazeManager.mazeSize);
    }

    public void RemoveEnemy(EnemyController enemy)
    {
        enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }
}