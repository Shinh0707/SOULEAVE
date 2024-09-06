using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Skill
{
    public SkillData data;
    public int currentLevel;
    public bool isEnabled;

    public bool Success => data.Success;
    public Skill(SkillData data)
    {
        this.data = data;
        this.currentLevel = 0;
    }

    public bool CanUse(PlayerController player) => data.CanUse(player) && isEnabled;

    public IEnumerator Use(PlayerController player, KeyCode triggerKey) => data.ApplySkillEffects(player, currentLevel, triggerKey);

    public void ApplyPassiveEffects(ref PlayerStatus status, PlayerStatus baseStatus)
    {
        if (isEnabled)
        {
            data.ApplyPassiveEffects(ref status, baseStatus);
        }
    }

    public void ApplyMultiplicativeEffects(ref PlayerStatus status, PlayerStatus baseStatus)
    {
        if (isEnabled)
        {
            data.ApplyMultiplicativeEffects(ref status, baseStatus);
        }
    }

    public void ApplyConstantEffects(ref PlayerStatus status, PlayerStatus baseStatus)
    {
        if (isEnabled)
        {
            data.ApplyConstantEffects(ref status, baseStatus);
        }
    }

    public string SkillName => data.skillName;
    public string SkillLevelStr => SL.Lib.AdvancedRomanNumeralConverter.ConvertToRomanNumeralOrDefault(currentLevel);
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