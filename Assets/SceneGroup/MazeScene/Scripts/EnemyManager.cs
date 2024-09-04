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

    public void InitializeEnemies(List<Vector2> positions, (int,int) mazeSize)
    {
        foreach (var position in positions)
        {
            SpawnEnemy(position, mazeSize);
        }

        // TODO: ���H�̏�Ԃ�Tensor�ŕ\�����AAI�̊w�K�Ɏg�p���鏀��������
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
        Vector2 spawnPosition = MazeGameScene.Instance.MazeManager.GetPosition();
        SpawnEnemy(spawnPosition, MazeGameScene.Instance.MazeManager.mazeSize);
    }

    public void RemoveEnemy(EnemyController enemy)
    {
        enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }
}