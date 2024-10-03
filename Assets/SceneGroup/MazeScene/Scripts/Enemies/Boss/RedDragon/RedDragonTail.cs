using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL.Lib
{
    [Serializable]
    public class StorableTransform
    {
        public Vector2 position = Vector2.zero;
        public bool positionIsDelta = false;
        public Vector3 eulerAngles = Vector3.zero;
        public bool eulerAnglesIsDelta = false;
        public bool atWorld = true;
        public Vector3 scale = Vector3.one;
        public bool scaleIsDelta = false;

        public void Apply(Transform t)
        {
            
            if (atWorld)
            {
                var pos = (Vector2)t.position;
                var rot = t.rotation;
                var scl = t.localScale;
                Apply(ref pos, ref rot, ref scl);
                t.position = pos;
                t.rotation = rot;
                t.localScale = scl;
            }
            else
            {
                var pos = (Vector2)t.localPosition;
                var rot = t.localRotation;
                var scl = t.localScale;
                Apply(ref pos, ref rot, ref scl);
                t.localPosition = pos;
                t.localRotation = rot;
                t.localScale = scl;
            }
        }

        public void Apply(ref Vector2 position, ref Quaternion rotation, ref Vector3 scale)
        {
            if (positionIsDelta)
            {
                position += this.position;
            }
            else
            {
                position = this.position;
            }

            if (eulerAnglesIsDelta)
            {
                rotation *= Quaternion.Euler(this.eulerAngles);
            }
            else
            {
                rotation = Quaternion.Euler(this.eulerAngles);
            }

            if (scaleIsDelta)
            {
                scale += this.scale;
            }
            else
            {
                scale = this.scale;
            }
        }
    }
    public class RedDragonTail : EnemyController
    {
        [SerializeField] private List<StorableTransform> StartTransforms;

        protected override void Think()
        {
            
        }

        private void OnDrawGizmosSelected()
        {
            if (character == null) return;

            Vector2 currentPosition = Position;
            Quaternion currentRotation = character.transform.rotation;
            Vector3 currentScale = character.transform.localScale;

            foreach (StorableTransform t in StartTransforms)
            {
                Vector2 newPosition = currentPosition;
                Quaternion newRotation = currentRotation;
                Vector3 newScale = currentScale;

                // Apply the StorableTransform
                ApplyStorableTransform(t, ref newPosition, ref newRotation, ref newScale);

                // Draw the wireframe cube
                DrawWireframeCube(newPosition, newRotation, newScale);

                // Update current values for the next iteration
                currentPosition = newPosition;
                currentRotation = newRotation;
                currentScale = newScale;
            }
        }

        private void ApplyStorableTransform(StorableTransform t, ref Vector2 position, ref Quaternion rotation, ref Vector3 scale)
        {
            t.Apply(ref position, ref rotation, ref scale);

            // Handle local vs world space
            if (!t.atWorld && character.transform.parent != null)
            {
                position = character.transform.parent.TransformPoint(position);
                rotation = character.transform.parent.rotation * rotation;
            }
        }

        private void DrawWireframeCube(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
            Gizmos.matrix = matrix;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}