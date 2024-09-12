using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillTreeSceneState
{
    Loading,
    SkillSelect,
    KeySelect
}

public class SkillTreeScene : SceneInitializer<SkillTreeScene>
{
    [SerializeField] private SkillTreeLayout skillTreeLayout;
    [SerializeField] private SkillTreeDialogManager dialogManager;
    public SkillTreeLayout SkillTreeLayout => skillTreeLayout;
    public SkillTreeDialogManager DialogManager => dialogManager;

    public SkillTreeSceneState State { get; set; }

    protected override void BeforeInitialize()
    {
        State = SkillTreeSceneState.Loading;
        sceneState = SceneState.WaitInitialize;
        StartCoroutine(Loading());
        StartCoroutine(skillTreeLayout.Initialize());
        StartCoroutine(dialogManager.Initialize());
    }

    private IEnumerator Loading()
    {
        while(State == SkillTreeSceneState.Loading)
        {
            yield return null;
        }
        sceneState = SceneState.Initializing;
    }
}