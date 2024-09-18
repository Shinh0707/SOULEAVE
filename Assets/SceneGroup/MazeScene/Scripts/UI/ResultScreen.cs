using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] private Button ContinueButton;
    [SerializeField] private TextMeshProUGUI TotalSoulsText;
    [SerializeField] private TextMeshProUGUI SavedSoulsText;
    [SerializeField] private TextMeshProUGUI EatenSoulsText;
    [SerializeField] private TextMeshProUGUI AbandonedSoulsText;

    private void OnEnable()
    {
        TotalSoulsText.text = $"{MazeGameScene.Instance.SoulNPCManager.TotalSouls}";
        SavedSoulsText.text = $"{MazeGameScene.Instance.SoulNPCManager.SavedSouls}";
        EatenSoulsText.text = $"{MazeGameScene.Instance.SoulNPCManager.EatenSouls}";
        AbandonedSoulsText.text = $"{MazeGameScene.Instance.SoulNPCManager.CurrentSouls}";
        ContinueButton.onClick.RemoveAllListeners();
        ContinueButton.onClick.AddListener(() => MazeGameScene.Instance.QuitToMainMenu(true));
    }
}
