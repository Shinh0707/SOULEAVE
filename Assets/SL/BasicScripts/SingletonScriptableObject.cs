using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance = null;

    public static T I
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<T>("Parameters/" + typeof(T).Name);
                if (_instance == null)
                {
                    _instance = CreateInstance();
                }
            }
            return _instance;
        }
    }

    protected virtual void OnEnable()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"Multiple instances of {typeof(T).Name} found. Using the first one.");
        }
    }

    private static T CreateInstance()
    {
        T instance = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
        string resourcesPath = "Assets/Resources/Parameters";
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }
        string assetPath = $"{resourcesPath}/{typeof(T).Name}.asset";
        AssetDatabase.CreateAsset(instance, assetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"{typeof(T).Name} created at {assetPath}");
#endif
        return instance;
    }
}