using UnityEngine;
using System;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif


[Serializable]
public class AnimatorTriggerSelector
{
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName;

    public GameObject target => animator == null ? null : animator.gameObject;

    public void ActivateTrigger()
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            target.SetActive(true);
            animator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning("Animator or Trigger name is not set.");
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AnimatorTriggerSelector))]
    public class AnimatorTriggerSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var animatorRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var triggerRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            var animatorProp = property.FindPropertyRelative("animator");
            var triggerProp = property.FindPropertyRelative("triggerName");

            EditorGUI.PropertyField(animatorRect, animatorProp, GUIContent.none);

            Animator animator = (Animator)animatorProp.objectReferenceValue;
            if (animator != null)
            {
                var controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller != null)
                {
                    var triggers = new string[controller.parameters.Length + 1];
                    triggers[0] = "None";
                    int currentIndex = 0;

                    for (int i = 0; i < controller.parameters.Length; i++)
                    {
                        if (controller.parameters[i].type == AnimatorControllerParameterType.Trigger)
                        {
                            triggers[i + 1] = controller.parameters[i].name;
                            if (controller.parameters[i].name == triggerProp.stringValue)
                            {
                                currentIndex = i + 1;
                            }
                        }
                    }

                    int newIndex = EditorGUI.Popup(triggerRect, currentIndex, triggers);
                    triggerProp.stringValue = newIndex > 0 ? triggers[newIndex] : "";
                }
                else
                {
                    EditorGUI.PropertyField(triggerRect, triggerProp, GUIContent.none);
                }
            }
            else
            {
                EditorGUI.PropertyField(triggerRect, triggerProp, GUIContent.none);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }
    }
#endif
}

[Serializable]
public class AnimatorClipSelector
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip selectedClip;

    public GameObject target => animator == null ? null : animator.gameObject;
    public void PlayClip()
    {
        if (animator != null && selectedClip != null)
        {
            target.SetActive(true);
            animator.Play(selectedClip.name);
        }
        else
        {
            Debug.LogWarning("Animator or AnimationClip is not set.");
        }
    }

    public IEnumerator PlayClipAsync()
    {
        if (animator != null && selectedClip != null)
        {
            target.SetActive(true);
            animator.Play(selectedClip.name);
            yield return new WaitForSeconds(selectedClip.length);
        }
        else
        {
            Debug.LogWarning("Animator or AnimationClip is not set.");
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AnimatorClipSelector))]
    public class AnimatorClipSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var animatorRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var clipRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            var animatorProp = property.FindPropertyRelative("animator");
            var clipProp = property.FindPropertyRelative("selectedClip");

            EditorGUI.PropertyField(animatorRect, animatorProp, GUIContent.none);

            Animator animator = (Animator)animatorProp.objectReferenceValue;
            if (animator != null)
            {
                var controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller != null)
                {
                    var clips = controller.animationClips;
                    var clipNames = new string[clips.Length + 1];
                    clipNames[0] = "None";
                    int currentIndex = 0;

                    for (int i = 0; i < clips.Length; i++)
                    {
                        clipNames[i + 1] = clips[i].name;
                        if (clips[i] == clipProp.objectReferenceValue)
                        {
                            currentIndex = i + 1;
                        }
                    }

                    int newIndex = EditorGUI.Popup(clipRect, currentIndex, clipNames);
                    clipProp.objectReferenceValue = newIndex > 0 ? clips[newIndex - 1] : null;
                }
                else
                {
                    EditorGUI.PropertyField(clipRect, clipProp, GUIContent.none);
                }
            }
            else
            {
                EditorGUI.PropertyField(clipRect, clipProp, GUIContent.none);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }
    }
#endif
}