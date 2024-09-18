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
            yield return mazeManager.GenerateMazeAsync();
            _gameStartTime = Time.time;
            PlayerStatusManager.Instance.ResetRuntimeStatus();
            player = Instantiate(playerPrefab);
            playerController = player.GetComponent<PlayerController>();
            var mazeSize = MazeManager.mazeSize;
            playerController.Initialize(mazeManager.StartPosition, mazeSize);
            goal = Instantiate(goalPrefab, new Vector3(mazeManager.GoalPosition.x, mazeManager.GoalPosition.y, 0f), Quaternion.identity);
            enemyManager.InitializeEnemies(mazeManager.GetRandomPositions(mazeManager.firstEnemies), mazeSize);
            soulNPCManager.Initialize();
            soulNPCManager.InitializeSouls(mazeManager.GetRandomPositions(Mathf.Max(1,Mathf.RoundToInt(mazeManager.firstEnemies * 0.5f))), mazeSize);
            uiManager.Initialize();
            var cameraFollow = Camera.main.GetOrAddComponent<CameraFollow2D>();
            cameraFollow.Initialize(playerController.character.transform);
            cameraFollow.follow = true;
            GameTime = 0f;
            yield return new WaitForSeconds(2f); // レンダリング待ち
            CurrentState = GameState.Playing;
            // TODO: ゲーム開始時のサウンドを再生する処理を追加
        }

        protected override void OnFixedUpdate()
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

                // TODO: ゲームの経過時間を更新し、UIに反映する処理を追加
                // TODO: プレイヤーの位置に基づいてミニマップを更新する処理を追加
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
                 
                // TODO: ポーズ時のサウンドを再生する処理を追加
                // TODO: ポーズ中のバックグラウンド処理（例：統計の保存）を実装
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1;
                uiManager.HidePauseMenu();

                // TODO: ゲーム再開時のサウンドを再生する処理を追加
            }
        }

        private void CheckGameOverConditions()
        {
            if (playerController.IsDead)
            {
                CurrentState = GameState.GameOver;
                uiManager.ShowGameOverScreen();

                // TODO: ゲームオーバー時のサウンドを再生する処理を追加
                // TODO: プレイヤーの最終スコアを計算し、表示する処理を追加
            }
        }

        public void Respawn()
        {
            if(playerController.IsDead && MazeGameMemory.CanRespawn)
            {
                UIManager.HideGameOverScreen();
                MazeGameMemory.Respawn();
                playerController.Reinitialize();
                CurrentState = GameState.Playing;
                playerController.CurrentState |= CharacterState.Alive;
            }
        }

        public void Victory()
        {
            CurrentState = GameState.Victory;
            uiManager.ShowVictoryScreen();

            // TODO: 勝利時のサウンドを再生する処理を追加
            // TODO: プレイヤーの最終スコアを計算し、表示する処理を追加
        }

        public void RestartGame()
        {
            // ゲームを再起動するロジック

            // TODO: 現在のゲーム状態をリセットする処理を追加
            // TODO: プレイヤーの統計情報をリセットする処理を追加
            // TODO: 敵の位置をリセットする処理を追加
        }

        public void QuitToMainMenu(bool Success)
        {
            if (!Success)
            {
                UIManager.HideGameOverScreen();
            }
            else
            {
                MazeGameMemory.AssignMemory();
            }
            SceneManager.Instance.TransitionToScene(Scenes.Home);
        }

        // TODO: ゲームの難易度を変更する機能を追加
        // TODO: プレイヤーの進捗を保存する機能を追加
        // TODO: ゲーム内イベント（例：特殊アイテムの出現）を管理する機能を追加
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