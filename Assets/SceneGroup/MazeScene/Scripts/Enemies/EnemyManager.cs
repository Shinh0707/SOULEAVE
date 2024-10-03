using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SL.Lib;

public class EnemyManager : DynamicObject
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        EnemyControllerManager.OnUpdate();
    }
    public override void UpdateState()
    {
       base.UpdateState();
        EnemyControllerManager.UpdateState();
    }
    public void HandleInput()
    {
        EnemyControllerManager.HandleInput();
    }

    public void SpawnEnemies(EnemySpawnData[] enemySpawnDatas)
    {
        foreach (var spawnData in enemySpawnDatas)
        {
            SpawnEnemy(spawnData);
        }
    }

    private void SpawnEnemy(EnemySpawnData enemySpawnData)
    {
        GameObject enemyObject = Instantiate(EnemyManagerData.Instance.GetEnemyPrefab(enemySpawnData.EnemyType), enemySpawnData.Position, Quaternion.identity);
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();
        AssignEnemy(enemy, enemySpawnData.Position);
    }

    public void AssignEnemy(EnemyController enemy, Vector2 position)
    {
        enemy.Initialize(position);
    }
}