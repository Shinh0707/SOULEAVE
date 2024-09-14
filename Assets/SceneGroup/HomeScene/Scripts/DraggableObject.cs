using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    public bool IsDragging { get { return isDragging; } }

    [SerializeField] private float screenEdgeThreshold = 0.1f; // 画面端からの閾値

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseUp()
    {
        isDragging = false;
        if (!IsWithinCameraBounds())
        {
            ResetToScreenCenter();
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
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
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