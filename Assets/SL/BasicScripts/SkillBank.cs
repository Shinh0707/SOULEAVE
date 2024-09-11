using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SL.Lib;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class SkillBank
{
    [SerializeField]
    private SelectableSkillName[] AssignedSkills;
    [SerializeField]
    private bool[] AssignedFlag;

    public void AssignSkill(int bankNo, SelectableSkillName targetSkillName)
    {
        if (bankNo < 0 || bankNo >= AssignedSkills.Length) return;

        Skill targetSkill = SkillTree.Instance.GetSkill(targetSkillName);
        if (!targetSkill.data.HasManualEffects())
        {
            SkillTree.Instance.SetSkillActivate(targetSkillName, true);
            return;
        }

        int existingIndex = Array.IndexOf(AssignedSkills, targetSkillName);
        if (existingIndex != -1)
        {
            AssignedFlag[existingIndex] = false;
        }

        AssignedSkills[bankNo] = targetSkillName;
        AssignedFlag[bankNo] = true;
    }

    public bool CanAssign(int bankNo, SelectableSkillName targetSkillName)
    {
        if (bankNo < 0 || bankNo >= AssignedSkills.Length) return false;
        if (Array.IndexOf(AssignedSkills, targetSkillName) != -1) return true;
        return Assigned < PlayerStatus.Instance.PlayerParameter.MaxSkillBank;
    }

    public int GetSkillBankNo(SelectableSkillName skillName)
    {
        return Array.IndexOf(AssignedSkills, skillName);
    }

    public int[] AssignedBankNos => Enumerable.Range(0, AssignedSkills.Length)
                                              .Where(i => AssignedFlag[i])
                                              .ToArray();

    public int Assigned => AssignedFlag.Count(f => f);

    public SelectableSkillName GetSkillName(int bankNo)
    {
        if (bankNo >= 0 && bankNo < AssignedSkills.Length && AssignedFlag[bankNo])
        {
            return AssignedSkills[bankNo];
        }
        return null;
    }

    public bool TryGetSkillName(int bankNo, out SelectableSkillName skillName)
    {
        if (bankNo >= 0 && bankNo < AssignedSkills.Length && AssignedFlag[bankNo])
        {
            skillName = AssignedSkills[bankNo];
            return true;
        }
        skillName = null;
        return false;
    }

    public bool RemoveSkill(int bankNo)
    {
        if (bankNo >= 0 && bankNo < AssignedSkills.Length && AssignedFlag[bankNo])
        {
            SelectableSkillName skillName = AssignedSkills[bankNo];
            Skill skill = SkillTree.Instance.GetSkill(skillName);
            SkillTree.Instance.SetSkillActivate(skillName, false);
            AssignedFlag[bankNo] = false;
            AssignedSkills[bankNo] = null;
            return true;
        }
        return false;
    }

    public bool RemoveSkill(SelectableSkillName targetSkillName)
    {
        int index = Array.IndexOf(AssignedSkills, targetSkillName);
        if (index != -1 && AssignedFlag[index])
        {
            return RemoveSkill(index);
        }
        return false;
    }

    public Dictionary<KeyCode, SkillManager> GetSkills()
    {
        return Enumerable.Range(0, AssignedSkills.Length)
                         .Where(i => AssignedFlag[i])
                         .ToDictionary(
                             i => PlayerStatus.Instance.GetSkillKeyCode(i),
                             i => new SkillManager(SkillTree.Instance.GetSkill(AssignedSkills[i].skillName))
                         );
    }

    public bool IsSkillAssigned(SelectableSkillName skillName)
    {
        Skill skill = SkillTree.Instance.GetSkill(skillName);
        if (!skill.isUnlocked || !skill.isActivated)
        {
            return false;
        }

        if (!skill.data.HasManualEffects()) return true;
        int bankNo = Array.IndexOf(AssignedSkills, skillName);
        return (bankNo != -1)  && AssignedFlag[bankNo];
    }

    public void ClearAll()
    {
        int maxSkillBank = PlayerStatus.Instance.PlayerParameter.MaxSkillBank;
        AssignedSkills = new SelectableSkillName[maxSkillBank];
        AssignedFlag = new bool[maxSkillBank];
    }

    public override string ToString()
    {
        return $"[{string.Join(",", AssignedSkills.Select((s, i) => AssignedFlag[i] ? s.ToString() : "Empty"))}]";
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SkillBank))]
    public class SkillBankPropertyDrawer : PropertyDrawer
    {
        private const float LineHeight = 20f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // SkillBank 全体のラベルを描画
            position.height = LineHeight;
            EditorGUI.LabelField(position, label);
            position.y += LineHeight + Spacing;

            SerializedProperty assignedSkillsProp = property.FindPropertyRelative("AssignedSkills");
            SerializedProperty assignedFlagProp = property.FindPropertyRelative("AssignedFlag");

            // PlayerStatus.Instance.PlayerParameter.MaxSkillBankの値を取得
            int maxSkillBank = GetMaxSkillBank();

            // 配列のサイズを調整
            while (assignedSkillsProp.arraySize < maxSkillBank)
            {
                assignedSkillsProp.InsertArrayElementAtIndex(assignedSkillsProp.arraySize);
                assignedFlagProp.InsertArrayElementAtIndex(assignedFlagProp.arraySize);
                assignedFlagProp.GetArrayElementAtIndex(assignedFlagProp.arraySize - 1).boolValue = false;
            }

            for (int i = 0; i < maxSkillBank; i++)
            {
                SerializedProperty skillProp = assignedSkillsProp.GetArrayElementAtIndex(i);
                SerializedProperty flagProp = assignedFlagProp.GetArrayElementAtIndex(i);

                Rect lineRect = new Rect(position.x, position.y, position.width, LineHeight);

                // チェックボックスを描画
                Rect checkboxRect = new Rect(lineRect.x, lineRect.y, 20, lineRect.height);
                bool isAssigned = flagProp.boolValue;
                bool newIsAssigned = EditorGUI.Toggle(checkboxRect, isAssigned);

                if (newIsAssigned != isAssigned)
                {
                    flagProp.boolValue = newIsAssigned;
                    if (!newIsAssigned)
                    {
                        // AssignedFlagがfalseになった場合、AssignedSkillsをリセット
                        skillProp.objectReferenceValue = null;
                    }
                }

                // スキル名とプロパティを描画
                if (isAssigned)
                {
                    Rect skillRect = new Rect(lineRect.x + 25, lineRect.y, lineRect.width - 25, lineRect.height);
                    EditorGUI.PropertyField(skillRect, skillProp, new GUIContent($"Skill {i}"));
                }
                else
                {
                    Rect labelRect = new Rect(lineRect.x + 25, lineRect.y, lineRect.width - 25, lineRect.height);
                    EditorGUI.LabelField(labelRect, $"Skill {i} - Not Assigned");
                }

                position.y += LineHeight + Spacing;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int maxSkillBank = GetMaxSkillBank();
            return (LineHeight + Spacing) * (maxSkillBank + 1);
        }

        private int GetMaxSkillBank()
        {
            // エディタ上でPlayerStatus.Instance.PlayerParameter.MaxSkillBankの値を取得する
            // 注意: これはランタイムではなくエディタ上での処理なので、適切に処理する必要があります
            var playerStatusType = typeof(PlayerStatus);
            var instanceProperty = playerStatusType.GetProperty("Instance");
            if (instanceProperty != null)
            {
                var instance = instanceProperty.GetValue(null);
                if (instance != null)
                {
                    var playerParameterProperty = playerStatusType.GetProperty("PlayerParameter");
                    if (playerParameterProperty != null)
                    {
                        var playerParameter = playerParameterProperty.GetValue(instance);
                        if (playerParameter != null)
                        {
                            var maxSkillBankField = playerParameter.GetType().GetField("MaxSkillBank");
                            if (maxSkillBankField != null)
                            {
                                return (int)maxSkillBankField.GetValue(playerParameter);
                            }
                        }
                    }
                }
            }

            // 値を取得できない場合のデフォルト値
            return 4;
        }
    }
#endif
}

[Serializable]
public class SelectableSkillName
{
    public string skillName;

    public static implicit operator SelectableSkillName(string skillName) => new(){ skillName = skillName };
    public static implicit operator SelectableSkillName(Skill skill) => new() { skillName = skill.SkillName };
    public static implicit operator SelectableSkillName(SkillData skilldata) => new() { skillName = skilldata.skillName };

    public override int GetHashCode()
    {
        return skillName.GetHashCode();
    }

    public override string ToString()
    {
        return skillName;
    }

    public override bool Equals(object obj)
    {
        return ToString().Equals(obj.ToString());
    }

    public Skill Skill => SkillTree.Instance.GetSkill(skillName);
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SelectableSkillName))]
public class SelectableSkillNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var skillNameProp = property.FindPropertyRelative("skillName");
        var allSkillNames = SkillTree.Instance.GetAllSkillNames().ToArray();
        int currentIndex = Array.IndexOf(allSkillNames, skillNameProp.stringValue);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int newIndex = EditorGUI.Popup(position, currentIndex, allSkillNames);
        if (currentIndex != newIndex && newIndex != -1)
        {
            skillNameProp.stringValue = allSkillNames[newIndex];
        }

        EditorGUI.EndProperty();
    }
}
#endif
