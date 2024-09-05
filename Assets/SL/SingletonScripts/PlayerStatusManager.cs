using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatusManager : SingletonMonoBehaviour<PlayerStatusManager>
{
    private Dictionary<KeyCode,Skill> skillBank = new();

    public void UseSkill(PlayerController player,KeyCode key)
    {
        StartCoroutine(skillBank[key].Use(player, key));
    }

    public Dictionary<KeyCode,SkillManager> GetSkills()
    {
        return skillBank.ToDictionary(s => s.Key, s => new SkillManager(s.Value));
    }

    public void AssignSkill(KeyCode key, Skill skill)
    {
        skillBank[key] = skill;
    }
}
