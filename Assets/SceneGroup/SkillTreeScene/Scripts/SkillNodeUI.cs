using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SkillNodeUI : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private Image SkillIcon;
    private SelectableSkillName _skillName;
    public void Initialize(SelectableSkillName skillName)
    {
        _skillName = skillName;
        SkillIcon.sprite = skillName.Skill.data.skillImage;
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