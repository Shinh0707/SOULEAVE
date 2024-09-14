using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    public bool IsDragging { get { return isDragging; } }

    [SerializeField] private float screenEdgeThreshold = 0.1f; // ��ʒ[�����臒l

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
               viewportPosition.z > 0; // �I�u�W�F�N�g���J�����̑O�ɂ��邱�Ƃ��m�F
    }

    private void ResetToScreenCenter()
    {
        // �I�u�W�F�N�g�̌��݂̃��[���h���W���擾
        Vector3 objectWorldPosition = transform.position;

        // �I�u�W�F�N�g�̃X�N���[�����W���v�Z
        Vector3 objectScreenPosition = mainCamera.WorldToScreenPoint(objectWorldPosition);

        // ��ʒ����̃X�N���[�����W���v�Z
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, objectScreenPosition.z);

        // ��ʒ����̃��[���h���W���v�Z�i�I�u�W�F�N�g�̌��̐[�x���ێ��j
        Vector3 worldCenter = mainCamera.ScreenToWorldPoint(screenCenter);

        // �I�u�W�F�N�g��V�����ʒu�Ɉړ�
        transform.position = worldCenter;
    }
}