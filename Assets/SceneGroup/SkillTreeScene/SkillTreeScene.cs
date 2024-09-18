using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private BasicStatusDrawer statusDrawer;
    [SerializeField] private Button backToButton;
    public SkillTreeLayout SkillTreeLayout => skillTreeLayout;
    public SkillTreeDialogManager DialogManager => dialogManager;

    public void UpdateStatusDrawer()
    {
        statusDrawer.UpdateValues();
    }

    public SkillTreeSceneState State { get; set; }

    protected override void BeforeInitialize()
    {
        State = SkillTreeSceneState.Loading;
        sceneState = SceneState.WaitInitialize;
        StartCoroutine(Loading());
        StartCoroutine(skillTreeLayout.Initialize());
        StartCoroutine(dialogManager.Initialize());
        backToButton.onClick.RemoveAllListeners();
        backToButton.onClick.AddListener(() => SceneManager.Instance.TransitionToScene(Scenes.Home));
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