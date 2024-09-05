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
    private SkillManager _skillManager;

    public void Initilize(SkillManager skillManager,KeyCode keyCode)
    {
        if (_skillManager != null) 
        {
            _skillManager.OnCoolDownChanged -= OnCoolDownChanged;
        }
        _skillManager = skillManager;
        _skillManager.OnCoolDownChanged += OnCoolDownChanged;
        nameLabel.text = skillManager.Skill.SkillName;
        levelLabel.text = skillManager.Skill.SkillLevelStr;
        keyLabel.text = keyCode.ToString();
        Icon.sprite = skillManager.Skill.data.skillImage;
    }

    private void OnCoolDownChanged(float value)
    {
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
