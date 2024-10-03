using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SL.Lib {

    public class  TitleData
    {
        public static bool titleShowed = false;
    }
    public class Title : SceneInitializer<Title>
    {
        [SerializeField] private DraggableObject Wisp;
        [SerializeField] private GameObject TitleCanvas;
        [SerializeField] private Button StartButton;
        [SerializeField] private AnimatorClipSelector StartTitleAnimation; 

        protected override IEnumerator InitializeScene()
        {
            yield return base.InitializeScene();
            StartButton.onClick.RemoveAllListeners();
            StartButton.interactable = false;
            if (TitleData.titleShowed)
            {
                Wisp.enabled = true;
                TitleCanvas.SetActive(false);
            }
            else
            {
                Wisp.enabled = false;
                TitleCanvas.SetActive(true);
                StartButton.onClick.AddListener(() => StartCoroutine(TitleClicked()));
                StartButton.interactable = true;
            }
        }

        private IEnumerator TitleClicked()
        {
            StartButton.interactable = false;
            yield return StartTitleAnimation.PlayClipAsync();
            OnEndAnimation();
        }
        private void OnEndAnimation()
        {
            Wisp.enabled = true;
            TitleData.titleShowed = true;
            TitleCanvas.SetActive(false);
           
        }
    }
}
