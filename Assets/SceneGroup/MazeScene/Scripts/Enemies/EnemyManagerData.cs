using SL.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SL.Lib
{
    public class EnemyManagerData : SingletonScriptableObject<EnemyManagerData>
    {
        
        [SerializeField] private SerializableDictionary<EnemyType, GameObject> EnemyPrefabs = new();

        public GameObject GetEnemyPrefab(EnemyType enemyType) => EnemyPrefabs[enemyType];
        public GameObject GetEnemyPrefab(EnemyTypeSelect enemyTypeSelect) => enemyTypeSelect == EnemyTypeSelect.None ? null : EnemyPrefabs[(EnemyType)SLRandom.GetRandomEnumValue(enemyTypeSelect)];
    }
    [Flags]
    [Serializable]
    public enum EnemyTypeSelect
    {
        None = 0,
        Common = 1,
        Shot = 2
    }
    public enum EnemyType
    {
        Common = 1,
        Shot = 2
    }

    [Serializable]
    public class IEnemyTypeSelect : IComparable<IEnemyTypeSelect>, IConvertible, INumeric<IEnemyTypeSelect>
    {
        public EnemyTypeSelect enemyType = EnemyTypeSelect.None;
        // デフォルトコンストラクタ
        public IEnemyTypeSelect() 
        {
            enemyType = EnemyTypeSelect.None;
        }
        public IEnemyTypeSelect(EnemyTypeSelect enemyType)
        {
            this.enemyType = enemyType;
        }
        public int CompareTo(IEnemyTypeSelect other)
        {
            // 整数型の基底値を取得して比較
            var value1 = Convert.ToInt64(enemyType);
            var value2 = Convert.ToInt64(other);

            return value1.CompareTo(value2);
        }

        public override bool Equals(object obj)
        {
            return enemyType.Equals(obj);
        }

        public override int GetHashCode()
        {
            return enemyType.GetHashCode();
        }

        public override string ToString()
        {
            return enemyType.ToString();
        }

        public TypeCode GetTypeCode()
        {
            return enemyType.GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToBoolean(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToByte(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToChar(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToDateTime(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToDecimal(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToDouble(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToInt64(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToSByte(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToSingle(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return enemyType.ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToType(conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToUInt16(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToUInt32(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)enemyType).ToUInt64(provider);
        }

        public IEnemyTypeSelect Zero() => new();

        public IEnemyTypeSelect One() => new();

        public IEnemyTypeSelect Random(float rate) => new() { enemyType = (EnemyTypeSelect)SLRandom.GetRandomEnumValue<EnemyTypeSelect>() };

        public static implicit operator EnemyTypeSelect(IEnemyTypeSelect enemyTypeSelect) => enemyTypeSelect.enemyType;
        public static implicit operator IEnemyTypeSelect(EnemyTypeSelect enemyTypeSelect) => new() { enemyType = enemyTypeSelect };
        public static implicit operator IEnemyTypeSelect(EnemyType enemyType) => new() { enemyType = (EnemyTypeSelect)enemyType };
        // int からの明示的キャスト
        public static explicit operator IEnemyTypeSelect(int value)
        {
            return new IEnemyTypeSelect { enemyType = (EnemyTypeSelect)value };
        }

        public static bool operator ==(IEnemyTypeSelect left, EnemyTypeSelect right) => left.enemyType == right;
        public static bool operator !=(IEnemyTypeSelect left, EnemyTypeSelect right) => left.enemyType != right;

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(IEnemyTypeSelect))]
        public class IEnemyTypeSelectDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                // プロパティのラベルを非表示にし、EnemyTypeSelectを直接選択しているように見せる
                EditorGUI.BeginProperty(position, GUIContent.none, property);

                SerializedProperty enemyTypeProperty = property.FindPropertyRelative("enemyType");
                // EnumPopupを使用してEnemyTypeSelectを直接表示
                enemyTypeProperty.enumValueIndex = (int)(EnemyTypeSelect)EditorGUI.EnumPopup(position, (EnemyTypeSelect)enemyTypeProperty.enumValueIndex);

                EditorGUI.EndProperty();
            }
        }
#endif
    }
}
