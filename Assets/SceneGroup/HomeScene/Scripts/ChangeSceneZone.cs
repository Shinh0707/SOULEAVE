using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneZone : SnapZone
{
    [SerializeField] private Scenes scene;
    [SerializeField] private AnimatorClipSelector animatorClipSelector;

    private CancellableAnimationPlayer currentPlayer;
    protected override void OnSnapped(GameObject obj)
    {
        if (currentPlayer != null) return;
        if (animatorClipSelector.HasAnimation)
        {
            currentPlayer = animatorClipSelector.PlayClipWithCancell();
            currentPlayer.Play(OnAnimationEnd);
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
