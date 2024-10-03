using UnityEngine;
using UnityEditor;
using System;

// AnimationTrigger�����̒�`
public class AnimationTriggerAttribute : PropertyAttribute
{
    public string AnimatorPropertyName { get; private set; }

    public AnimationTriggerAttribute(string animatorPropertyName)
    {
        AnimatorPropertyName = animatorPropertyName;
    }
}

#if UNITY_EDITOR
// AnimationTrigger�����̂��߂�PropertyDrawer
[CustomPropertyDrawer(typeof(AnimationTriggerAttribute))]
public class AnimationTriggerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AnimationTriggerAttribute animTriggerAttr = attribute as AnimationTriggerAttribute;

        // �e�̃I�u�W�F�N�g���擾
        UnityEngine.Object targetObject = property.serializedObject.targetObject;

        // Animator�v���p�e�B���擾
        var animatorProperty = property.serializedObject.FindProperty(animTriggerAttr.AnimatorPropertyName);

        if (animatorProperty != null && animatorProperty.objectReferenceValue is Animator animator)
        {
            EditorGUI.BeginProperty(position, label, property);

            // ���݂̒l���擾
            string currentTriggerName = property.stringValue;

            // Animator�R���g���[���[���炷�ׂẴp�����[�^�[���擾
            var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            if (controller != null)
            {
                var parameters = controller.parameters;
                var triggerNames = new string[parameters.Length + 1];
                triggerNames[0] = "None";
                int currentIndex = 0;

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].type == AnimatorControllerParameterType.Trigger)
                    {
                        triggerNames[i + 1] = parameters[i].name;
                        if (parameters[i].name == currentTriggerName)
                        {
                            currentIndex = i + 1;
                        }
                    }
                }

                // �h���b�v�_�E����\��
                int newIndex = EditorGUI.Popup(position, label.text, currentIndex, triggerNames);

                // �V�����l��ݒ�
                if (newIndex != currentIndex)
                {
                    property.stringValue = (newIndex == 0) ? "" : triggerNames[newIndex];
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Animator Controller not found");
            }

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Animator not found");
        }
    }
}
#endif