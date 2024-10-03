using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace SL.Lib
{
    [Serializable]
    public class MazeGameMemory
    {
        public int CollectedLP = 0;
        public int RespawnCount = 0;

        public int TotalCollectedLP => CollectedLP + PlayerStatus.Instance.PlayerParameter.LP;

        public void AssignMemory()
        {
            PlayerStatus.Instance.PlayerParameter.LP = Mathf.Max(0, TotalCollectedLP);
        }
        public int RespwanCost => 1 + RespawnCount;
        public bool CanRespawn => TotalCollectedLP >= RespwanCost;
        public void Respawn()
        {
            CollectedLP -= RespwanCost;
            RespawnCount++;
        }
    }
    public class MazeGameScene : SceneInitializer<MazeGameScene>
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
            FreezeInput = 12,
            FreezeAll = 27
        }
        public GameState CurrentState { get; private set; }
        public GameFlag CurrentFlag { get; private set; }
        [SerializeField] private MazeManager mazeManager;
        [SerializeField] private GameObject playerPrefab;
        private GameObject player;
        [SerializeField] private GameObject goalPrefab;
        private GameObject goal;
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private SoulNPCManager soulNPCManager;
        [SerializeField] private GameUIManager uiManager;
        public MazeGameMemory MazeGameMemory = new();
        public MazeManager MazeManager => mazeManager;
        private PlayerController playerController;
        public PlayerController Player => playerController;
        public EnemyManager EnemyManager => enemyManager;
        public SoulNPCManager SoulNPCManager => soulNPCManager;
        public GameUIManager UIManager => uiManager;
        private float _gameStartTime;
        public float GameTime { get; private set; }
        public float GameStartTime => _gameStartTime;


        protected override IEnumerator InitializeScene()
        {
            CurrentState = GameState.Setup;
            MazeGameMemory = new();
            // GenerateMazeAsync�́A���I�ɖ��H�𐶐�����ꍇ�Ɏg�p����
            // StageSelect����Singleton��SelectedStage��ێ������Ă����A���ꂪ���I��������X�e�[�W�Ȃ瓮�I��������B�����łȂ���΃X�e�[�W��ǂݍ��ށB
            mazeManager.Initialize();
            yield return mazeManager.GenerateMazeAsync();
            mazeManager.MazeData.SetEnemyArea(EnemyTypeSelect.Common | EnemyTypeSelect.Shot, mazeManager.CurrentRegion);
            PlayerStatusManager.Instance.ResetRuntimeStatus();
            player = Instantiate(playerPrefab);
            playerController = player.GetComponent<PlayerController>();
            playerController.Initialize(mazeManager.StartPosition);
            goal = Instantiate(goalPrefab, new Vector3(mazeManager.GoalPosition.x, mazeManager.GoalPosition.y, 0f), Quaternion.identity);
            enemyManager.SpawnEnemies(mazeManager.GetEnemySpawnData());
            soulNPCManager.Initialize();
            soulNPCManager.InitializeSouls(mazeManager.GetRandomPositions(Mathf.Max(1,Mathf.RoundToInt(mazeManager.firstEnemies * 0.5f))));
            uiManager.Initialize();
            var cameraFollow = Camera.main.GetOrAddComponent<CameraFollow2D>();
            cameraFollow.Initialize(playerController.character.transform);
            cameraFollow.follow = true;
            GameTime = 0f;
            yield return new WaitForSeconds(2f); // �����_�����O�҂�
            _gameStartTime = Time.time;
            CurrentState = GameState.Playing;
            // TODO: �Q�[���J�n���̃T�E���h���Đ����鏈����ǉ�
        }

        protected override void OnFixedUpdate()
        {
            Player.OnUpdate();
            EnemyManager.OnUpdate();
            SoulNPCManager.OnUpdate();
            if (CurrentState == GameState.Playing)
            {
                if (!CurrentFlag.HasFlag(GameFlag.FreezeTime))
                {
                    GameTime += Time.fixedDeltaTime;
                    UIManager.UpdateElapsedTime(GameTime);
                }
                if (!CurrentFlag.HasFlag(GameFlag.FreezeState))
                {
                    mazeManager.ResetIntensityMap();
                    Player.UpdateState();
                    EnemyManager.UpdateState();
                    SoulNPCManager.UpdateState();
                    CheckGameOverConditions();
                }
                if (!CurrentFlag.HasFlag(GameFlag.FreezePlayerInput))
                {
                    Player.HandleInput();
                    SoulNPCManager.HandleInput();
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        PauseGame();
                    }
                }
                if (!CurrentFlag.HasFlag(GameFlag.FreezeEnemyInput))
                {
                    EnemyManager.HandleInput();
                }

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
                CurrentFlag = GameFlag.FreezeAll;
                uiManager.ShowGameOverScreen();

                // TODO: �Q�[���I�[�o�[���̃T�E���h���Đ����鏈����ǉ�
                // TODO: �v���C���[�̍ŏI�X�R�A���v�Z���A�\�����鏈����ǉ�
            }
        }

        public void Respawn()
        {
            if(playerController.IsDead && MazeGameMemory.CanRespawn)
            {
                UIManager.HideGameOverScreen();
                MazeGameMemory.Respawn();
                playerController.Reinitialize();
                playerController.CurrentState |= CharacterState.Alive;
                CurrentState = GameState.Playing;
                CurrentFlag = GameFlag.None;
                playerController.StartInvincible();
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

            // TODO: ���݂̃Q�[����Ԃ����Z�b�g���鏈����ǉ�
            // TODO: �v���C���[�̓��v�������Z�b�g���鏈����ǉ�
            // TODO: �G�̈ʒu�����Z�b�g���鏈����ǉ�
        }

        public void QuitToMainMenu(bool Success)
        {
            if (!Success)
            {
                if(MazeGameMemory.CollectedLP > 0)
                {
                    MazeGameMemory.CollectedLP = 0;
                }
            }
            MazeGameMemory.AssignMemory();
            SceneManager.Instance.TransitionToScene(Scenes.Home);
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
}