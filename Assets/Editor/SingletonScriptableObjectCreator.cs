using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;

public class SingletonScriptableObjectCreator
{
    [MenuItem("Tools/Create Singleton ScriptableObjects")]
    public static void CreateSingletonScriptableObjects()
    {
        var singletonTypes = Assembly.GetAssembly(typeof(SingletonScriptableObject<>))
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(SingletonScriptableObject<>))
            .ToList();

        foreach (var type in singletonTypes)
        {
            var resourcePath = "Assets/Resources/Parameters/" + type.Name + ".asset";
            if (!File.Exists(resourcePath))
            {
                var instance = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(instance, resourcePath);
                Debug.Log($"Created {type.Name} at {resourcePath}");
            }
            else
            {
                Debug.Log($"{type.Name} already exists at {resourcePath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}