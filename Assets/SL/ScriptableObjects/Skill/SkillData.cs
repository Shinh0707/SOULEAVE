using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum LevelMappingType
{
    Mapping,
    Linear
}

[System.Serializable]
public class LevelMapping
{
    public LevelMappingType mappingType;

    [Tooltip("Used for Mapping type. Each entry maps a skill level to an effect level.")]
    public List<Vector2Int> levelMap = new List<Vector2Int>();

    [Tooltip("Used for Linear type. EffectLevel = (SkillLevel * multiplier) + offset")]
    public float multiplier = 1f;
    public int offset = 0;

    public int GetEffectLevel(int skillLevel)
    {
        switch (mappingType)
        {
            case LevelMappingType.Mapping:
                var mapping = levelMap.FirstOrDefault(m => m.x == skillLevel);
                return mapping != default ? mapping.y : 1;
            case LevelMappingType.Linear:
                return Mathf.RoundToInt((skillLevel * multiplier) + offset);
            default:
                return 1;
        }
    }
}

[System.Serializable]
public class EffectUnitEntry
{
    public EffectUnit effectUnit;
    public LevelMapping levelMapping;
}

[System.Serializable]
public class SkillRequirement
{
    public SkillData requiredSkill;
    public int requiredLevel;
}
public enum CostType
{
    Constant,
    PercentageOfMax,
    PercentageOfCurrent
}

[System.Serializable]
public class CostSetting
{
    public float value;
    public CostType type;

    public float GetActualCost(float currentValue, float maxValue)
    {
        switch (type)
        {
            case CostType.Constant:
                return value;
            case CostType.PercentageOfMax:
                return maxValue * (value / 100f);
            case CostType.PercentageOfCurrent:
                return currentValue * (value / 100f);
            default:
                return 0f;
        }
    }
}

[System.Serializable]
public class ResourceCost
{
    public CostSetting useCost;
    public CostSetting requiredCost;
    public bool useRequiredAsCost = true;

    public float GetRequiredCost(float currentValue, float maxValue)
    {
        return useRequiredAsCost ? useCost.GetActualCost(currentValue, maxValue) : requiredCost.GetActualCost(currentValue, maxValue);
    }

    public bool CheckRequired(float currentValue, float maxValue)
    {
        return currentValue > GetRequiredCost(currentValue, maxValue);
    }
}

[System.Serializable]
public class SkillUseCostData
{
    public ResourceCost MPCost;
    public ResourceCost IntensityCost;

    private float lastMP = 0f;
    private float lastIntensity = 0f;

    public bool CanUse(PlayerController player)
    {
        return MPCost.CheckRequired(player.MP, PlayerStatusManager.MaxMP) && IntensityCost.CheckRequired(player.Intensity, PlayerStatusManager.MaxIntensity);
    }
    public void ApplyCost(PlayerController player)
    {
        Debug.Log($"Applied Cost MP[{MPCost.useCost.GetActualCost(player.MP, PlayerStatusManager.MaxMP)}], Intensity[{IntensityCost.useCost.GetActualCost(player.Intensity, PlayerStatusManager.MaxIntensity)}]");
        lastMP = player.MP;
        lastIntensity = player.Intensity;
        player.MP -= MPCost.useCost.GetActualCost(player.MP, PlayerStatusManager.MaxMP);
        player.Intensity -= IntensityCost.useCost.GetActualCost(player.Intensity, PlayerStatusManager.MaxIntensity);
    }
    public void ReturnCost(PlayerController player)
    {
        player.MP = lastMP;
        player.Intensity = lastIntensity;
    }
}

[CreateAssetMenu(fileName = "SkillData", menuName = "SkillSystem/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite skillImage;
    public string description;
    public SkillUseCostData skillUseCostData;
    public float coolDown = 0f;
    [Tooltip("“¯‚¶ƒXƒLƒ‹”­“®’†‚É”­“®‰Â”\‚©")]
    public bool singleton = true;
    public int baseUpgradeCost;
    public int maxLevel = 5;
    public List<SkillRequirement> requirements;
    public List<EffectUnitEntry> effects;

    public bool Success { get; private set; }

    public bool HasManualEffects()
    {
        foreach(var effect in effects)
        {
            if (!effect.effectUnit.IsOnlyPassive) return true;
        }
        return false;
    }

    public IEnumerator ApplySkillEffects(PlayerController player, int skillLevel, KeyCode triggerKey)
    {
        Success = true;
        skillUseCostData.ApplyCost(player);
        foreach (var effect in effects)
        {
            if (!effect.effectUnit.IsOnlyPassive)
            {
                int effectLevel = effect.levelMapping.GetEffectLevel(skillLevel);
                yield return effect.effectUnit.ApplyEffect(player, effectLevel, triggerKey);
                if (!effect.effectUnit.Success)
                {
                    Success = false;
                    break;
                }
            }
        }
        if (!Success)
        {
            skillUseCostData.ReturnCost(player);
        }
    }
    public void ApplyPassiveEffects(ref PlayerStatus status, PlayerStatus baseStatus)
    {
        foreach (var effect in effects)
        {
            effect.effectUnit.ApplyPassiveEffects(ref status, baseStatus);
        }
    }

    public void ApplyMultiplicativeEffects(ref PlayerStatus status, PlayerStatus baseStatus)
    {
        foreach (var effect in effects)
        {
            effect.effectUnit.ApplyMultiplicativeEffects(ref status, baseStatus);
        }
    }

    public void ApplyConstantEffects(ref PlayerStatus status, PlayerStatus baseStatus)
    {
        foreach (var effect in effects)
        {
            effect.effectUnit.ApplyConstantEffects(ref status, baseStatus);
        }
    }

    public int GetUpgradeCost(int currentLevel)
    {
        return baseUpgradeCost * currentLevel;
    }

    public bool CanUse(PlayerController player) => skillUseCostData.CanUse(player) && HasManualEffects();
}