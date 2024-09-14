using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target; // �Ǐ]����^�[�Q�b�g�i�v���C���[�L�����N�^�[�j
    public float smoothSpeed = 0.125f; // �J�����̓����̃X���[�Y���i0����1�̊ԁj
    public Vector3 offset = Vector3.zero; // �J�����ƃ^�[�Q�b�g�̋���
    public bool follow = false;

    public void Initialize(Transform target,Vector3 offset)
    {
        this.target = target;
        this.offset = offset;
    }

    public void Initialize(Transform target)
    {
        Initialize(target, Vector3.zero);
    }

    void LateUpdate()
    {
        if (follow)
        {
            if (target == null)
            {
                Debug.LogWarning("�J�����̒Ǐ]�^�[�Q�b�g���ݒ肳��Ă��܂���B");
                return;
            }

            // �^�[�Q�b�g�̈ʒu�ɃI�t�Z�b�g���������ڕW�ʒu���v�Z
            Vector3 desiredPosition = target.position + offset;

            // Z���i���s���j�͕ύX���Ȃ�
            desiredPosition.z = transform.position.z;

            // ���݂̈ʒu����ڕW�ʒu�փX���[�Y�Ɉړ�
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}