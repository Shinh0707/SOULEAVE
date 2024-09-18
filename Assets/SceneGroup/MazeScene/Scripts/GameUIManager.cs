using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using SL.Lib;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryScreen;

    [SerializeField] private CustomUIGauge fluxGauge;
    [SerializeField] private TextMeshProUGUI fluxGaugeValueText;
    [SerializeField] private CustomUIGauge intensityGauge;
    [SerializeField] private TextMeshProUGUI intensityGaugeValueText;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private RectTransform SkillBankBox;
    [SerializeField] private GameObject SkillBankContent;
    private Dictionary<KeyCode,SkillBoxUI> skillBoxes = new();
    [SerializeField] private TextMeshProUGUI elapsedTimeText;
    [SerializeField] private GameObject[] itemSlots;

    public void Initialize()
    {
        gameplayUI.SetActive(true);
        pauseMenu.SetActive(false);
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
        UpdateSkillBank();
    }

    public void UpdateSkillBank()
    {
        var skillBank = MazeGameScene.Instance.Player.SkillBank;
        foreach(var skillKey in skillBank.Keys)
        {
            if (!skillBoxes.ContainsKey(skillKey))
            {
                var newSkillBox = Instantiate(SkillBankContent, SkillBankBox);
                skillBoxes[skillKey] = newSkillBox.GetComponent<SkillBoxUI>();
            }
            skillBoxes[skillKey].Initilize(skillBank[skillKey], skillKey);
        }
        var dontNeedKeys = skillBoxes.Keys.Where(sk => !skillBank.ContainsKey(sk));
        foreach (var skillKey in dontNeedKeys)
        {
            Destroy(skillBoxes[skillKey]);
            skillBoxes.Remove(skillKey);   
        }
    }

    public void UpdatePlayerStats(float currentFlux, float maxFlux, float currentIntensity, float maxIntensity)
    {
        fluxGauge.CurrentValue = currentFlux / maxFlux;
        fluxGaugeValueText.text = $"{currentFlux:F0} / {maxFlux:F0}";

        intensityGauge.CurrentValue = currentIntensity / maxIntensity;
        intensityGaugeValueText.text = $"{currentIntensity:F2} / {maxIntensity:F2}";
    }
    public void UpdatePlayerStats(float currentMp,float currentSight)
    {
        UpdatePlayerStats(currentMp, PlayerStatusManager.MaxMP,currentSight, PlayerStatusManager.MaxIntensity);
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
    public void HideGameOverScreen()
    {
        gameplayUI.SetActive(true);
        gameOverScreen.SetActive(false);
    }

    public void ShowVictoryScreen()
    {
        gameplayUI.SetActive(false);
        victoryScreen.SetActive(true);
    }

    public void UpdateElapsedTime(float elapsedTime)
    {
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