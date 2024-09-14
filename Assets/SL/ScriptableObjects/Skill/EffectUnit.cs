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

[System.Serializable]
public class StatusFormula
{
    public CharacterStatusType targetStat;
    public OperationType operationType;
    public float value;

    public float Apply(float currentValue, float baseValue)
    {
        return operationType switch
        {
            OperationType.Addition => currentValue + value,
            OperationType.Multiplication => currentValue * value,
            OperationType.Division => currentValue / value,
            OperationType.Constant => value,
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
    public void ApplyPassiveEffects(ref CharacterStatus status, CharacterStatus baseStatus)
    {
        foreach (var effect in passiveEffects.Where(e => e.operationType == OperationType.Addition))
        {
            float currentValue = status.GetValue(effect.targetStat);
            float baseValue = baseStatus.GetValue(effect.targetStat);
            status.SetValue(effect.targetStat, effect.Apply(currentValue, baseValue));
        }
    }

    public void ApplyMultiplicativeEffects(ref CharacterStatus status, CharacterStatus baseStatus)
    {
        foreach (var effect in passiveEffects.Where(e => e.operationType == OperationType.Multiplication || e.operationType == OperationType.Division))
        {
            float currentValue = status.GetValue(effect.targetStat);
            float baseValue = baseStatus.GetValue(effect.targetStat);
            status.SetValue(effect.targetStat, effect.Apply(currentValue, baseValue));
        }
    }

    public void ApplyConstantEffects(ref CharacterStatus status, CharacterStatus baseStatus)
    {
        foreach (var effect in passiveEffects.Where(e => e.operationType == OperationType.Constant))
        {
            float currentValue = status.GetValue(effect.targetStat);
            float baseValue = baseStatus.GetValue(effect.targetStat);
            status.SetValue(effect.targetStat, effect.Apply(currentValue, baseValue));
        }
    }
}
