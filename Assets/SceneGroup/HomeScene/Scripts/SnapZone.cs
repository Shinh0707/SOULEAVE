using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class SnapZone : MonoBehaviour
{
    private float snapDistance = 1f;
    public float SnapDistance
    {
        get { return snapDistance; }
        set { 
            snapDistance = value;
            Collider.radius = value;
        }
    }

    [SerializeField]
    private float _snapDistance = 1f;

    private SphereCollider Collider => GetComponent<SphereCollider>();

    private void OnValidate()
    {
        if (_snapDistance != snapDistance)
        {
            SnapDistance = _snapDistance;
        }
    }

    public UnityAction<GameObject> onObjectSnapped;

    private void OnTriggerStay(Collider other)
    {
        DraggableObject draggable = other.GetComponent<DraggableObject>();
        if (draggable != null && !draggable.IsDragging)
        {
            float distance = Vector2.Distance(transform.position, other.transform.position);
            if (distance <= snapDistance)
            {
                SnapObject(other.gameObject);
            }
        }
    }

    private void SnapObject(GameObject obj)
    {
        obj.transform.position = transform.position;
        OnObjectSnapped(obj);
        onObjectSnapped?.Invoke(obj);
    }

    protected virtual void OnObjectSnapped(GameObject obj)
    {
    }
}