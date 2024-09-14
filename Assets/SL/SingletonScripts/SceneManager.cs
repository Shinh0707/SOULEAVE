using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SL.Lib
{
    public class SceneManager : SingletonMonoBehaviour<SceneManager>
    {
        [SerializeField]
        private Scenes prevScene;
        [SerializeField]
        private bool loadPrevScene;

        private void Start()
        {
            if (loadPrevScene)
            {
                TransitionToScene(prevScene);
            }
        }

        private const float MinLoadingTime = 0.5f; // 最小のローディング時間

        public IEnumerator LoadScene(Scenes scene)
        {
            // ローディング開始
            LoadingManager.Instance.StartLoading();

            // 最小ローディング時間を確保
            float startTime = Time.time;

            // シーンの非同期ロード開始
            SceneManagerProperty sceneManagerProperty = SceneManagerProperty.Instance;
            SceneData sceneData = sceneManagerProperty.GetScene(scene);
            if (sceneData == null)
            {
                Debug.LogError($"Scene data not found for {scene}");
                yield break;
            }

            // 現在のアクティブシーンを取得
            Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            // 現在のシーンと読み込もうとしているシーンが同じかチェック
            if (currentScene.path == sceneData)
            {
                Debug.Log($"Scene {scene} is already loaded. Skipping load and proceeding to setup.");
            }
            else
            {
                AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData);
                asyncLoad.allowSceneActivation = false;

                // シーンのロードが90%完了するまで待機
                while (asyncLoad.progress < 0.9f)
                {
                    yield return null;
                }

                // シーンのアクティベーションを許可
                asyncLoad.allowSceneActivation = true;

                // シーンが完全に読み込まれるまで待機
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            // 最小ローディング時間を確保
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < MinLoadingTime)
            {
                yield return new WaitForSeconds(MinLoadingTime - elapsedTime);
            }

            // 新しいシーンの初期化処理を待つ
            yield return SceneInitializerManager.SetupAllPendingInitializers(this);

            // ローディング終了
            LoadingManager.Instance.EndLoading();
        }

        // シーン遷移のためのラッパーメソッド
        public void TransitionToScene(Scenes scene)
        {
            StartCoroutine(LoadScene(scene));
        }
    }
}