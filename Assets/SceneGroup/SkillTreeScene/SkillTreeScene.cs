using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillTreeSceneState
{
    Loading,
    SkillSelect,
    KeySelect
}

public class SkillTreeScene : SingletonMonoBehaviour<SkillTreeScene>
{
    [SerializeField] private SkillTreeLayout skillTreeLayout;
    [SerializeField] private SkillTreeDialogManager dialogManager;
    public SkillTreeLayout SkillTreeLayout => skillTreeLayout;
    public SkillTreeDialogManager DialogManager => dialogManager;

    private SkillTreeSceneState _state;

    public SkillTreeSceneState State
    {
        get
        {
            return _state;
        }
        set
        {
            if (_state != value) 
            {
                _state = value;
                OnChangeState(value);
            }
        }
    }

    private void Start()
    {
        State = SkillTreeSceneState.Loading;
        StartCoroutine(ShowLoading());
        StartCoroutine(skillTreeLayout.Initialize());
        StartCoroutine(dialogManager.Initialize());
    }

    private IEnumerator ShowLoading()
    {
        LoadingManager.Instance.StartLoading();
        while(State == SkillTreeSceneState.Loading)
        {
            yield return null;
            // ローディングの演出
        }
        LoadingManager.Instance.EndLoading();
    }

    private void OnChangeState(SkillTreeSceneState state)
    {

    }

    public void UpdateSkillAvailability()
    {
        //skillTreeLayout.UpdateSkillAvailability();
    }
}