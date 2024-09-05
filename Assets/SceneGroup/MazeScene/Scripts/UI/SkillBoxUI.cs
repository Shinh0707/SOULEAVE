using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBoxUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private Image Icon;
    [SerializeField] private Image background;
    private SkillManager _skillManager;

    public void Initilize(SkillManager skillManager,KeyCode keyCode)
    {
        if (_skillManager != null) 
        {
            _skillManager.OnCoolDownChanged -= OnCoolDownChanged;
        }
        _skillManager = skillManager;
        _skillManager.OnCoolDownChanged += OnCoolDownChanged;
        OnCoolDownChanged(0f);
        nameLabel.text = skillManager.Skill.SkillName;
        levelLabel.text = skillManager.Skill.SkillLevelStr;
        keyLabel.text = keyCode.ToString();
        Icon.sprite = skillManager.Skill.data.skillImage;
    }

    private void OnCoolDownChanged(float value)
    {

        if (value > 0)
        {
            slider.fillRect.gameObject.SetActive(true);
            background.color = SLColors.I.TransparentBlack;
        }
        else
        {
            slider.fillRect.gameObject.SetActive(false);
            background.color = SLColors.I.TransparentBlackCyan;
        }
        slider.value = value;

    }

    private void OnDestroy()
    {
        if (_skillManager != null)
        {
            _skillManager.OnCoolDownChanged -= OnCoolDownChanged;
        }
    }
}
