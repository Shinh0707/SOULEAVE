using SL.SLGizmos;
using UnityEngine;
using UnityEngine.Events;

public class SnapZone : UIWorldCollider
{
    public float SnapDistance
    {
        get { return radius; }
        set { radius = value; }
    }

    [SerializeField]
    private float _snapDistance = 1f;

    private void OnValidate()
    {
        if (_snapDistance != SnapDistance)
        {
            SnapDistance = _snapDistance;
        }
    }

    private bool snapped = false;

    protected override void OnTriggerStayCallback(Collider other)
    {
        if (other.TryGetComponent(out DraggableObject draggable)) {
            if (snapped)
            {
                if (draggable.IsDragging)
                {
                    snapped = false;
                    Debug.Log($"Exit Snap {other.gameObject.name}");
                    OnExitSnapped(other.gameObject);
                }
            }
            else
            {
                if (draggable.IsDragging)
                {
                    Debug.Log($"Wait Snap {other.gameObject.name}");
                    OnWaitSnap(other.gameObject);
                }
                else
                {
                    snapped = true;
                    Debug.Log($"Start Snap {other.gameObject.name}");
                    other.gameObject.transform.position = transform.position;
                    OnSnapped(other.gameObject);
                }
            }
        }
    }

    protected override void OnTriggerExitCallback(Collider other)
    {
        if (other.TryGetComponent(out DraggableObject draggable))
        {
            Debug.Log($"End Wait Snap {other.gameObject.name}");
            OnExitWaitSnap(other.gameObject);
        }
    }

    protected virtual void OnSnapped(GameObject obj)
    {
    }
    protected virtual void OnExitSnapped(GameObject obj)
    {
    }
    protected virtual void OnWaitSnap(GameObject obj)
    {
    }
    protected virtual void OnExitWaitSnap(GameObject obj)
    {
    }
}