using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneData
{
    [SerializeField] private string scenePath;
    public static implicit operator string(SceneData sceneData)
    {
        return sceneData.scenePath;
    }

    public SceneData(string path)
    {
        scenePath = path;
    }
#if UNITY_EDITOR
    // BuildSettings‚É“o˜^‚³‚ê‚Ä‚¢‚é‚©Šm”F
    public bool IsInBuildSettings()
    {
        return System.Array.Exists(EditorBuildSettings.scenes, scene => scene.path == scenePath);
    }

    // BuildSettings‚É“o˜^
    public void AddToBuildSettings()
    {
        if (!IsInBuildSettings())
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    [CustomPropertyDrawer(typeof(SceneData))]
    public class SceneDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty scenePathProp = property.FindPropertyRelative("scenePath");

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProp.stringValue);

            EditorGUI.BeginChangeCheck();
            sceneAsset = EditorGUI.ObjectField(position, label, sceneAsset, typeof(SceneAsset), false) as SceneAsset;

            if (EditorGUI.EndChangeCheck())
            {
                if (sceneAsset != null)
                {
                    scenePathProp.stringValue = AssetDatabase.GetAssetPath(sceneAsset);
                }
                else
                {
                    scenePathProp.stringValue = "";
                }
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}