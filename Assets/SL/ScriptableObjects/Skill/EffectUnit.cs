using SL.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum OperationType
{
    Addition,
    Multiplication,
    Division,
    Constant
}

[Serializable]
public class ValueMapping
{
    public LevelMappingType mappingType;

    [Tooltip("Used for Mapping type. Each entry maps a skill level to an effect level.")]
    public List<Vector2> levelMap = new();

    [Tooltip("Used for Linear type. EffectLevel = (SkillLevel * multiplier) + offset")]
    public float multiplier = 1f;
    public float offset = 0;

    public float GetEffectVelue(int skillLevel)
    {
        switch (mappingType)
        {
            case LevelMappingType.Mapping:
                var mapping = levelMap.Where(m => m.x <= skillLevel).OrderBy(m => -m.x).FirstOrDefault();
                return mapping != default ? mapping.y : 1;
            case LevelMappingType.Linear:
                return (skillLevel * multiplier) + offset;
            default:
                return 1;
        }
    }
}

[System.Serializable]
public class StatusFormula
{
    public CharacterStatusType targetStat;
    public OperationType operationType;
    public ValueMapping valueMapping;

    private float Value(int effectLevel) => valueMapping.GetEffectVelue(effectLevel);
    public float Apply(int effectLevel,float currentValue, float baseValue)
    {
        return operationType switch
        {
            OperationType.Addition => currentValue + Value(effectLevel),
            OperationType.Multiplication => currentValue * Value(effectLevel),
            OperationType.Division => currentValue / Value(effectLevel),
            OperationType.Constant => Value(effectLevel),
            _ => currentValue
        };
    }
}


public abstract class EffectUnit : ScriptableObject
{
    public bool Success { get; protected set; } = false;
    private bool isOnlyPassiveCached = false;
    [NonSerialized]
    private bool _isOnlyPassive;
    public bool IsOnlyPassive
    {
        get
        {
            if (!isOnlyPassiveCached)
            {
                MethodInfo method = this.GetType().GetMethod("ApplyEffect");
                _isOnlyPassive = method.DeclaringType == typeof(EffectUnit);
            }
            //Debug.Log($"{this.GetType().Name} {(_isOnlyPassive?"is Only Passive":"has Manual Effect")}");
            return _isOnlyPassive;
        }
    }

    public List<StatusFormula> passiveEffects;
    public virtual IEnumerator ApplyEffect(PlayerController player, int level, KeyCode triggerKey)
    {
        Success = true;
        yield return null;
    }
    public void ApplyPassiveEffects(ref CharacterStatus status, int level, CharacterStatus baseStatus)
    {
        foreach (var effect in passiveEffects.Where(e => e.operationType == OperationType.Addition))
        {
            float currentValue = status.GetValue(effect.targetStat);
            float baseValue = baseStatus.GetValue(effect.targetStat);
            status.SetValue(effect.targetStat, effect.Apply(level,currentValue, baseValue));
        }
    }

    public void ApplyMultiplicativeEffects(ref CharacterStatus status, int level, CharacterStatus baseStatus)
    {
        foreach (var effect in passiveEffects.Where(e => e.operationType == OperationType.Multiplication || e.operationType == OperationType.Division))
        {
            float currentValue = status.GetValue(effect.targetStat);
            float baseValue = baseStatus.GetValue(effect.targetStat);
            status.SetValue(effect.targetStat, effect.Apply(level,currentValue, baseValue));
        }
    }

    public void ApplyConstantEffects(ref CharacterStatus status, int level, CharacterStatus baseStatus)
    {
        foreach (var effect in passiveEffects.Where(e => e.operationType == OperationType.Constant))
        {
            float currentValue = status.GetValue(effect.targetStat);
            float baseValue = baseStatus.GetValue(effect.targetStat);
            status.SetValue(effect.targetStat, effect.Apply(level, currentValue, baseValue));
        }
    }
}
