using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL.Lib
{
    public class HomeSceneManager : SceneInitializer<HomeSceneManager>
    {
        [SerializeField] private GameObject Wisp;

        public DraggableObject Object => Wisp.GetComponent<DraggableObject>();
        protected override IEnumerator InitializeScene()
        {
            Wisp.SetActive(true);
            yield return null;
        }
    }
}
