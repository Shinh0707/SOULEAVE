using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Flags]
public enum DetectCallbackType
{
    TriggerStay = 1,
    TriggerEnter = 3,
    TriggerExit = 5
}

public class CollisionDetector3D : MonoBehaviour
{
    private UnityAction<Collider> _callback;
    private DetectCallbackType detectCallbackType;
    public void Initialize(UnityAction<Collider> callback, DetectCallbackType callbackType)
    {
        _callback = callback;
        detectCallbackType = callbackType;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!detectCallbackType.HasFlag(DetectCallbackType.TriggerStay)) return;
        _callback?.Invoke(other);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!detectCallbackType.HasFlag(DetectCallbackType.TriggerEnter)) return;
        _callback?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (!detectCallbackType.HasFlag(DetectCallbackType.TriggerExit)) return;
        _callback?.Invoke(other);
    }
}
