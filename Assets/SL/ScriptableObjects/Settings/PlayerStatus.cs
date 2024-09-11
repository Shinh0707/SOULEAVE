using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CharacterStatusType
{
    MaxIntensity,
    RestoreIntensityPerSecond,
    MaxMP,
    RestoreMPPerSecond,
    MaxSpeed
}

[Serializable]
public class CharacterStatus
{
    public float MaxIntensity = 2;
    public float RestoreIntensityPerSecond = 0.5f;
    public float MaxMP = 20;
    public float RestoreMPPerSecond = 1;
    public float MaxSpeed = 10;

    public float GetValue(CharacterStatusType type)
    {
        return type switch
        {
            CharacterStatusType.MaxIntensity => MaxIntensity,
            CharacterStatusType.RestoreIntensityPerSecond => RestoreIntensityPerSecond,
            CharacterStatusType.MaxMP => MaxMP,
            CharacterStatusType.RestoreMPPerSecond => RestoreMPPerSecond,
            CharacterStatusType.MaxSpeed => MaxSpeed,
            _ => throw new ArgumentException("Invalid status type")
        };
    }

    public void SetValue(CharacterStatusType type, float value)
    {
        switch (type)
        {
            case CharacterStatusType.MaxIntensity: MaxIntensity = value; break;
            case CharacterStatusType.RestoreIntensityPerSecond: RestoreIntensityPerSecond = value; break;
            case CharacterStatusType.MaxMP: MaxMP = value; break;
            case CharacterStatusType.RestoreMPPerSecond: RestoreMPPerSecond = value; break;
            case CharacterStatusType.MaxSpeed: MaxSpeed = value; break;
            default: throw new ArgumentException("Invalid status type");
        }
    }
}

[Serializable]
public class PlayerParameter
{
    public int LP = 0;
    public int MaxSkillBank = 4;
    public KeyCode[] KeyMapping;
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PlayerParameter))]
    public class PlayerParameterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 主要なプロパティを取得
            var lpProp = property.FindPropertyRelative("LP");
            var maxSkillBankProp = property.FindPropertyRelative("MaxSkillBank");
            var keyMappingProp = property.FindPropertyRelative("KeyMapping");

            // レイアウトの設定
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);

            // LP と MaxSkillBank のフィールドを描画
            EditorGUI.PropertyField(currentRect, lpProp);
            currentRect.y += lineHeight + spacing;
            EditorGUI.PropertyField(currentRect, maxSkillBankProp);
            currentRect.y += lineHeight + spacing;

            // MaxSkillBank の値を取得
            int maxSkillBank = maxSkillBankProp.intValue;

            // KeyMapping 配列のサイズを調整
            if (keyMappingProp.arraySize != maxSkillBank)
            {
                keyMappingProp.arraySize = maxSkillBank;
            }

            // KeyMapping フィールドを描画
            EditorGUI.LabelField(currentRect, "Key Mapping");
            currentRect.y += lineHeight;

            EditorGUI.indentLevel++;
            for (int i = 0; i < maxSkillBank; i++)
            {
                var element = keyMappingProp.GetArrayElementAtIndex(i);
                EditorGUI.PropertyField(currentRect, element, new GUIContent($"Skill {i + 1}"));
                currentRect.y += lineHeight + spacing;
            }
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var maxSkillBankProp = property.FindPropertyRelative("MaxSkillBank");
            int maxSkillBank = maxSkillBankProp.intValue;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            // LP, MaxSkillBank, "Key Mapping" ラベル, 各キーマッピング
            return lineHeight * (3 + maxSkillBank) + spacing * (2 + maxSkillBank);
        }
    }
#endif
}

[Serializable]
public class PlayerStatus : SingletonScriptableObject<PlayerStatus>
{
    public SkillBank SkillBank;
    public CharacterStatus CharacterStatus;
    public PlayerParameter PlayerParameter;

    public KeyCode GetSkillKeyCode(int bankNo) => PlayerParameter.KeyMapping[bankNo];
}