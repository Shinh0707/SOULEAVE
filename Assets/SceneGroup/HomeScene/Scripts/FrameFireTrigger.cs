using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameFireTrigger : SnapZone
{
    [SerializeField] private AnimatorClipSelector clipSelector;
    [SerializeField] private Vector3 afterSnappedPosition;
    [SerializeField] private Color gizmoColor = Color.yellow; // Gizmo �̐F��ݒ肷�邽�߂̕ϐ���ǉ�
    [SerializeField] private float gizmoSize = 0.2f; // Gizmo �̃T�C�Y��ݒ肷�邽�߂̕ϐ���ǉ�

    private bool animating = false;

    protected override void OnObjectSnapped(GameObject obj)
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
        obj.transform.position = transform.position + afterSnappedPosition; // �I�u�W�F�N�g�� afterSnappedPosition �Ɉړ�
        obj.SetActive(true);
        animating = false;
    }

    // �I������ Gizmo ��`�悷�郁�\�b�h��ǉ�
    private void OnDrawGizmosSelected()
    {
        // ���݂� Gizmo �̐F��ۑ�
        Color oldColor = Gizmos.color;

        // Gizmo �̐F��ݒ�
        Gizmos.color = gizmoColor;

        // afterSnappedPosition �̈ʒu�ɋ��̂� Gizmo ��`��
        Gizmos.DrawSphere(transform.position + afterSnappedPosition, gizmoSize);

        // ����`�悵�Č��݂̈ʒu�� afterSnappedPosition ������
        Gizmos.DrawLine(transform.position, transform.position + afterSnappedPosition);

        // Gizmo �̐F�����ɖ߂�
        Gizmos.color = oldColor;
    }
}