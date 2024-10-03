using SL.Lib;
using System.Collections;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private Coroutine MoveCoroutine;
    public bool IsDragging { get { return isDragging; } }

    [SerializeField] private float screenEdgeThreshold = 0.1f; // 画面端からの閾値

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        if (!isDragging && MoveCoroutine == null)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPosition();
        }
    }

    public void SetDrag(Vector2 scrrenPosition)
    {
        if (!isDragging && MoveCoroutine == null)
        {
            isDragging = true;
            MoveCoroutine = StartCoroutine(EaseIn(ScreenToWorld(scrrenPosition)));
        }
    }
    public void SetDrag() => SetDrag(Input.mousePosition);

    private IEnumerator EaseIn(Vector3 target, float duration = 0.5f)
    {
        Vector3 start = transform.position;
        float yOffset = start.y;
        float totalTime = 0f;
        Vector3 current;
        while (totalTime <= duration) 
        {
            current = VectorExtensions.EaseInTanh(start, target, totalTime / duration);
            current.y = yOffset;
            transform.position = current;
            totalTime += Time.deltaTime;
            yield return null;
        }
        current = target;
        current.y = yOffset;
        transform.position = current;
        isDragging = false;
    }

    private void OnMouseUp()
    {
        if (isDragging && MoveCoroutine == null)
        {
            isDragging = false;
            if (!IsWithinCameraBounds())
            {
                ResetToScreenCenter();
            }
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector3 newPosition = GetMouseWorldPosition() + offset;
            transform.position = newPosition;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        return ScreenToWorld(Input.mousePosition);
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        Vector3 position = screenPosition;
        position.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(position);
    }

    private bool IsWithinCameraBounds()
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.x > screenEdgeThreshold &&
               viewportPosition.x < 1 - screenEdgeThreshold &&
               viewportPosition.y > screenEdgeThreshold &&
               viewportPosition.y < 1 - screenEdgeThreshold &&
               viewportPosition.z > 0; // オブジェクトがカメラの前にあることを確認
    }

    private void ResetToScreenCenter()
    {
        // オブジェクトの現在のワールド座標を取得
        Vector3 objectWorldPosition = transform.position;

        // オブジェクトのスクリーン座標を計算
        Vector3 objectScreenPosition = mainCamera.WorldToScreenPoint(objectWorldPosition);

        // 画面中央のスクリーン座標を計算
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, objectScreenPosition.z);

        // 画面中央のワールド座標を計算（オブジェクトの元の深度を維持）
        Vector3 worldCenter = mainCamera.ScreenToWorldPoint(screenCenter);

        // オブジェクトを新しい位置に移動
        transform.position = worldCenter;
    }
}