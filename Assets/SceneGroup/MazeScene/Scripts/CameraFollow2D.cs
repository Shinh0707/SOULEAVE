using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target; // 追従するターゲット（プレイヤーキャラクター）
    public float smoothSpeed = 0.125f; // カメラの動きのスムーズさ（0から1の間）
    public Vector3 offset = Vector3.zero; // カメラとターゲットの距離
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
                Debug.LogWarning("カメラの追従ターゲットが設定されていません。");
                return;
            }

            // ターゲットの位置にオフセットを加えた目標位置を計算
            Vector3 desiredPosition = target.position + offset;

            // Z軸（奥行き）は変更しない
            desiredPosition.z = transform.position.z;

            // 現在の位置から目標位置へスムーズに移動
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}