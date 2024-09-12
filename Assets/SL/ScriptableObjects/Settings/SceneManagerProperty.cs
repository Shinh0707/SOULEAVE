using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SL.Lib
{
    public enum Scenes
    {
        Home,
        Maze,
        SkillTree
    }
    public class SceneManagerProperty : SingletonScriptableObject<SceneManagerProperty>
    {
        [SerializeField] private SerializableDictionary<Scenes, SceneData> _scenes;

        public SceneData GetScene(Scenes scene)
        {
            if(_scenes.ContainsKey(scene)) return _scenes[scene];
            return null;
        }
#if UNITY_EDITOR

        private void AssignAllSceneToBuildSettings()
        {
            foreach(var scene in _scenes.Values)
            {
                scene.AddToBuildSettings();
            }
        }

        private bool AllScenesInBuildSettings()
        {
            foreach (var scene in _scenes.Values)
            {
                if(!scene.IsInBuildSettings())return false;
            }
            return true;
        }


        [CustomEditor(typeof(SceneManagerProperty))]
        public class SceneManagerPropertyEditor : Editor
        {
            private bool? _cachedAllScenesInBuildSettings = null;
            private SerializedProperty _scenesProperty;

            private void OnEnable()
            {
                _scenesProperty = serializedObject.FindProperty("_scenes");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_scenesProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    _cachedAllScenesInBuildSettings = null;
                }

                if (_cachedAllScenesInBuildSettings == null)
                {
                    _cachedAllScenesInBuildSettings = ((SceneManagerProperty)target).AllScenesInBuildSettings();
                }

                if (_cachedAllScenesInBuildSettings == false)
                {
                    if (GUILayout.Button("Assign To Build Settings"))
                    {
                        ((SceneManagerProperty)target).AssignAllSceneToBuildSettings();
                        _cachedAllScenesInBuildSettings = true;
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            [MenuItem("CONTEXT/SceneManagerProperty/Force Assign Scenes")]
            private static void ForceAssignScenes(MenuCommand command)
            {
                SceneManagerProperty data = (SceneManagerProperty)command.context;
                data.AssignAllSceneToBuildSettings();
                EditorUtility.SetDirty(data);
            }
        }
#endif
    }
}