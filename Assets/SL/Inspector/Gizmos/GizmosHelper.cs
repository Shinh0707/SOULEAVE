using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL.SLGizmos
{
    public static class GizmosHelper
    {
        public static void SetGizmosForCanvas(Canvas canvas)
        {
            Gizmos.matrix = canvas.GetCanvasMatrix();
        }

        public static void GizmosForCanvas(Canvas canvas, Action action)
        {
            Matrix4x4 lastMatrix = Gizmos.matrix;
            SetGizmosForCanvas(canvas);
            action?.Invoke();
            Gizmos.matrix = lastMatrix;
        }
        public static void GizmosForRectTransform(RectTransform rectTransform, Action action)
        {
            Matrix4x4 lastMatrix = Gizmos.matrix;
            Gizmos.matrix = rectTransform.localToWorldMatrix;
            action?.Invoke();
            Gizmos.matrix = lastMatrix;
        }
        public static Matrix4x4 GetCanvasMatrix(this Canvas _Canvas)
        {
            RectTransform rectTr = _Canvas.transform as RectTransform;
            Matrix4x4 canvasMatrix = rectTr.localToWorldMatrix;
            canvasMatrix *= Matrix4x4.Translate(-rectTr.sizeDelta / 2);
            return canvasMatrix;
        }
        public static void DrawCameraRect(Camera camera, Vector3 targetPosition)
        {
            if(camera == null) return;
            Vector3 cameraPosition = camera.transform.position;
            Quaternion cameraRotation = camera.transform.rotation;
            // カメラからオブジェクトまでの距離を計算
            float distanceToObject = Vector3.Distance(cameraPosition, targetPosition);

            // カメラの種類に応じて処理を分岐
            if (camera.orthographic)
            {
                DrawOrthographicFrustum(camera, cameraPosition, cameraRotation, distanceToObject);
            }
            else
            {
                DrawPerspectiveFrustum(camera, cameraPosition, cameraRotation, distanceToObject);
            }
        }
        private static void DrawOrthographicFrustum(Camera camera, Vector3 cameraPosition, Quaternion cameraRotation, float distance)
        {
            float orthoHeight = camera.orthographicSize;
            float orthoWidth = orthoHeight * camera.aspect;

            Vector3 topLeft = cameraRotation * new Vector3(-orthoWidth, orthoHeight, distance) + cameraPosition;
            Vector3 topRight = cameraRotation * new Vector3(orthoWidth, orthoHeight, distance) + cameraPosition;
            Vector3 bottomLeft = cameraRotation * new Vector3(-orthoWidth, -orthoHeight, distance) + cameraPosition;
            Vector3 bottomRight = cameraRotation * new Vector3(orthoWidth, -orthoHeight, distance) + cameraPosition;

            Gizmos.color = Color.green;

            // 前面の長方形を描画
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            // カメラから各頂点への線を描画
            Gizmos.DrawLine(cameraPosition, topLeft);
            Gizmos.DrawLine(cameraPosition, topRight);
            Gizmos.DrawLine(cameraPosition, bottomLeft);
            Gizmos.DrawLine(cameraPosition, bottomRight);

            // 後面の長方形を描画（オプション）
            Vector3 farDistance = cameraRotation * new Vector3(0, 0, camera.farClipPlane) + cameraPosition;
            Vector3 offsetToFar = farDistance - cameraPosition;

            Vector3 farTopLeft = topLeft + offsetToFar;
            Vector3 farTopRight = topRight + offsetToFar;
            Vector3 farBottomLeft = bottomLeft + offsetToFar;
            Vector3 farBottomRight = bottomRight + offsetToFar;

            Gizmos.DrawLine(farTopLeft, farTopRight);
            Gizmos.DrawLine(farTopRight, farBottomRight);
            Gizmos.DrawLine(farBottomRight, farBottomLeft);
            Gizmos.DrawLine(farBottomLeft, farTopLeft);
        }

        private static void DrawPerspectiveFrustum(Camera camera, Vector3 cameraPosition, Quaternion cameraRotation, float distance)
        {
            float fov = camera.fieldOfView;
            float aspect = camera.aspect;
            float near = camera.nearClipPlane;
            float far = camera.farClipPlane;

            float frustumHeight = 2.0f * distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * aspect;

            Vector3 topLeft = cameraRotation * new Vector3(-frustumWidth * 0.5f, frustumHeight * 0.5f, distance) + cameraPosition;
            Vector3 topRight = cameraRotation * new Vector3(frustumWidth * 0.5f, frustumHeight * 0.5f, distance) + cameraPosition;
            Vector3 bottomLeft = cameraRotation * new Vector3(-frustumWidth * 0.5f, -frustumHeight * 0.5f, distance) + cameraPosition;
            Vector3 bottomRight = cameraRotation * new Vector3(frustumWidth * 0.5f, -frustumHeight * 0.5f, distance) + cameraPosition;

            Gizmos.color = Color.yellow;

            // 視錐台の輪郭を描画
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            // カメラからの線を描画
            Gizmos.DrawLine(cameraPosition, topLeft);
            Gizmos.DrawLine(cameraPosition, topRight);
            Gizmos.DrawLine(cameraPosition, bottomLeft);
            Gizmos.DrawLine(cameraPosition, bottomRight);

            // Near平面とFar平面を描画（オプション）
            float nearHeight = 2.0f * near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            float nearWidth = nearHeight * aspect;
            float farHeight = 2.0f * far * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            float farWidth = farHeight * aspect;

            Vector3 nearTopLeft = cameraRotation * new Vector3(-nearWidth * 0.5f, nearHeight * 0.5f, near) + cameraPosition;
            Vector3 nearTopRight = cameraRotation * new Vector3(nearWidth * 0.5f, nearHeight * 0.5f, near) + cameraPosition;
            Vector3 nearBottomLeft = cameraRotation * new Vector3(-nearWidth * 0.5f, -nearHeight * 0.5f, near) + cameraPosition;
            Vector3 nearBottomRight = cameraRotation * new Vector3(nearWidth * 0.5f, -nearHeight * 0.5f, near) + cameraPosition;

            Vector3 farTopLeft = cameraRotation * new Vector3(-farWidth * 0.5f, farHeight * 0.5f, far) + cameraPosition;
            Vector3 farTopRight = cameraRotation * new Vector3(farWidth * 0.5f, farHeight * 0.5f, far) + cameraPosition;
            Vector3 farBottomLeft = cameraRotation * new Vector3(-farWidth * 0.5f, -farHeight * 0.5f, far) + cameraPosition;
            Vector3 farBottomRight = cameraRotation * new Vector3(farWidth * 0.5f, -farHeight * 0.5f, far) + cameraPosition;

            Gizmos.DrawLine(nearTopLeft, nearTopRight);
            Gizmos.DrawLine(nearTopRight, nearBottomRight);
            Gizmos.DrawLine(nearBottomRight, nearBottomLeft);
            Gizmos.DrawLine(nearBottomLeft, nearTopLeft);

            Gizmos.DrawLine(farTopLeft, farTopRight);
            Gizmos.DrawLine(farTopRight, farBottomRight);
            Gizmos.DrawLine(farBottomRight, farBottomLeft);
            Gizmos.DrawLine(farBottomLeft, farTopLeft);
        }
    }
}
