using SL.Lib;
using SL.SLGizmos;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(RectTransform))]
public class UIWorldCollider : MonoBehaviour
{
    public enum SizeMode
    {
        Constant,
        Width,
        Height,
        Long,
        Short
    }
    [SerializeField] private SizeMode sizeMode = SizeMode.Constant;
    [SerializeField] private float _radius = 1f;
    [SerializeField] private float maxDepth = 10f;
    [SerializeField] private bool useRigitBody = false;
    [SerializeField] protected Camera targetCamera;
    [SerializeField] private Transform colliderParent;

    protected float radius
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            switch (sizeMode)
            {
                case SizeMode.Constant:
                    return _radius;
                case SizeMode.Width:
                    return rectTransform.sizeDelta.x * 0.5f;
                case SizeMode.Height:
                    return rectTransform.sizeDelta.y * 0.5f;
                case SizeMode.Long:
                    return Mathf.Max(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y) * 0.5f;
                case SizeMode.Short:
                    return Mathf.Min(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y) * 0.5f;
            }
            return _radius;
        }
        set
        {
            _radius = value;
            sizeMode = SizeMode.Constant;
        }
    }

    protected GameObject colliderObject;
    private GameObject colliderAdjuster;
    private CapsuleCollider capsuleCollider;
    private Rigidbody capsuleColliderRigitBody;
    private RectTransform rectTransform;
    private Canvas canvas;

    protected Canvas relateCanvas
    {
        get 
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            return canvas;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        CreateCollider();
    }

    private void OnEnable()
    {
        UpdateColliderState();
    }

    private void OnDisable()
    {
        UpdateColliderState();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateCollider();
        }
    }

    private void Update()
    {
        UpdateCollider();
    }

    private void OnDestroy()
    {
        DestroyCollider();
    }

    private void CreateCollider()
    {
        if (colliderObject == null)
        {
            if(colliderParent == null)
            {
                var obj = new GameObject("UI_WorldColliderParent");
                colliderParent = obj.transform;
            }
            colliderAdjuster = new GameObject("UI_WorldCollider_Adjuster");
            colliderAdjuster.transform.SetParent(colliderParent, true);
            colliderObject = new GameObject("UI_WorldCollider");
            colliderObject.transform.SetParent(colliderAdjuster.transform);
            colliderObject.transform.localPosition = Vector3.zero;
            colliderObject.transform.localRotation = Quaternion.identity;
            capsuleCollider = colliderObject.AddComponent<CapsuleCollider>();
            capsuleCollider.direction = 2; // Z-axis
            capsuleCollider.isTrigger = true;
            if (useRigitBody)
            {
                capsuleColliderRigitBody = colliderObject.AddComponent<Rigidbody>();
                capsuleColliderRigitBody.isKinematic = true;
            }

            var handler = colliderObject.AddComponent<CollisionDetector3D>();
            handler.Initialize(OnTriggerStayCallback, DetectCallbackType.TriggerStay);
        }

        UpdateCollider();
    }

    private void UpdateCollider()
    {
        if (colliderObject == null || targetCamera == null) return;
        Vector3 worldPosition = rectTransform.ConvertToWorldPositionInCamera(relateCanvas, targetCamera, maxDepth / 2f);
        colliderAdjuster.transform.position = worldPosition;
        colliderAdjuster.transform.rotation = targetCamera.transform.rotation;

        float worldRadius = GetWorldRadius();
        capsuleCollider.radius = worldRadius;
        capsuleCollider.height = maxDepth;
        capsuleCollider.center = Vector3.zero;
        
        if (useRigitBody) {
            if (capsuleColliderRigitBody == null)
            {
                capsuleColliderRigitBody = capsuleCollider.GetOrAddComponent<Rigidbody>();
                capsuleColliderRigitBody.isKinematic = true;
            }
            if (capsuleColliderRigitBody.IsSleeping())
            {
                capsuleColliderRigitBody.WakeUp();
            }

        }
        else if (capsuleColliderRigitBody != null)
        {
            capsuleColliderRigitBody.Sleep();
        }

        UpdateColliderState();
    }
    private float GetWorldRadius()
    {
        Vector3 screenCenter = targetCamera.ScreenToWorldPoint(rectTransform.position);
        screenCenter.z = 1f;
        Vector3 screenEdge = screenCenter + new Vector3(radius, 0, 0);
        Vector3 worldCenter = targetCamera.ScreenToWorldPoint(screenCenter);
        Vector3 worldEdge = targetCamera.ScreenToWorldPoint(screenEdge);
        return Vector3.Distance(worldCenter, worldEdge);
    }

    private void UpdateColliderState()
    {
        if (colliderAdjuster != null)
        {
            colliderAdjuster.SetActive(isActiveAndEnabled);
        }
    }

    private void DestroyCollider()
    {
        if (colliderAdjuster != null)
        {
            Destroy(colliderObject);
            Destroy(colliderAdjuster);
            colliderAdjuster = null;
            colliderObject = null;
        }
    }

    protected virtual void OnTriggerStayCallback(Collider other)
    {
    }
    protected virtual void OnTriggerExitCallback(Collider other)
    {
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        GizmosHelper.DrawCameraRect(targetCamera, transform.position);
        Gizmos.matrix = rectTransform.localToWorldMatrix;
        // Gizmosの色を設定
        Gizmos.color = Color.yellow;
        // ワイヤーフレームの円（球）を描画
        Gizmos.DrawWireSphere(Vector3.zero, radius);
    }
}