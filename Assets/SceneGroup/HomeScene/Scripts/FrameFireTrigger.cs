using SL.Lib;
using SL.SLGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameFireTrigger : SnapZone
{
    [SerializeField] private AnimatorClipSelector clipSelector;
    [SerializeField] private Vector2 afterSnappedPosition;
    [SerializeField] private float gizmoSize = 0.2f; // Gizmo �̃T�C�Y��ݒ肷�邽�߂̕ϐ���ǉ�

    private bool animating = false;

    protected override void OnSnapped(GameObject obj)
    {
        if (!animating)
        {
            StartCoroutine(PlayAnimation(obj));
        }
    }

    private IEnumerator PlayAnimation(GameObject obj)
    {
        animating = true;
        obj.SetActive(false);
        yield return clipSelector.PlayClipAsync();
        clipSelector.target.SetActive(false);
        obj.transform.position = (transform as RectTransform).ConvertToWorldPositionInCamera(relateCanvas, targetCamera, targetCamera.Depth(obj.transform.position), afterSnappedPosition); // �I�u�W�F�N�g�� afterSnappedPosition �Ɉړ�
        obj.SetActive(true);
        animating = false;
    }

    // �I������ Gizmo ��`�悷�郁�\�b�h��ǉ�
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        GizmosHelper.GizmosForRectTransform(transform as RectTransform, () =>
        {
            // ���݂� Gizmo �̐F��ۑ�
            Color oldColor = Gizmos.color;

            // Gizmo �̐F��ݒ�
            Gizmos.color = Color.blue;

            // afterSnappedPosition �̈ʒu�ɋ��̂� Gizmo ��`��
            Gizmos.DrawSphere(afterSnappedPosition, gizmoSize);

            // ����`�悵�Č��݂̈ʒu�� afterSnappedPosition ������
            Gizmos.DrawLine(Vector3.zero,afterSnappedPosition);

            // Gizmo �̐F�����ɖ߂�
            Gizmos.color = oldColor;
        });
        
        
    }
}