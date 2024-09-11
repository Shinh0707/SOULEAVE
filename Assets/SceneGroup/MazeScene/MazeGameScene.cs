using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using SL.Lib;
public class MazeGameScene : SingletonMonoBehaviour<MazeGameScene>
{
    public enum GameState
    {
        Setup,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    [Flags]
    public enum GameFlag
    {
        None = 0,
        FreezeTime = 1,
        FreezeState = 2,
        FreezePlayerInput = 4,
        FreezeEnemyInput = 8,
        FreezeInput = 12
    }
    public GameState CurrentState { get; private set; }
    public GameFlag CurrentFlag { get; private set; }
    [SerializeField] private MazeManager mazeManager;
    [SerializeField] private GameObject playerPrefab;
    private GameObject player;
    [SerializeField] private GameObject goalPrefab;
    private GameObject goal;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private GameUIManager uiManager;
    public MazeManager MazeManager => mazeManager;
    private PlayerController playerController;
    public PlayerController Player => playerController;
    public EnemyManager EnemyManager => enemyManager;
    public GameUIManager UIManager => uiManager;
    private float _gameStartTime;
    public float GameTime { get; private set; }
    public float GameStartTime => _gameStartTime;

    private void Start()
    {
        if (!MazeGameStats.Instance.IsInitialized) {
            MazeGameStats.OnInitialized += OnMazeGameStatsInitialized;
        }
        else
        {
            OnMazeGameStatsInitialized();
        }
    }

    private void OnMazeGameStatsInitialized()
    {
        StartCoroutine(SetupGame());
    }

    private IEnumerator SetupGame()
    {
        CurrentState = GameState.Setup;
        yield return mazeManager.GenerateMazeAsync();
        _gameStartTime = Time.time;
        PlayerStatusManager.Instance.ResetRuntimeStatus();
        player = Instantiate(playerPrefab);
        playerController = player.GetComponent<PlayerController>();
        var mazeSize = MazeManager.mazeSize;
        playerController.Initialize(mazeManager.StartPosition, mazeSize);
        goal = Instantiate(goalPrefab, new Vector3(mazeManager.GoalPosition.x, mazeManager.GoalPosition.y, 0f), Quaternion.identity);
        enemyManager.InitializeEnemies(mazeManager.GetRandomPositions(mazeManager.firstEnemies), mazeSize);
        uiManager.Initialize();
        CurrentState = GameState.Playing;
        var cameraFollow = Camera.main.GetOrAddComponent<CameraFollow2D>();
        cameraFollow.Initialize(playerController.character);
        cameraFollow.follow = true;
        GameTime = 0f;

        // TODO: �Q�[���J�n���̃T�E���h���Đ����鏈����ǉ�
    }

    private void FixedUpdate()
    {
        if (CurrentState == GameState.Playing)
        {
            if (!CurrentFlag.HasFlag(GameFlag.FreezeTime))
            {
                GameTime += Time.fixedDeltaTime;
                UIManager.UpdateElapsedTime(GameTime);
            }
            if (!CurrentFlag.HasFlag(GameFlag.FreezeState))
            {
                Player.ApplyQueuedMove();
                EnemyManager.ApplyQueuedMove();
                Player.UpdateState();
                EnemyManager.UpdateState();

            }
            if (!CurrentFlag.HasFlag(GameFlag.FreezePlayerInput))
            {
                Player.HandleInput();
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGame();
                }
            }
            if (!CurrentFlag.HasFlag(GameFlag.FreezeEnemyInput))
            {
                EnemyManager.HandleInput();
            }
            CheckGameOverConditions();

            // TODO: �Q�[���̌o�ߎ��Ԃ��X�V���AUI�ɔ��f���鏈����ǉ�
            // TODO: �v���C���[�̈ʒu�Ɋ�Â��ă~�j�}�b�v���X�V���鏈����ǉ�
        }
    }

