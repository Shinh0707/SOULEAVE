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
                var mapping = levelMap.Where(m => m.x <= skillLevel).OrderBy(m => -m.x).FirstOrDefault();
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
    public SelectableSkillName skillName;
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
        return MPCost.CheckRequired(player.Flux, PlayerStatusManager.MaxMP) && IntensityCost.CheckRequired(player.Intensity, PlayerStatusManager.MaxIntensity);
    }
    public void ApplyCost(PlayerController player)
    {
        Debug.Log($"Applied Cost Flux[{MPCost.useCost.GetActualCost(player.Flux, PlayerStatusManager.MaxMP)}], Intensity[{IntensityCost.useCost.GetActualCost(player.Intensity, PlayerStatusManager.MaxIntensity)}]");
        lastMP = player.Flux;
        lastIntensity = player.Intensity;
        player.Flux -= MPCost.useCost.GetActualCost(player.Flux, PlayerStatusManager.MaxMP);
        player.Intensity -= IntensityCost.useCost.GetActualCost(player.Intensity, PlayerStatusManager.MaxIntensity);
    }
    public void ReturnCost(PlayerController player)
    {
        player.Flux = lastMP;
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

    public bool IsRoot => requirements.Count == 0;

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
    public void ApplyPassiveEffects(ref CharacterStatus status, int skillLevel, CharacterStatus baseStatus)
    {
        foreach (var effect in effects)
        {
            effect.effectUnit.ApplyPassiveEffects(ref status, skillLevel, baseStatus);
        }
    }

    public void ApplyMultiplicativeEffects(ref CharacterStatus status, int skillLevel, CharacterStatus baseStatus)
    {
        foreach (var effect in effects)
        {
            effect.effectUnit.ApplyMultiplicativeEffects(ref status, skillLevel, baseStatus);
        }
    }

    public void ApplyConstantEffects(ref CharacterStatus status, int skillLevel, CharacterStatus baseStatus)
    {
        foreach (var effect in effects)
        {
            effect.effectUnit.ApplyConstantEffects(ref status, skillLevel, baseStatus);
        }
    }

    public int GetUpgradeCost(int currentLevel)
    {
        return baseUpgradeCost * currentLevel;
    }

    public bool CanUse(PlayerController player) => skillUseCostData.CanUse(player) && HasManualEffects();

    public bool IsUnlockable()
    {
        if (requirements.Count == 0) return true;
        foreach(var req in requirements)
        {
            var skill = SkillTree.Instance.GetSkill(req.skillName);
            if (!(skill.isUnlocked && skill.currentLevel >= req.requiredLevel)) return false;
        }
        return true;
    }

    public bool IsRequirement(SelectableSkillName skillName)
    {
        foreach(var req in requirements)
        {
            if(req.skillName.Equals(skillName)) return true;
        }
        return false;
    }
    public bool IsRequirementMet(SelectableSkillName skillName)
    {
        foreach (var req in requirements)
        {
            if (req.skillName.Equals(skillName))
            {
                var skill = SkillTree.Instance.GetSkill(req.skillName);
                if (skill.isUnlocked && skill.currentLevel >= req.requiredLevel) return true;
                return false;
            }
        }
        return false;
    }
}