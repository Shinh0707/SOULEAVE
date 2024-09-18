using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI RespawnCostText;
    [SerializeField] private Button RespawnButton;
    [SerializeField] private Button GiveUpButton;
    private void OnEnable()
    {
        var afterRemain = MazeGameScene.Instance.MazeGameMemory.TotalCollectedLP - MazeGameScene.Instance.MazeGameMemory.RespwanCost;
        RespawnCostText.text = $"åªç›ÇÃç∞ {MazeGameScene.Instance.MazeGameMemory.TotalCollectedLP} Å® {(afterRemain < 0?$"<color=#9B1C38>{afterRemain}</color>":afterRemain)}";
        if (MazeGameScene.Instance.MazeGameMemory.CanRespawn)
        {
            RespawnButton.gameObject.SetActive(true);
            RespawnButton.onClick.RemoveAllListeners();
            RespawnButton.onClick.AddListener(() => MazeGameScene.Instance.Respawn());
        }
        else
        {
            RespawnButton.gameObject.SetActive(false);
        }
        GiveUpButton.onClick.RemoveAllListeners();
        GiveUpButton.onClick.AddListener(() => MazeGameScene.Instance.QuitToMainMenu(false));
    }
}
