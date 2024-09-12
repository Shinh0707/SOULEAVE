using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameFireTrigger : SnapZone
{
    [SerializeField] private AnimatorClipSelector clipSelector;
    [SerializeField] private Vector3 afterSnappedPosition;
    [SerializeField] private Color gizmoColor = Color.yellow; // Gizmo の色を設定するための変数を追加
    [SerializeField] private float gizmoSize = 0.2f; // Gizmo のサイズを設定するための変数を追加

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
        obj.transform.position = transform.position + afterSnappedPosition; // オブジェクトを afterSnappedPosition に移動
        obj.SetActive(true);
        animating = false;
    }

    // 選択時に Gizmo を描画するメソッドを追加
    private void OnDrawGizmosSelected()
    {
        // 現在の Gizmo の色を保存
        Color oldColor = Gizmos.color;

        // Gizmo の色を設定
        Gizmos.color = gizmoColor;

        // afterSnappedPosition の位置に球体の Gizmo を描画
        Gizmos.DrawSphere(transform.position + afterSnappedPosition, gizmoSize);

        // 線を描画して現在の位置と afterSnappedPosition を結ぶ
        Gizmos.DrawLine(transform.position, transform.position + afterSnappedPosition);

        // Gizmo の色を元に戻す
        Gizmos.color = oldColor;
    }
}