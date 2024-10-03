using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSceneZone : SnapZone
{
    [SerializeField] private Button button;
    [SerializeField] private Scenes scene;
    [SerializeField] private AnimatorClipSelector animatorClipSelector;

    private CancellableAnimationPlayer currentPlayer;

    protected override void Awake()
    {
        base.Awake();
        if(button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        button.onClick.RemoveAllListeners();
        HomeSceneManager.Instance.Object.SetDrag();
    }
    protected override void OnSnapped(GameObject obj)
    {
        if (currentPlayer != null) return;
        if (animatorClipSelector.HasAnimation)
        {
            currentPlayer = animatorClipSelector.PlayClipWithCancell();
            StartCoroutine(currentPlayer.Play(OnAnimationEnd));
        }
        else
        {
            OnAnimationEnd(true);
        }
    }

    protected override void OnExitSnapped(GameObject obj)
    {
        if(currentPlayer != null)
        {
            currentPlayer.Stop();
        }
    }

    private void OnAnimationEnd(bool success)
    {
        currentPlayer = null;
        if (!success) return;
        SceneManager.Instance.TransitionToScene(scene);
    }
}
