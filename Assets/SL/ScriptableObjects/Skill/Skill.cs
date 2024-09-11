using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Skill
{
    public SkillData data;
    public int currentLevel;
    public bool isUnlocked;
    [SerializeField]
    private bool _isActivated;

    public bool isActivated
    {
        get
        {
            if (data.HasManualEffects())
            {
                return isUnlocked;
            }
            return _isActivated && isUnlocked;
        }
        set
        {
            if (!data.HasManualEffects())
            {
                _isActivated = value;
            }
        }
    }
    public bool Success => data.Success;

    public bool CanUse(PlayerController player) => data.CanUse(player) && isUnlocked;
    public IEnumerator Use(PlayerController player, KeyCode triggerKey)
    {
        if (CanUse(player))
        {
            return data.ApplySkillEffects(player, currentLevel, triggerKey);
        }
        return null;
    }

    public void ApplyPassiveEffects(ref CharacterStatus status, CharacterStatus baseStatus)
    {
        if (isActivated)
        {
            data.ApplyPassiveEffects(ref status, baseStatus);
        }
    }

    public void ApplyMultiplicativeEffects(ref CharacterStatus status, CharacterStatus baseStatus)
    {
        if (isActivated)
        {
            data.ApplyMultiplicativeEffects(ref status, baseStatus);
        }
    }

    public void ApplyConstantEffects(ref CharacterStatus status, CharacterStatus baseStatus)
    {
        if (isActivated)
        {
            data.ApplyConstantEffects(ref status, baseStatus);
        }
    }

    public int GetUpgradeCost()
    {
        return data.GetUpgradeCost(currentLevel + 1);
    }

    public bool CanUpgrade()
    {
        if(!isUnlocked)
        {
            return data.IsUnlockable();
        }
        return currentLevel < data.maxLevel;
    }

    public string SkillName => data.skillName;
    public string SkillLevelStr => SL.Lib.AdvancedRomanNumeralConverter.ConvertToRomanNumeralOrDefault(currentLevel);
    // GetHashCode のオーバーライド
    public override int GetHashCode()
    {
        return SkillName.GetHashCode();
    }

    // Equals のオーバーライド
    public override bool Equals(object obj)
    {
        return Equals(obj as Skill);
    }

    // IEquatable<Skill> の実装
    public bool Equals(Skill other)
    {
        if (other is null)
            return false;

        return this.SkillName == other.SkillName;
    }

    // == 演算子のオーバーロード
    public static bool operator ==(Skill left, Skill right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    // != 演算子のオーバーロード
    public static bool operator !=(Skill left, Skill right)
    {
        return !(left == right);
    }
}

public class SkillManager
{
    public Skill Skill;
    private float _cooldownTimer = 0f;
    private float CoolDownTimer
    {
        get { return _cooldownTimer; }
        set {
            if (_cooldownTimer != value)
            {
                _cooldownTimer = value;
                OnCoolDownChanged?.Invoke(Skill.data.coolDown > 0 ? (_cooldownTimer/ Skill.data.coolDown) :0f);
            }
        }
    }
    public bool IsBusy => Stacks > 0;
    public int Stacks { get; private set; }

    public SkillManager(Skill skill)
    {
        Skill = skill;
    }

    public event Action<float> OnCoolDownChanged;

    public bool CanUse(PlayerController player)
    {
        if (CoolDownTimer <= 0)
        {
            if (IsBusy && Skill.data.singleton)
            {
                return false;
            }
            return Skill.CanUse(player);
        }
        return false;
    }

    public IEnumerator Use(PlayerController player, KeyCode triggerKey)
    {
        if (CanUse(player))
        {
            CoolDownTimer = Skill.data.coolDown;
            Stacks++;
            Debug.Log($"Skill used [{Skill.data.skillName}](level: {Skill.currentLevel})");
            yield return Skill.Use(player, triggerKey);
            if (!Skill.Success)
            {
                CoolDownTimer = 0f;
            }
            Stacks--;
            if (Stacks <= 0) 
            {
                while((!IsBusy)&&(CoolDownTimer > 0))
                {
                    yield return new WaitForGameSeconds(1f);
                    CoolDownTimer -= 1f;
                }
                CoolDownTimer = 0f;
            }
        }
    }
}