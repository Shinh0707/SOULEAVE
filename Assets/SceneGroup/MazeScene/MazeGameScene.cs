using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
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
    public GameState CurrentState { get; private set; }
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
        player = Instantiate(playerPrefab);
        playerController = player.GetComponent<PlayerController>();
        var mazeSize = MazeManager.mazeSize;
        playerController.Initialize(mazeManager.StartPosition, mazeSize);
        goal = Instantiate(goalPrefab, new Vector3(mazeManager.GoalPosition.x, mazeManager.GoalPosition.y, 0f), Quaternion.identity);
        enemyManager.InitializeEnemies(mazeManager.GetPositions(mazeManager.firstEnemies), mazeSize);
        uiManager.Initialize();
        CurrentState = GameState.Playing;
        var cameraFollow = Camera.main.GetOrAddComponent<CameraFollow2D>();
        cameraFollow.Initialize(player.transform);
        cameraFollow.follow = true;

        // TODO: プレイヤーの初期位置をカメラに追従させる処理を追加
        // TODO: ゲーム開始時のサウンドを再生する処理を追加
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
            CheckGameOverConditions();

            // TODO: ゲームの経過時間を更新し、UIに反映する処理を追加
            // TODO: プレイヤーの位置に基づいてミニマップを更新する処理を追加
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
        StartCoroutine(SetupGame());

        // TODO: 現在のゲーム状態をリセットする処理を追加
        // TODO: プレイヤーの統計情報をリセットする処理を追加
        // TODO: 敵の位置をリセットする処理を追加
    }

    public void QuitToMainMenu()
    {
        // メインメニューへ戻るロジック
        // 例: SceneManager.LoadScene("MainMenu");

        // TODO: 現在のゲーム状態を保存する処理を追加
        // TODO: シーン遷移のアニメーションを追加
        // TODO: バックグラウンドミュージックを停止または変更する処理を追加
    }

    // TODO: ゲームの難易度を変更する機能を追加
    // TODO: プレイヤーの進捗を保存する機能を追加
    // TODO: ゲーム内イベント（例：特殊アイテムの出現）を管理する機能を追加
}