using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryScreen;

    [SerializeField] private Slider mpSlider;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private Slider sightSlider;
    [SerializeField] private TextMeshProUGUI sightText;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private GameObject hintArrow;
    [SerializeField] private TextMeshProUGUI elapsedTimeText;
    [SerializeField] private GameObject[] itemSlots;

    private float hintTimer;

    public void Initialize()
    {
        gameplayUI.SetActive(true);
        pauseMenu.SetActive(false);
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
        hintArrow.SetActive(false);
    }

    private void Update()
    {
        UpdateHintTimer();
        UpdateElapsedTime();
    }

    public void UpdatePlayerStats(float currentMp, float maxMp, float currentSight, float maxSight)
    {
        mpSlider.value = currentMp / maxMp;
        mpText.text = $"MP: {currentMp:F0} / {maxMp:F0}";

        sightSlider.value = currentSight / maxSight;
        sightText.text = $"Sight: {currentSight:F2} / {maxSight:F2}";
    }

    public void UpdateMinimap(Texture2D minimapTexture)
    {
        minimapImage.texture = minimapTexture;
    }

    public void ShowPauseMenu()
    {
        pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        pauseMenu.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        gameplayUI.SetActive(false);
        gameOverScreen.SetActive(true);
    }

    public void ShowVictoryScreen()
    {
        gameplayUI.SetActive(false);
        victoryScreen.SetActive(true);
    }

    public void ShowHint()
    {
        hintArrow.SetActive(true);
        hintTimer = MazeGameStats.Instance.HintDuration;
    }

    public void UpdateTeleportMode(bool isActive)
    {
        // Implement teleport mode UI update
    }

    private void UpdateHintTimer()
    {
        if (hintTimer > 0)
        {
            hintTimer -= Time.deltaTime;
            if (hintTimer <= 0)
            {
                hintArrow.SetActive(false);
            }
        }
    }

    private void UpdateElapsedTime()
    {
        float elapsedTime = Time.time - MazeGameScene.Instance.GameStartTime;
        elapsedTimeText.text = $"Time: {elapsedTime:F1}s";
    }

    public void OnResumeButtonClicked()
    {
        MazeGameScene.Instance.ResumeGame();
    }

    public void OnRestartButtonClicked()
    {
        MazeGameScene.Instance.RestartGame();
    }

    public void OnQuitButtonClicked()
    {
        MazeGameScene.Instance.QuitToMainMenu();
    }
}