    public void SetFreezeInput(bool freeze)
    {
        if (freeze)
        {
            if (!CurrentFlag.HasFlag(GameFlag.FreezeInput))
            {
                CurrentFlag |= GameFlag.FreezeInput;
                //OnInputFreeze();
            }
        }
        else
        {
            if (CurrentFlag.HasFlag(GameFlag.FreezeInput))
            {
                CurrentFlag &= ~GameFlag.FreezeInput;
                //OnInputUnfreeze();
            }
        }
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0;
            uiManager.ShowPauseMenu();

            // TODO: �|�[�Y���̃T�E���h���Đ����鏈����ǉ�
            // TODO: �|�[�Y���̃o�b�N�O���E���h�����i��F���v�̕ۑ��j������
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1;
            uiManager.HidePauseMenu();

            // TODO: �Q�[���ĊJ���̃T�E���h���Đ����鏈����ǉ�
        }
    }

    private void CheckGameOverConditions()
    {
        if (playerController.IsDead)
        {
            CurrentState = GameState.GameOver;
            uiManager.ShowGameOverScreen();

            // TODO: �Q�[���I�[�o�[���̃T�E���h���Đ����鏈����ǉ�
            // TODO: �v���C���[�̍ŏI�X�R�A���v�Z���A�\�����鏈����ǉ�
        }
    }

    public void Victory()
    {
        CurrentState = GameState.Victory;
        uiManager.ShowVictoryScreen();

        // TODO: �������̃T�E���h���Đ����鏈����ǉ�
        // TODO: �v���C���[�̍ŏI�X�R�A���v�Z���A�\�����鏈����ǉ�
    }

    public void RestartGame()
    {
        // �Q�[�����ċN�����郍�W�b�N
        StartCoroutine(SetupGame());

        // TODO: ���݂̃Q�[����Ԃ����Z�b�g���鏈����ǉ�
        // TODO: �v���C���[�̓��v�������Z�b�g���鏈����ǉ�
        // TODO: �G�̈ʒu�����Z�b�g���鏈����ǉ�
    }

    public void QuitToMainMenu()
    {
        // ���C�����j���[�֖߂郍�W�b�N
        // ��: SceneManager.LoadScene("MainMenu");

        // TODO: ���݂̃Q�[����Ԃ�ۑ����鏈����ǉ�
        // TODO: �V�[���J�ڂ̃A�j���[�V������ǉ�
        // TODO: �o�b�N�O���E���h�~���[�W�b�N���~�܂��͕ύX���鏈����ǉ�
    }

    // TODO: �Q�[���̓�Փx��ύX����@�\��ǉ�
    // TODO: �v���C���[�̐i����ۑ�����@�\��ǉ�
    // TODO: �Q�[�����C�x���g�i��F����A�C�e���̏o���j���Ǘ�����@�\��ǉ�
    public IEnumerator WaitForNextPlayingFrame()
    {
        yield return new WaitForNextPlayingFrame();
    }
}

public class WaitForGameSeconds : CustomYieldInstruction
{
    private float waitTime;
    private float startTime;

    public WaitForGameSeconds(float time)
    {
        waitTime = time;
        startTime = MazeGameScene.Instance.GameTime;
    }

    public override bool keepWaiting
    {
        get
        {
            if (MazeGameScene.Instance.CurrentState == MazeGameScene.GameState.Paused)
            {
                startTime = MazeGameScene.Instance.GameTime;
                return true;
            }
            return MazeGameScene.Instance.GameTime - startTime < waitTime;
        }
    }
}
public class WaitForNextPlayingFrame : CustomYieldInstruction
{
    private bool wasPlayingLastFrame = false;

    public override bool keepWaiting
    {
        get
        {
            bool isPlayingNow = MazeGameScene.Instance.CurrentState == MazeGameScene.GameState.Playing;

            if (!wasPlayingLastFrame && isPlayingNow)
            {
                wasPlayingLastFrame = true;
                return true;
            }

            wasPlayingLastFrame = isPlayingNow;
            return !isPlayingNow;
        }
    }
}