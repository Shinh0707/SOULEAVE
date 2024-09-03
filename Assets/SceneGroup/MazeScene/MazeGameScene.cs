using UnityEngine;
using System.Collections;

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
        MazeGameStats.OnInitialized += OnMazeGameStatsInitialized;
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
        playerController.Initialize(mazeManager.StartPosition);
        enemyManager.InitializeEnemies(mazeManager.GetPositions(mazeManager.firstEnemies));
        uiManager.Initialize();

        CurrentState = GameState.Playing;
        
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
            CheckVictoryConditions();
        }
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0;
            uiManager.ShowPauseMenu();
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1;
            uiManager.HidePauseMenu();
        }
    }

    private void CheckGameOverConditions()
    {
        if (playerController.IsDead)
        {
            CurrentState = GameState.GameOver;
            uiManager.ShowGameOverScreen();
        }
    }

    private void CheckVictoryConditions()
    {
        if (mazeManager.IsInGoal(playerController.Position))
        {
            CurrentState = GameState.Victory;
            uiManager.ShowVictoryScreen();
        }
    }

    public void RestartGame()
    {
        // ゲームを再起動するロジック
        StartCoroutine(SetupGame());
    }

    public void QuitToMainMenu()
    {
        // メインメニューへ戻るロジック
        // 例: SceneManager.LoadScene("MainMenu");
    }
}