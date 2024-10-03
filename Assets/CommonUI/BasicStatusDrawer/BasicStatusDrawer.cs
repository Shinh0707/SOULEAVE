using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasicStatusDrawer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI LPValue;
    [SerializeField] private TextMeshProUGUI FluxValue;
    [SerializeField] private TextMeshProUGUI IntensityValue;

    private void Start()
    {
        UpdateValues();
    }

    public void UpdateValues()
    {
        PlayerStatusManager.Instance.ResetRuntimeStatus();
        LPValue.text = $"{PlayerStatus.Instance.PlayerParameter.LP:F0}";
        FluxValue.text = $"{PlayerStatusManager.Instance.RuntimeStatus.MaxFlux:F2}";
        IntensityValue.text = $"{PlayerStatusManager.Instance.RuntimeStatus.MaxIntensity:F2}";
    }
}
