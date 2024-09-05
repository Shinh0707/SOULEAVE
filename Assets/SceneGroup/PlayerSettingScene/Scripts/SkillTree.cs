using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkillTree : SingletonMonoBehaviour<SkillTree>
{
    public List<Skill> skills;
    private void LoadSkillTree()
    {

    }

    private void SaveCurrentSkillTree()
    {

    }
    public bool CanUpgradeSkill(SkillData skillData, int availableLifePoints)
    {
        Skill skill = skills.Find(s => s.data == skillData);
        if (skill == null || skill.currentLevel >= skill.data.maxLevel) return false;

        bool requirementsMet = skillData.requirements.Count > 0 ? skillData.requirements.All(req =>
        {
            Skill parentSkill = skills.Find(s => s.data == req.requiredSkill);
            return parentSkill != null && parentSkill.currentLevel >= req.requiredLevel;
        }): true;

        int upgradeCost = skillData.GetUpgradeCost(skill.currentLevel + 1);
        return requirementsMet && availableLifePoints >= upgradeCost;
    }

    public bool UpgradeSkill(SkillData skillData, ref int availableLifePoints)
    {
        if (CanUpgradeSkill(skillData, availableLifePoints))
        {
            Skill skill = skills.Find(s => s.data == skillData);
            int upgradeCost = skillData.GetUpgradeCost(skill.currentLevel + 1);
            skill.currentLevel++;
            availableLifePoints -= upgradeCost;
            return true;
        }
        return false;
    }
}
