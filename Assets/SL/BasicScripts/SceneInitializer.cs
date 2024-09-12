using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL.Lib
{
    public enum SceneState
    {
        Setup = 0,
        WaitInitialize,
        Initializing,
        Initialized
    }

    public interface ISceneInitializer
    {
        bool IsInitialized { get; }
        IEnumerator Setup();
    }

    public static class SceneInitializerManager
    {
        private static Dictionary<string, ISceneInitializer> pendingInitializers = new();

        public static string AddPendingInitializer(ISceneInitializer initializer)
        {
            string id = Guid.NewGuid().ToString();
            pendingInitializers.Add(id, initializer);
            return id;
        }

        public static void RemovePendingInitializer(string id)
        {
            if (id != null && pendingInitializers.ContainsKey(id))
            {
                pendingInitializers.Remove(id);
            }
        }

        public static bool HasPendingInitializers()
        {
            return pendingInitializers.Count > 0;
        }

        public static IEnumerator SetupAllPendingInitializers(MonoBehaviour coroutineExecuter)
        {
            var runningCoroutines = new List<Coroutine>();

            foreach (var initializer in pendingInitializers.Values)
            {
                var coroutine = coroutineExecuter.StartCoroutine(initializer.Setup());
                runningCoroutines.Add(coroutine);
            }

            foreach (Coroutine c in runningCoroutines)
            {
                yield return c;
            }

            Debug.Log("All SceneInitializers have been set up.");
        }
    }
    public abstract class SceneInitializer<T> : SceneSingletonMonoBehaviour<T>, ISceneInitializer where T : MonoBehaviour
    {
        public bool IsInitialized => sceneState == SceneState.Initialized;
        protected SceneState sceneState = SceneState.Setup;
        protected virtual float TimeOut { get; set; } = -1;

        private string id = null;

        protected override void Awake()
        {
            base.Awake();
            ResetScene();
            id = SceneInitializerManager.AddPendingInitializer(this);
        }

        protected override void OnLateDestroy()
        {
            SceneInitializerManager.RemovePendingInitializer(id);
        }

        private void FixedUpdate()
        {
            if (IsInitialized)
            {
                OnFixedUpdate();
            }
        }

        private void Update()
        {
            if (IsInitialized)
            {
                OnUpdate();
            }
        }

        protected virtual void OnFixedUpdate() { }
        protected virtual void OnUpdate() { }

        public virtual void ResetScene()
        {
            sceneState = SceneState.Setup;
        }

        public IEnumerator Setup()
        {
            sceneState = SceneState.Setup;
            Debug.Log($"{typeof(T).Name} in Before Initialize");
            BeforeInitialize();
            Debug.Log($"{typeof(T).Name} in LateBeforeInitialize");
            yield return LateBeforeInitialize();
            float executeTime = 0f;
            Debug.Log($"{typeof(T).Name} is {sceneState}");
            while (sceneState <= SceneState.WaitInitialize)
            {
                yield return null;
                if (TimeOut >= 0f)
                {
                    executeTime += Time.deltaTime;
                    if (executeTime > TimeOut) break;
                }
            }
            Debug.Log($"{typeof(T).Name} in InitializeScene");
            yield return InitializeScene();
            sceneState = SceneState.Initialized;
            SceneInitializerManager.RemovePendingInitializer(id);
            Debug.Log($"{typeof(T).Name} is Initialized");
        }

        protected virtual void BeforeInitialize()
        {
            sceneState = SceneState.Initializing;
        }

        protected virtual IEnumerator LateBeforeInitialize()
        {
            yield break;
        }

        protected virtual IEnumerator InitializeScene()
        {
            yield break;
        }
    }
}