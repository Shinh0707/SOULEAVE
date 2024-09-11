using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using SL.Lib;

public class SkillTree : SingletonScriptableObject<SkillTree>
{
    public List<Skill> Skills;

    [NonSerialized]
    private Dictionary<string, int> skillTable = new Dictionary<string, int>();
    [NonSerialized]
    private string[] skillNames;

    public Skill GetSkill(string skillName)
    {
        if (skillTable.ContainsKey(skillName))
        {
            if (skillTable[skillName] == -1)
            {
                return null;
            }
            return Skills[skillTable[skillName]];
        }
        for(int i = 0; i < Skills.Count;i++)
        {
            if (Skills[i].SkillName == skillName)
            {
                skillTable[skillName] = i;
                return Skills[i];
            }
        }
        skillTable[skillName] = -1;
        return null;
    }
    public Skill GetSkill(SelectableSkillName skillName) => GetSkill(skillName.skillName);
    
    public void SetSkillActivate(string skillName, bool value)
    {
        if (skillTable.ContainsKey(skillName) && Skills[skillTable[skillName]].isUnlocked)
        {
            Skills[skillTable[skillName]].isActivated = value;   
        }
    }
    public void SetSkillActivate(SelectableSkillName skillName, bool value) => SetSkillActivate(skillName.skillName, value);
    public void SetSkillToggleActivate(string skillName)
    {
        if (skillTable.ContainsKey(skillName) && Skills[skillTable[skillName]].isUnlocked)
        {
            Skills[skillTable[skillName]].isActivated ^= true;
        }
    }
    public void SetSkillToggleActivate(SelectableSkillName skillName) => SetSkillToggleActivate(skillName.skillName);

    public IReadOnlyList<string> GetAllSkillNames()
    {
        return Array.AsReadOnly(Skills.Select(x => x.SkillName).ToArray());
    }

    private void LoadSkillTree()
    {

    }

    private void SaveCurrentSkillTree()
    {

    }
    public bool CanUpgradeSkill(SelectableSkillName skillName, int availableLifePoints)
    {
        Skill skill = GetSkill(skillName.skillName);
        if (skill == null || skill.currentLevel >= skill.data.maxLevel) return false;

        bool requirementsMet = skill.data.IsUnlockable();

        int upgradeCost = skill.GetUpgradeCost();
        return requirementsMet && availableLifePoints >= upgradeCost;
    }

    public bool UpgradeSkill(SelectableSkillName skillName, ref int availableLifePoints)
    {
        if (CanUpgradeSkill(skillName, availableLifePoints))
        {
            int skillIndex = skillTable[skillName.skillName];
            int upgradeCost = Skills[skillIndex].GetUpgradeCost();
            Skills[skillIndex].isUnlocked = true;
            Skills[skillIndex].currentLevel++;
            availableLifePoints -= upgradeCost;
            return true;
        }
        return false;
    }
}