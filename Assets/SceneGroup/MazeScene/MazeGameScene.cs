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

        // TODO: �v���C���[�̏����ʒu���J�����ɒǏ]�����鏈����ǉ�
        // TODO: �Q�[���J�n���̃T�E���h���Đ����鏈����ǉ�
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

            // TODO: �Q�[���̌o�ߎ��Ԃ��X�V���AUI�ɔ��f���鏈����ǉ�
            // TODO: �v���C���[�̈ʒu�Ɋ�Â��ă~�j�}�b�v���X�V���鏈����ǉ�
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
}