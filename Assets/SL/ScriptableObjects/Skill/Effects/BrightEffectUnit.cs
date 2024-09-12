using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BrightEffect", menuName = "SkillSystem/Effects/BrightEffect")]
public class BrightEffectUnit : EffectUnit
{
    public enum BrightMode
    {
        OneShot,
        Increase
    }

    public BrightMode brightMode = BrightMode.OneShot;
    public float baseValue;
    public float duration = 5f;
    public float decreaseDuration = 2f;
    public float brightExtraMPCost = 0.1f;
    private float LocalExtraIntensity = 0f;
    public override IEnumerator ApplyEffect(PlayerController player, int level, KeyCode triggerKey)
    {
        Success = true;
        LocalExtraIntensity = 0f;
        if (brightMode == BrightMode.OneShot)
        {
            UpdateLocalExtraLight(player, baseValue*level);
            player.StartCoroutine(LightDuration(player, duration));
        }
        else if(brightMode == BrightMode.Increase)
        {
            var dCost = brightExtraMPCost * Time.fixedDeltaTime;
            var dBrh = baseValue * level * Time.fixedDeltaTime;
            while (Input.GetKeyDown(triggerKey)&&player.MP > dCost) 
            {
                DeltaUpdateLocalExtraLight(player, dBrh);
                player.MP -= dCost;
                yield return new WaitForNextPlayingFrame();
            }
            player.StartCoroutine(LightDecrease(player, decreaseDuration));
            while (Input.GetKeyDown(triggerKey))
            {
                yield return null;
            }
        }
        
    }

    private void UpdateLocalExtraLight(PlayerController player,float localExtraIntensity) 
    {
        player.ExtraIntensity += - LocalExtraIntensity + localExtraIntensity;
        LocalExtraIntensity = localExtraIntensity;
    }
    private void DeltaUpdateLocalExtraLight(PlayerController player, float deltaLocalExtraIntensity)
    {
        UpdateLocalExtraLight(player, LocalExtraIntensity+deltaLocalExtraIntensity);
    }

    private IEnumerator LightDuration(PlayerController player,float duration)
    {
        yield return new WaitForGameSeconds(duration);
        yield return LightDecrease(player, decreaseDuration);
    }

    private IEnumerator LightDecrease(PlayerController player,float duration)
    {
        var dDec = LocalExtraIntensity / duration;
        while(LocalExtraIntensity > 0)
        {
            DeltaUpdateLocalExtraLight(player, -dDec * Time.fixedDeltaTime);
            yield return new WaitForNextPlayingFrame();
        }
        UpdateLocalExtraLight(player, 0f);
    }

}
