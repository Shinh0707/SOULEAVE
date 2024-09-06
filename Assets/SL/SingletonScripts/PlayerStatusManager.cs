using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatusManager : SingletonMonoBehaviour<PlayerStatusManager>
{
    private Dictionary<KeyCode,Skill> skillBank = new();
    private SkillTree skillTree;
    [SerializeField]
    private PlayerStatus defaultStatus;
    private PlayerStatus runtimeStatus;

    public Dictionary<KeyCode,SkillManager> GetSkills()
    {
        return skillBank.ToDictionary(s => s.Key, s => new SkillManager(s.Value));
    }

    public void AssignSkill(KeyCode key, Skill skill)
    {
        if (skill.data.HasManualEffects())
        {
            skillBank[key] = skill;
        }
        else
        {
            Debug.LogWarning($"{skill} has not manual effects!");
        }
    }

    public void ResetRuntimeStatus()
    {
        runtimeStatus = defaultStatus;
        if (skillTree == null) return;
        foreach (var skill in skillTree.skills) 
        {
            skill.ApplyPassiveEffects(ref runtimeStatus, defaultStatus);
        }
        foreach (var skill in skillTree.skills)
        {
            skill.ApplyMultiplicativeEffects(ref runtimeStatus, defaultStatus);
        }
        foreach (var skill in skillTree.skills)
        {
            skill.ApplyConstantEffects(ref runtimeStatus, defaultStatus);
        }
    }
    public float GetStat(StatusType statType)
    {
        return runtimeStatus.GetValue(statType);
    }

    public static float MaxMP => Instance.GetStat(StatusType.MaxMP);
    public static float MaxIntensity => Instance.GetStat(StatusType.MaxIntensity);
    public static float RestoreIntensityPerSecond => Instance.GetStat(StatusType.RestoreIntensityPerSecond);
    public static float RestoreMPPerSecond => Instance.GetStat(StatusType.RestoreMPPerSecond);
    public static float MaxSpeed => Instance.GetStat(StatusType.MaxSpeed);
}

public enum StatusType
{
    MaxIntensity,
    RestoreIntensityPerSecond,
    MaxMP,
    RestoreMPPerSecond,
    MaxSpeed
}

[Serializable]
public struct PlayerStatus
{
    public float MaxIntensity;
    public float RestoreIntensityPerSecond;
    public float MaxMP;
    public float RestoreMPPerSecond;
    public float MaxSpeed;

    public float GetValue(StatusType type)
    {
        return type switch
        {
            StatusType.MaxIntensity => MaxIntensity,
            StatusType.RestoreIntensityPerSecond => RestoreIntensityPerSecond,
            StatusType.MaxMP => MaxMP,
            StatusType.RestoreMPPerSecond => RestoreMPPerSecond,
            StatusType.MaxSpeed => MaxSpeed,
            _ => throw new ArgumentException("Invalid status type")
        };
    }

    public void SetValue(StatusType type, float value)
    {
        switch (type)
        {
            case StatusType.MaxIntensity: MaxIntensity = value; break;
            case StatusType.RestoreIntensityPerSecond: RestoreIntensityPerSecond = value; break;
            case StatusType.MaxMP: MaxMP = value; break;
            case StatusType.RestoreMPPerSecond: RestoreMPPerSecond = value; break;
            case StatusType.MaxSpeed: MaxSpeed = value; break;
            default: throw new ArgumentException("Invalid status type");
        }
    }
}