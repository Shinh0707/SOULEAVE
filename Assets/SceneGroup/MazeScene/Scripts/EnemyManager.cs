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

        // TODO: ���H�̏�Ԃ�Tensor�ŕ\�����AAI�̊w�K�Ɏg�p���鏀��������
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
        // TODO: �v���C���[�ƓG�̈ʒu�𔽉f���Ė��H�̏�Ԃ��X�V
        // ��: mazeState[playerController.Position.y, playerController.Position.x] = 2;
        // �G�̈ʒu�����l�ɍX�V
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

    // TODO: AI���f���̊w�K���\�b�h������
    // TODO: �w�K�ς݃��f���̕ۑ��Ɠǂݍ��݋@�\������
    // TODO: �G�̎�ނɉ������قȂ�AI�헪������
    // TODO: �v���C���[�̍s���p�^�[�����w�K���A�K�����鍂�x��AI������
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
        // TODO: �G�̎��E���̖��H��Ԃ��X�V
        // ��: localMazeState = currentMazeState.Slice(position.y-5, position.y+5, position.x-5, position.x+5);
    }

    // TODO: �G�̗̑͂����\�͂�����
    // TODO: �v���C���[�Ƃ̑��ݍ�p�i�U���A����Ȃǁj������
    // TODO: �G�̏�Ԃɉ������r�W���A�����ʂ�����
}

public class EnemyAI
{
    // �ȒP�Ȍo�H�T���A���S���Y���iA*��Dijkstra�Ȃǁj������
    public Vector2Int GetNextMove(Vector2Int currentPosition, Vector2Int playerPosition, Tensor<int> mazeState)
    {
        // TODO: �o�H�T���A���S���Y��������
        // ���̎����Ƃ��āA�v���C���[�Ɍ�������1�}�X�ړ�����
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

    // TODO: �����w�K��p�����ӎv���胁�\�b�h������
    // TODO: �قȂ��Փx���x���ɉ�����AI�헪������
    // TODO: �����s�����s�������̓G��AI������
}