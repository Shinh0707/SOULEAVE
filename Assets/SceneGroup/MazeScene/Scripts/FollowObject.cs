using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] bool LinkActive = false;
    [SerializeField] bool follow = true;

    private void LateUpdate()
    {
        if (follow)
        {
            transform.position = target.position + offset;
            if (LinkActive && target.gameObject.activeSelf != gameObject.activeSelf)
            {
                gameObject.SetActive(target.gameObject.activeSelf);
            }
        }
    }
}
