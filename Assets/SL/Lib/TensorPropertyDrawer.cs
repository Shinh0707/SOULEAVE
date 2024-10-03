
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using SL.Lib;

namespace SL.Lib
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Tensor<>))]
    public class TensorPropertyDrawer : PropertyDrawer
    {
        private int axis1 = 0;
        private int axis2 = 1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var shapeProperty = property.FindPropertyRelative("_shape");
            if (shapeProperty == null || shapeProperty.arraySize == 0)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Invalid Tensor"));
                EditorGUI.EndProperty();
                return;
            }

            int[] shape = new int[shapeProperty.arraySize];
            for (int i = 0; i < shape.Length; i++)
            {
                shape[i] = shapeProperty.GetArrayElementAtIndex(i).intValue;
            }

            int ndim = shape.Length;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var axisRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            var axis = EditorGUI.Vector2IntField(axisRect, "Axis", new(axis1, axis2));
            if (EditorGUI.EndChangeCheck())
            {
                axis1 = Mathf.Clamp(axis.x, 0, ndim - 1);
                axis2 = Mathf.Clamp(axis.y, 0, ndim - 1);
            }

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            int rows = shape[axis1];
            int cols = shape[axis2];

            float cellWidth = Mathf.Min(position.width / cols, 50);
            float cellHeight = EditorGUIUtility.singleLineHeight;

            var dataProperty = property.FindPropertyRelative("_data");
            var stridesProperty = property.FindPropertyRelative("_strides");

            if (dataProperty == null || stridesProperty == null)
            {
                EditorGUI.LabelField(position, "Data or Strides not found");
                EditorGUI.EndProperty();
                return;
            }

            int[] strides = new int[stridesProperty.arraySize];
            for (int i = 0; i < strides.Length; i++)
            {
                strides[i] = stridesProperty.GetArrayElementAtIndex(i).intValue;
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Rect cellRect = new Rect(position.x + j * cellWidth, position.y + i * cellHeight, cellWidth, cellHeight);
                    int[] indices = new int[ndim];
                    indices[axis1] = i;
                    indices[axis2] = j;

                    int flatIndex = 0;
                    for (int k = 0; k < ndim; k++)
                    {
                        flatIndex += indices[k] * strides[k];
                    }

                    var element = dataProperty.GetArrayElementAtIndex(flatIndex);

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(cellRect, element, new GUIContent(""));
                    if (EditorGUI.EndChangeCheck())
                    {
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var shapeProperty = property.FindPropertyRelative("_shape");
            if (shapeProperty == null || shapeProperty.arraySize == 0) return EditorGUIUtility.singleLineHeight;

            int rows = shapeProperty.GetArrayElementAtIndex(axis1).intValue;

            return EditorGUIUtility.singleLineHeight * (rows + 1) + EditorGUIUtility.standardVerticalSpacing * rows;
        }
    }
#endif
}