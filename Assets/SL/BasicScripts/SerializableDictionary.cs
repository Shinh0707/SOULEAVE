using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SL.Lib;


#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] private List<SerializableKeyValuePair<TKey, TValue>> list = new List<SerializableKeyValuePair<TKey, TValue>>();
    [NonSerialized] private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
    [NonSerialized] private Dictionary<TValue, TKey> valueDictionary = new Dictionary<TValue, TKey>();

    [SerializeField] private SerializableKeyValuePair<TKey, TValue> editingElement = new SerializableKeyValuePair<TKey, TValue>() { Key = default, Value = default };

    public Dictionary<TKey, TValue> ToDictionary() => new Dictionary<TKey, TValue>(dictionary);

    public void OnBeforeSerialize()
    {
        list = dictionary.Select(kvp => new SerializableKeyValuePair<TKey, TValue>() { Key = kvp.Key, Value = kvp.Value }).ToList();
    }

    public void OnAfterDeserialize()
    {
        dictionary = list.ToDictionary(item => item.Key, item => item.Value);
    }

    public void Add(TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key.Equals(key))
                {
                    list[i] = new SerializableKeyValuePair<TKey, TValue> { Key = key, Value = value };
                    break;
                }
            }
        }
        else
        {
            dictionary.Add(key, value);
            list.Add(new SerializableKeyValuePair<TKey, TValue>() { Key = key, Value = value });
        }
        Debug.Log($"{key}:{value} was added!");
    }

    public TKey[] Keys
    {
        get
        {
            TKey[] keys = new TKey[list.Count];
            for(int i = 0; i < list.Count;i++)
            {
                keys[i] = list[i].Key;
            }
            return keys;
        }
    }

    public TValue[] Values
    {
        get
        {
            TValue[] values = new TValue[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                values[i] = list[i].Value;
            }
            return values;
        }
    }
    public (TKey key,TValue value)[] Items
    {
        get
        {
            (TKey key, TValue value)[] items = new (TKey key, TValue value)[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                items[i] = (list[i].Key,list[i].Value);
            }
            return items;
        }
    }

    public void AddFromEditing()
    {
        Add(editingElement.Key, editingElement.Value);
    }

    public bool Remove(TKey key) => dictionary.Remove(key);

    public TValue this[TKey k]
    {
        get => dictionary[k];
        set => Add(k,value);
    }

    public void Clear()
    {
        dictionary.Clear();
        list.Clear();
    }

    public List<TKey> GetKeys(TValue value)
    {
        List<TKey> result = new();
        foreach(var item in list)
        {
            if (item.Value.Equals(value)) result.Add(item.Key);
        }
        return result;
    }
    public bool TryGetKey(TValue value, out TKey key)
    {
        foreach (var item in list)
        {
            if (item.Value.Equals(value))
            {
                key = item.Key;
                return true;
            }
        }
        key = default;
        return false;
    }

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);
    public bool ContainsValue(TValue value) => dictionary.ContainsValue(value);
    public int Count => dictionary.Count;
    // �V�����ǉ����ꂽToString���\�b�h
    public override string ToString()
    {
        if (Count == 0)
        {
            return "{}";
        }

        var sb = new System.Text.StringBuilder();
        sb.Append("{ ");
        foreach (var kvp in dictionary)
        {
            sb.Append($"[{kvp.Key}] = {kvp.Value}, ");
        }
        sb.Length -= 2; // �Ō�̃J���}�ƃX�y�[�X���폜
        sb.Append(" }");
        return sb.ToString();
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>)), CanEditMultipleObjects]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float LineHeight = 20f;
        private const float Spacing = 2f;
        private bool showEditDialog = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var list = property.FindPropertyRelative("list");
            var editingElement = property.FindPropertyRelative("editingElement");

            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, LineHeight), label);

            for (int i = 0; i < list.arraySize; i++)
            {
                var kvp = list.GetArrayElementAtIndex(i);
                var key = kvp.FindPropertyRelative("Key");
                var value = kvp.FindPropertyRelative("Value");

                float y = position.y + (i + 1) * (LineHeight + Spacing);

                var keyRect = new Rect(position.x, y, position.width * 0.3f, LineHeight);
                var valueRect = new Rect(position.x + position.width * 0.35f, y, position.width * 0.55f, LineHeight);
                var buttonRect = new Rect(position.x + position.width * 0.92f, y, position.width * 0.08f, LineHeight);

                EditorGUI.PropertyField(keyRect, key, GUIContent.none);
                EditorGUI.PropertyField(valueRect, value, GUIContent.none);

                if (GUI.Button(buttonRect, "-"))
                {
                    list.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            var addButtonRect = new Rect(position.x, position.y + (list.arraySize + 1) * (LineHeight + Spacing), position.width * 0.48f, LineHeight);
            if (GUI.Button(addButtonRect, "Add"))
            {
                showEditDialog = true;
            }

            var clearButtonRect = new Rect(position.x + position.width * 0.52f, position.y + (list.arraySize + 1) * (LineHeight + Spacing), position.width * 0.48f, LineHeight);
            if (GUI.Button(clearButtonRect, "Clear"))
            {
                if (EditorUtility.DisplayDialog("Clear Dictionary", "Are you sure you want to clear the dictionary?", "Yes", "No"))
                {
                    list.ClearArray();
                }
            }

            if (showEditDialog)
            {
                ShowEditElementDialog(property, editingElement);
            }

            EditorGUI.EndProperty();
        }

        private void ShowEditElementDialog(SerializedProperty property, SerializedProperty editingElement)
        {
            // �G�f�B�e�B���O�v�f�̍X�V
            editingElement.serializedObject.Update();

            // �_�C�A���O��UI����
            EditorGUILayout.LabelField("Add New Element", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(editingElement.FindPropertyRelative("Key"));
            EditorGUILayout.PropertyField(editingElement.FindPropertyRelative("Value"));
            EditorGUI.indentLevel--;

            // �{�^������̏���
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Confirm"))
            {
                // �m�F�{�^���������ꂽ�ꍇ�̏���
                editingElement.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();

                // ���X�g�ɐV�����v�f��ǉ�
                var list = property.FindPropertyRelative("list");

                // �V�����v�f�����X�g�ɒǉ�
                list.arraySize++;
                var newElement = list.GetArrayElementAtIndex(list.arraySize - 1);

                // �ċA�I�Ƀv���p�e�B���R�s�[
                CopySerializedPropertyRecursive(editingElement, newElement);

                // �ύX�̓K�p
                property.serializedObject.ApplyModifiedProperties();

                // �_�C�A���O�����
                showEditDialog = false;
            }

            if (GUILayout.Button("Cancel"))
            {
                // �L�����Z���{�^���������ꂽ�ꍇ�̏���
                showEditDialog = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        // SerializedProperty���ċA�I�ɃR�s�[����w���p�[���\�b�h
        private void CopySerializedPropertyRecursive(SerializedProperty source, SerializedProperty dest)
        {
            var sourceIterator = source.Copy();
            var destIterator = dest.Copy();
            var sourceEnd = source.GetEndProperty();

            while (sourceIterator.NextVisible(true) && !SerializedProperty.EqualContents(sourceIterator, sourceEnd))
            {
                if (destIterator.NextVisible(true))
                {
                    CopySerializedPropertyValue(sourceIterator, destIterator);
                }
            }
        }

        // �ʂ�SerializedProperty�̒l���R�s�[����w���p�[���\�b�h
        private void CopySerializedPropertyValue(SerializedProperty source, SerializedProperty dest)
        {
            if (source.propertyType != dest.propertyType)
            {
                Debug.LogWarning($"Property type mismatch for {source.propertyPath}. Source: {source.propertyType}, Destination: {dest.propertyType}");
                return;
            }

            try
            {
                switch (source.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        dest.intValue = source.intValue;
                        break;
                    case SerializedPropertyType.Boolean:
                        dest.boolValue = source.boolValue;
                        break;
                    case SerializedPropertyType.Float:
                        if (source.type == "float" && dest.type == "float")
                        {
                            dest.floatValue = source.floatValue;
                        }
                        else
                        {
                            Debug.LogWarning($"Float type mismatch for {source.propertyPath}. Source type: {source.type}, Dest type: {dest.type}");
                        }
                        break;
                    case SerializedPropertyType.String:
                        dest.stringValue = source.stringValue;
                        break;
                    case SerializedPropertyType.Color:
                        dest.colorValue = source.colorValue;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        dest.objectReferenceValue = source.objectReferenceValue;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        dest.exposedReferenceValue = source.exposedReferenceValue;
                        break;
                    case SerializedPropertyType.Enum:
                        if (source.enumValueIndex >= 0 && source.enumValueIndex < dest.enumNames.Length)
                        {
                            dest.enumValueIndex = source.enumValueIndex;
                        }
                        else
                        {
                            dest.enumValueIndex = 0;
                            Debug.LogWarning($"Enum index out of range. Defaulting to 0. Property: {source.propertyPath}");
                        }
                        break;
                    case SerializedPropertyType.Vector2:
                        dest.vector2Value = source.vector2Value;
                        break;
                    case SerializedPropertyType.Vector3:
                        dest.vector3Value = source.vector3Value;
                        break;
                    case SerializedPropertyType.Vector4:
                        dest.vector4Value = source.vector4Value;
                        break;
                    case SerializedPropertyType.Rect:
                        dest.rectValue = source.rectValue;
                        break;
                    case SerializedPropertyType.ArraySize:
                        dest.arraySize = source.arraySize;
                        break;
                    case SerializedPropertyType.Character:
                        dest.intValue = source.intValue;
                        break;
                    case SerializedPropertyType.AnimationCurve:
                        dest.animationCurveValue = source.animationCurveValue;
                        break;
                    case SerializedPropertyType.Bounds:
                        dest.boundsValue = source.boundsValue;
                        break;
                    case SerializedPropertyType.Quaternion:
                        dest.quaternionValue = source.quaternionValue;
                        break;
                    default:
                        if (source.isArray)
                        {
                            dest.arraySize = source.arraySize;
                            for (int i = 0; i < source.arraySize; i++)
                            {
                                CopySerializedPropertyRecursive(source.GetArrayElementAtIndex(i), dest.GetArrayElementAtIndex(i));
                            }
                        }
                        else
                        {
                            CopySerializedPropertyRecursive(source, dest);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error copying property: {source.propertyPath} of type: {source.propertyType}");
                Debug.LogError($"Source type: {source.type}, Destination type: {dest.type}");
                Debug.LogError($"Exception: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }


        // �v���p�e�B�̒l�𕶎���Ƃ��Ď擾����w���p�[���\�b�h
        private string GetPropertyValueString(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return prop.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return prop.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return prop.floatValue.ToString();
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue != null ? prop.objectReferenceValue.ToString() : "null";
                case SerializedPropertyType.ExposedReference:
                    return prop.exposedReferenceValue != null ? prop.exposedReferenceValue.ToString() : "null";
                case SerializedPropertyType.Enum:
                    return prop.enumNames[prop.enumValueIndex];
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    return prop.rectValue.ToString();
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize.ToString();
                case SerializedPropertyType.Character:
                    return ((char)prop.intValue).ToString();
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue != null ? "AnimationCurve" : "null";
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue.ToString();
                case SerializedPropertyType.Gradient:
                    return "Gradient";
                case SerializedPropertyType.Quaternion:
                    return prop.quaternionValue.ToString();
                default:
                    return "Unknown type";
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var list = property.FindPropertyRelative("list");
            return (list.arraySize + 2) * (LineHeight + Spacing) + (showEditDialog ? 5 * LineHeight : 0);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SerializableDictionary<Scenes, SceneData>)), CanEditMultipleObjects]
public class ScenesDictionaryDrawer : SerializableDictionary<Scenes, SceneData>.SerializableDictionaryDrawer { }
[CustomPropertyDrawer(typeof(SerializableDictionary<EnemyType, GameObject>)), CanEditMultipleObjects]
public class EnemyTypeDictionaryDrawer : SerializableDictionary<EnemyType, GameObject>.SerializableDictionaryDrawer { }
// �K�v�ɉ����āA���̋�̓I�Ȍ^�̑g�ݍ��킹�ɑ΂���Drawer��ǉ�
#endif