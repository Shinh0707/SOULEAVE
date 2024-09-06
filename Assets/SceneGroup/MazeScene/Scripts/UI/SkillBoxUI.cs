using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBoxUI : MonoBehaviour
{
    [SerializeField] private Animator guage;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private Image Icon;
    //[SerializeField] private Image background;
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
        guage.SetFloat("value",value >= 1?2:(value<=0?-1:value));

    }

    private void OnDestroy()
    {
        if (_skillManager != null)
        {
            _skillManager.OnCoolDownChanged -= OnCoolDownChanged;
        }
    }
}
