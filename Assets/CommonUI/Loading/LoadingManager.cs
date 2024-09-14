using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LoadingManager : SingletonMonoBehaviour<LoadingManager>
{
    private GameObject LoadingObject;

    public void StartLoading()
    {
        if(LoadingObject == null)
        {
            LoadingObject = Instantiate(LoadingManagerData.Instance.LoadingObject, transform);
        }
        LoadingObject.SetActive(true);
    }

    public void EndLoading()
    {
        if(LoadingObject != null)
        {
            LoadingObject.SetActive(false);
        }
    }
}
