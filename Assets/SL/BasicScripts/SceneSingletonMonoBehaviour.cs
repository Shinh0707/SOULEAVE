using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SL.Lib
{
    public abstract class SceneSingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static Dictionary<Scene, T> _instances = new Dictionary<Scene, T>();

        public static T Instance
        {
            get
            {
                Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (!_instances.TryGetValue(currentScene, out T instance))
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        instance = obj.AddComponent<T>();
                    }
                    _instances[currentScene] = instance;
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            Scene currentScene = gameObject.scene;
            if (_instances.TryGetValue(currentScene, out T existingInstance) && existingInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instances[currentScene] = this as T;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene unloadedScene)
        {
            if (gameObject.scene == unloadedScene)
            {
                _instances.Remove(unloadedScene);
                UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
            }
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
            OnLateDestroy();
        }
        protected virtual void OnLateDestroy()
        {
        }
    }
}
