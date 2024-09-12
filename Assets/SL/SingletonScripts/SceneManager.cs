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

        private const float MinLoadingTime = 0.5f; // �ŏ��̃��[�f�B���O����

        public IEnumerator LoadScene(Scenes scene)
        {
            // ���[�f�B���O�J�n
            LoadingManager.Instance.StartLoading();

            // �ŏ����[�f�B���O���Ԃ��m��
            float startTime = Time.time;

            // �V�[���̔񓯊����[�h�J�n
            SceneManagerProperty sceneManagerProperty = SceneManagerProperty.Instance;
            SceneData sceneData = sceneManagerProperty.GetScene(scene);
            if (sceneData == null)
            {
                Debug.LogError($"Scene data not found for {scene}");
                yield break;
            }

            // ���݂̃A�N�e�B�u�V�[�����擾
            Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            // ���݂̃V�[���Ɠǂݍ������Ƃ��Ă���V�[�����������`�F�b�N
            if (currentScene.path == sceneData)
            {
                Debug.Log($"Scene {scene} is already loaded. Skipping load and proceeding to setup.");
            }
            else
            {
                AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData);
                asyncLoad.allowSceneActivation = false;

                // �V�[���̃��[�h��90%��������܂őҋ@
                while (asyncLoad.progress < 0.9f)
                {
                    yield return null;
                }

                // �V�[���̃A�N�e�B�x�[�V����������
                asyncLoad.allowSceneActivation = true;

                // �V�[�������S�ɓǂݍ��܂��܂őҋ@
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            // �ŏ����[�f�B���O���Ԃ��m��
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < MinLoadingTime)
            {
                yield return new WaitForSeconds(MinLoadingTime - elapsedTime);
            }

            // �V�����V�[���̏�����������҂�
            yield return SceneInitializerManager.SetupAllPendingInitializers(this);

            // ���[�f�B���O�I��
            LoadingManager.Instance.EndLoading();
        }

        // �V�[���J�ڂ̂��߂̃��b�p�[���\�b�h
        public void TransitionToScene(Scenes scene)
        {
            StartCoroutine(LoadScene(scene));
        }
    }
}