using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL.Lib
{
    public class HomeSceneManager : SceneInitializer<HomeSceneManager>
    {
        [SerializeField] private GameObject Wisp;
        protected override IEnumerator InitializeScene()
        {
            Wisp.SetActive(true);
            yield return null;
        }
    }
}
