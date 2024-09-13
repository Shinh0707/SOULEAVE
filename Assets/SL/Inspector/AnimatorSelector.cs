using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;



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
        if (HasAnimation)
        {
            target.SetActive(true);
            animator.Play(selectedClip.name);
        }
        else
        {
            Debug.LogWarning("Animator or AnimationClip is not set.");
        }
    }

    public IEnumerator PlayClipAsync(float normalizedTime = 0.0f, float speed = 1.0f)
    {
        if (HasAnimation)
        {
            target.SetActive(true);
            var lastSpeed = animator.speed;
            animator.speed = speed;
            animator.Play(selectedClip.name, -1, normalizedTime);
            animator.speed = speed;
            yield return new WaitForSeconds(selectedClip.length * (1.0f - normalizedTime) / Mathf.Abs(speed));
            animator.speed = lastSpeed;
        }
        else
        {
            Debug.LogWarning("Animator or AnimationClip is not set.");
        }
    }
    public IEnumerator RewindClipAsync(float normalizedTime = 1.0f, float rewindSpeed = 1.0f)
    {
        if (HasAnimation)
        {
            target.SetActive(true);
            var lastSpeed = animator.speed;
            animator.speed = -rewindSpeed;
            animator.Play(selectedClip.name,-1, normalizedTime);
            animator.speed = -rewindSpeed;
            yield return new WaitForSeconds(selectedClip.length * normalizedTime / Mathf.Abs(rewindSpeed));
            animator.speed = lastSpeed;
        }
        else
        {
            Debug.LogWarning("Animator or AnimationClip is not set.");
        }
    }
    public CancellableAnimationPlayer PlayClipWithCancell(float normalizedStartTime = 0.0f, float playSpeed = 1.0f, float rewindSpeed = 1.0f)
    {
        if (HasAnimation)
        {
            return new CancellableAnimationPlayer(animator, selectedClip, playSpeed, rewindSpeed, normalizedStartTime, 1.0f);
        }
        else
        {
            Debug.LogWarning("Animator or AnimationClip is not set.");
        }
        throw new NotImplementedException();
    }

    public bool HasAnimation => animator != null && selectedClip != null;

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

public class CancellableAnimationPlayer
{
    private Animator m_animator;
    private AnimationClip m_clip;
    private float startNormalizedTime;
    private float endNormalizedTime;
    private float playSpeed;
    private float rewindSpeed;
    private float playDuration;
    private bool isPlaying;
    private bool isCancelled;
    public CancellableAnimationPlayer(Animator animator, AnimationClip clip, float playSpeed, float rewindSpeed, float startNormalizedTime, float endNormalizedTime)
    {
        m_animator = animator;
        m_clip = clip;
        this.playSpeed = playSpeed;
        this.rewindSpeed = rewindSpeed;
        this.startNormalizedTime = startNormalizedTime;
        this.endNormalizedTime = endNormalizedTime;
        playDuration = (endNormalizedTime - startNormalizedTime) * m_clip.length / playSpeed; 
        isPlaying = false;
        isCancelled = false;
    }

    public IEnumerator Play(UnityAction<bool> callback = null)
    {
        float playTime = 0f;
        isPlaying = true;
        isCancelled = false;
        m_animator.Play(m_clip.name, -1, startNormalizedTime);
        m_animator.speed = playSpeed;
        while(playTime < playDuration && isPlaying)
        {
            yield return null;
            playTime += Time.deltaTime;
        }
        if (playTime < playDuration)
        {
            isCancelled = true;
            m_animator.speed = -rewindSpeed;
            float rewindDuration = playTime / (rewindSpeed / playSpeed);
            yield return new WaitForSeconds(rewindDuration);
            callback?.Invoke(false);
        }
        callback?.Invoke(true);
    }

    public void Stop() 
    {
        isPlaying = false;
    }
}