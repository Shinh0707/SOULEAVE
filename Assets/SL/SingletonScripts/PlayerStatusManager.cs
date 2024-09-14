using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatusManager : SingletonMonoBehaviour<PlayerStatusManager>
{
    private CharacterStatus runtimeStatus;

    public CharacterStatus RuntimeStatus => runtimeStatus;
    private CharacterStatus defaultStatus => PlayerStatus.Instance.CharacterStatus;
    public Dictionary<KeyCode, SkillManager> GetSkills()
    {
        return PlayerStatus.Instance.SkillBank.GetSkills();
    }

    public void ResetRuntimeStatus()
    {
        runtimeStatus = defaultStatus;
        foreach (var skillManager in GetSkills().Values)
        {
            skillManager.Skill.ApplyPassiveEffects(ref runtimeStatus, defaultStatus);
        }
        foreach (var skillManager in GetSkills().Values)
        {
            skillManager.Skill.ApplyMultiplicativeEffects(ref runtimeStatus, defaultStatus);
        }
        foreach (var skillManager in GetSkills().Values)
        {
            skillManager.Skill.ApplyConstantEffects(ref runtimeStatus, defaultStatus);
        }
    }
    public float GetStat(CharacterStatusType statType)
    {
        return runtimeStatus.GetValue(statType);
    }

    public static float MaxMP => Instance.GetStat(CharacterStatusType.MaxMP);
    public static float MaxIntensity => Instance.GetStat(CharacterStatusType.MaxIntensity);
    public static float RestoreIntensityPerSecond => Instance.GetStat(CharacterStatusType.RestoreIntensityPerSecond);
    public static float RestoreMPPerSecond => Instance.GetStat(CharacterStatusType.RestoreMPPerSecond);
    public static float MaxSpeed => Instance.GetStat(CharacterStatusType.MaxSpeed);
    public static float InvincibilityDuration => Instance.GetStat(CharacterStatusType.InvincibilityDuration);
}