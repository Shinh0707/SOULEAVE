using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SkillNodeUI : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private Image SkillIcon;
    [SerializeField]
    private TextMeshProUGUI levelTextMesh;
    [SerializeField]
    private Color UnlockedColor = Color.white;
    [SerializeField]
    private Color LockedColor = Color.gray;
    private SelectableSkillName _skillName;
    public void Initialize(SelectableSkillName skillName)
    {
        _skillName = skillName;
        SkillIcon.sprite = skillName.Skill.data.skillImage;
        levelTextMesh.text = skillName.Skill.SkillLevelStr;
        var colors = button.colors;
        colors.normalColor = skillName.Skill.isUnlocked?UnlockedColor : LockedColor;
        button.colors = colors;
        Debug.Log($"Initialized {skillName}");
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (SkillTreeScene.Instance.State == SkillTreeSceneState.SkillSelect)
        {
            SkillTreeScene.Instance.DialogManager.DisplaySkillDetails(_skillName);
            SkillTreeScene.Instance.SkillTreeLayout.CenterOnNode(_skillName);
        }
    }

}