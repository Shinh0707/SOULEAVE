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

    [SerializeField] private float screenEdgeThreshold = 0.1f; // ��ʒ[�����臒l

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