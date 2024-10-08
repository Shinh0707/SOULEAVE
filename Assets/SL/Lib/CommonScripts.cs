using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SL.Lib
{
    public static class ArrayExtensions
    {
        public static void Insert<T>(this T[] src, T insertValue, int insertIndex, T[] dst)
        {
            Array.Copy(src, 0, dst, 0, insertIndex);
            dst[insertIndex] = insertValue;
            Array.Copy(src, insertIndex, dst, insertIndex + 1, src.Length - insertIndex);
        }
        public static T[] Insert<T>(this T[] src, T insertValue, int insertIndex)
        {
            var dst = new T[src.Length + 1];
            Insert(src, insertValue, insertIndex, dst);
            return dst;
        }
        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            // List<T>の場合、既存のIndexOfメソッドを使用
            if (list is List<T> asList)
                return asList.IndexOf(item);

            // T[]の場合、Array.IndexOfを使用
            if (list is T[] asArray)
                return Array.IndexOf(asArray, item);

            // その他の場合、手動で探索
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < list.Count; i++)
            {
                if (comparer.Equals(list[i], item))
                    return i;
            }

            return -1;  // 見つからなかった場合
        }
        public static IEnumerable<T> PopMany<T>(this List<T> src, Func<T,bool> predicate)
        {
            var result = src.Where(predicate);
            src.RemoveAll(x => predicate(x));
            return result;
        }
    }

    public static class VectorExtensions 
    {
        /// <summary>
        /// 2つの座標間を線形補間して指定された数の頂点を生成します。
        /// </summary>
        /// <param name="start">開始座標</param>
        /// <param name="end">終了座標</param>
        /// <param name="vertexCount">生成する頂点の数（2以上）</param>
        /// <returns>線形補間された頂点の配列</returns>
        public static Vector3[] GenerateInterpolatedVertices(Vector3 start, Vector3 end, int vertexCount)
        {
            if (vertexCount < 2)
            {
                Debug.LogError("Vertex count must be at least 2.");
                return new Vector3[] { start, end };
            }

            Vector3[] vertices = new Vector3[vertexCount];
            vertices[0] = start;
            vertices[vertexCount - 1] = end;

            for (int i = 1; i < vertexCount - 1; i++)
            {
                float t = (float)i / (vertexCount - 1);
                vertices[i] = Vector3.Lerp(start, end, t);
            }

            return vertices;
        }
        public static Vector3 EaseInQuadratic(Vector3 start, Vector3 end, float t)
        {
            t = Mathf.Clamp01(t);
            float easedT = t * t; // Quadratic easing
            return Vector3.Lerp(start, end, easedT);
        }

        public static Vector3 EaseInTanh(Vector3 start, Vector3 end, float t)
        {
            t = Mathf.Clamp01(t);
            float easedT = (float)Math.Tanh(t * 2f) / 2f + 0.5f; // Adjusted Tanh easing
            return Vector3.Lerp(start, end, easedT);
        }

        public static Vector3 EaseInArctan(Vector3 start, Vector3 end, float t)
        {
            t = Mathf.Clamp01(t);
            float easedT = Mathf.Atan(t * Mathf.PI / 2) / (Mathf.PI / 2); // Arctan easing
            return Vector3.Lerp(start, end, easedT);
        }
        public static Vector3 ToVector3WithOrientation(this Vector2 vector2, Vector3 upDirection)
        {
            // Normalize the up direction
            upDirection.Normalize();

            // Create a rotation from the default up vector (0, 1, 0) to the desired up direction
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, upDirection);

            // Convert Vector2 to Vector3, setting z to 0
            Vector3 vector3 = new Vector3(vector2.x, vector2.y, 0);

            // Apply the rotation to the vector
            return rotation * vector3;
        }

        public static float Atan2(this Vector2 vector2) {
            var vn = vector2.normalized;
            return Mathf.Atan2(vn.y, vn.x);
        }
        public static Vector3 SetZ(this Vector3 vector3, float z)
        {
            return new Vector3(vector3.x,vector3.y, z);
        }
        public static Vector2 RadToVector2(float radAngle) => new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
    }

    public static class GradientExtensions
    {
        public static int CalculateMinimumKeys(this Gradient gradient, float timeThreshold = 0.001f, float startTime = 0.0f, float endTime = 1.0f)
        {
            if (gradient == null || timeThreshold <= 0 || startTime >= endTime)
            {
                Debug.LogError("Invalid input parameters.");
                return 0;
            }

            // Gradientのキーを時間でソートし、指定範囲内のキーのみを抽出
            var allKeys = new List<GradientKey>();
            allKeys.AddRange(gradient.alphaKeys.Select(k => new GradientKey(k.time, k.alpha, true)));
            allKeys.AddRange(gradient.colorKeys.Select(k => new GradientKey(k.time, k.color, false)));
            allKeys = allKeys.Where(k => k.time >= startTime && k.time <= endTime).OrderBy(k => k.time).ToList();

            // 開始時間と終了時間のキーを追加（既存のキーと重複しない場合）
            if (allKeys.Count == 0 || allKeys[0].time > startTime)
                allKeys.Insert(0, new GradientKey(startTime, gradient.Evaluate(startTime), false));
            if (allKeys[allKeys.Count - 1].time < endTime)
                allKeys.Add(new GradientKey(endTime, gradient.Evaluate(endTime), false));

            int minKeyCount = 0;
            float lastKeyTime = float.MinValue;

            foreach (var key in allKeys)
            {
                if (key.time - lastKeyTime > timeThreshold)
                {
                    minKeyCount++;
                    lastKeyTime = key.time;
                }
            }

            return minKeyCount;
        }

        private struct GradientKey
        {
            public float time;
            public Color color;
            public bool isAlpha;

            public GradientKey(float time, float alpha, bool isAlpha)
            {
                this.time = time;
                this.color = new Color(0, 0, 0, alpha);
                this.isAlpha = isAlpha;
            }

            public GradientKey(float time, Color color, bool isAlpha)
            {
                this.time = time;
                this.color = color;
                this.isAlpha = isAlpha;
            }
        }
        public static Vector3[] InterpolateAlongGradient(this Gradient gradient, Vector3 startPoint, Vector3 endPoint, float startTime = 0f, float endTime = 1f)
        {
            if (gradient == null || startTime >= endTime)
            {
                Debug.LogError("Invalid input parameters.");
                return new Vector3[] { startPoint, endPoint };
            }

            // Gradientのキーを時間でソートし、指定範囲内のキーのみを抽出
            var allKeys = new List<float>();
            allKeys.AddRange(gradient.alphaKeys.Select(k => k.time));
            allKeys.AddRange(gradient.colorKeys.Select(k => k.time));
            allKeys = allKeys.Where(t => t >= startTime && t <= endTime).Distinct().OrderBy(t => t).ToList();

            // 開始時間と終了時間を追加（既存のキーと重複しない場合）
            if (allKeys.Count == 0 || allKeys[0] > startTime)
                allKeys.Insert(0, startTime);
            if (allKeys[allKeys.Count - 1] < endTime)
                allKeys.Add(endTime);

            // キーに基づいて頂点を生成
            Vector3[] vertices = new Vector3[allKeys.Count];
            for (int i = 0; i < allKeys.Count; i++)
            {
                float normalizedTime = Mathf.InverseLerp(startTime, endTime, allKeys[i]);
                vertices[i] = Vector3.Lerp(startPoint, endPoint, normalizedTime);
            }

            return vertices;
        }

        // オプション: カラー情報も含めた結果を返すバージョン
        public static (Vector3[] positions, Color[] colors) InterpolateAlongGradientWithColors(this Gradient gradient, Vector3 startPoint, Vector3 endPoint, float startTime = 0f, float endTime = 1f)
        {
            Vector3[] positions = InterpolateAlongGradient(gradient, startPoint, endPoint, startTime, endTime);
            Color[] colors = new Color[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                float time = Mathf.Lerp(startTime, endTime, (float)i / (positions.Length - 1));
                colors[i] = gradient.Evaluate(time);
            }

            return (positions, colors);
        }
    }
    public static class DeepCopyUtility
    {
        public static T DeepCopy<T>(this T obj)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(obj));
            }

            if (ReferenceEquals(obj, null))
            {
                return default;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }

    public static class CollisionExtensions
    {
        public static LayerMask RemoveLayer(this LayerMask layerMask, int layerNumber)
        {
            // レイヤーを削除
            if (layerMask.HasLayer(layerNumber))
            {
                return layerMask & ~(1 << layerNumber);
            }
            return layerMask;
        }
        public static LayerMask RemoveLayer(this LayerMask layerMask, LayerMask layer) => layerMask.RemoveLayer(layer.value);

        public static LayerMask RemoveLayer(this LayerMask layerMask, string layerName)
        {
            int layerNumber = LayerMask.NameToLayer(layerName);
            if (layerNumber == -1)
            {
                Debug.LogError($"Layer '{layerName}' does not exist.");
                return layerMask;
            }
            return layerMask.RemoveLayer(layerNumber);
        }
        public static LayerMask AddLayer(this LayerMask layerMask, int layerNumber)
        {
            if (!layerMask.HasLayer(layerNumber))
            {
                return layerMask | (1 << layerNumber);
            }
            return layerMask;
        }
        public static LayerMask AddLayer(this LayerMask layerMask, LayerMask layer) => layerMask.AddLayer(layer.value);
        public static LayerMask AddLayer(this LayerMask layerMask, string layerName)
        {
            int layerNumber = LayerMask.NameToLayer(layerName);
            if (layerNumber == -1)
            {
                Debug.LogError($"Layer '{layerName}' does not exist.");
                return layerMask;
            }
            return layerMask.AddLayer(layerNumber);
        }
        public static bool HasLayer(this LayerMask layerMask, int layerNumber) => (layerMask.value & (1 << layerNumber)) != 0;
        public static bool HasLayer(this LayerMask layerMask, LayerMask layer) => layerMask.HasLayer(layer.value);
        public static bool HasLayer(this LayerMask layerMask, string layerName)
        {
            int layerNumber = LayerMask.NameToLayer(layerName);
            if (layerNumber == -1)
            {
                Debug.LogWarning($"Layer '{layerName}' does not exist.");
                return false;
            }
            return layerMask.HasLayer(layerNumber);
        }
        public static void AddIncludeLayer(this Collider2D collider, int layerToCheck)
        {
            collider.includeLayers = collider.includeLayers.AddLayer(layerToCheck);
        }
        public static void AddIncludeLayer(this Collider2D collider, LayerMask layerToCheck) => collider.AddIncludeLayer(layerToCheck.value);
        public static void AddIncludeLayer(this Collider2D collider, string layerToCheck)
        {
            collider.includeLayers = collider.includeLayers.AddLayer(layerToCheck);
        }
        public static void AddExcludeLayer(this Collider2D collider, int layerToCheck)
        {
            collider.excludeLayers = collider.excludeLayers.AddLayer(layerToCheck);
        }
        public static void AddExcludeLayer(this Collider2D collider, LayerMask layerToCheck) => collider.AddExcludeLayer(layerToCheck.value);
        public static void AddExcludeLayer(this Collider2D collider, string layerToCheck)
        {
            collider.excludeLayers = collider.excludeLayers.AddLayer(layerToCheck);
        }
        public static void RemoveExcludeLayer(this Collider2D collider, int layerToCheck)
        {
            collider.excludeLayers = collider.excludeLayers.RemoveLayer(layerToCheck);
        }
        public static void RemoveExcludeLayer(this Collider2D collider, LayerMask layerToCheck) => collider.RemoveExcludeLayer(layerToCheck.value);
        public static void RemoveExcludeLayer(this Collider2D collider, string layerToCheck)
        {
            collider.excludeLayers = collider.excludeLayers.RemoveLayer(layerToCheck);
        }
    }

    public static class ComponentExtension
    {
        public static T GetOrAddComponent<T>(this Component obj) where T : Component
        {
            if (obj.TryGetComponent(out T component))
            {
                return component;
            }
            return obj.AddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            if (obj.TryGetComponent(out T component))
            {
                return component;
            }
            return obj.AddComponent<T>();
        }
        public static bool TryGetComponentInParent<T>(this Component obj, out T comp) where T : class
        {
            if (!obj.TryGetComponent(out comp))
            {
                comp = obj.GetComponentInParent<T>();
                return comp != null;
            }
            return true;
        }
        public static bool TryGetComponentInParent<T>(this GameObject obj, out T comp) where T : class
        {
            if (!obj.TryGetComponent(out comp))
            {
                comp = obj.GetComponentInParent<T>();
                return comp != null;
            }
            return comp != null;
        }
        public static bool TryGetComponentInChildren<T>(this GameObject obj, out T comp) where T : class
        {
            comp = obj.GetComponentInChildren<T>();
            return comp != null;
        }

        public static bool TryGetComponentInChildrenToParent<T>(this GameObject obj, out T comp) where T : class
        {
            if (!obj.TryGetComponentInChildren(out comp))
            {
                return obj.TryGetComponentInParent(out comp);
            }
            return true;
        }

    }

    public static class RectTransformExtension
    {
        public static Vector3 ConvertToWorldPositionInCamera(this RectTransform rectTransform, Canvas canvas, Camera targetCamera, float depth, Vector2 screenOffset)
        {
            // 1. RectTransformの世界座標系でのデルタ位置を計算
            RectTransform canvasRectTransform = canvas.transform as RectTransform;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                if(canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    Vector3 worldOffset = Vector2.Scale(screenOffset, canvasRectTransform.localScale).ToVector3WithOrientation(canvas.worldCamera.transform.up);
                    return rectTransform.position - canvas.worldCamera.transform.forward * (canvas.planeDistance - depth) + worldOffset;
                }
                return rectTransform.position;
            }
           
            Vector3[] canvasWorldCorners = new Vector3[4];//LD,LU,RU,RL
            canvasRectTransform.GetWorldCorners(canvasWorldCorners);
            Rect canvasWorldRect = new(canvasWorldCorners[0].x, canvasWorldCorners[0].y, canvasWorldCorners[3].x - canvasWorldCorners[0].x, canvasWorldCorners[1].y - canvasWorldCorners[0].y);
            Vector3 canvasEdge = canvasWorldRect.position;
            Vector3 uiWorldPos = rectTransform.position;
            Vector3 deltaPosition = uiWorldPos - canvasEdge;

            // 2. Canvasの世界座標系での大きさを計算
            Vector3 canvasWorldSize = canvasWorldRect.size;

            // 3. スケール係数を計算
            Vector2 scaleFactor = new Vector2(
                canvasWorldSize.x / canvasRectTransform.rect.width,
                canvasWorldSize.y / canvasRectTransform.rect.height
            );

            // 4. デルタ位置をスケールで割る
            Vector3 scaledDeltaPosition = new Vector3(
                deltaPosition.x / scaleFactor.x,
                deltaPosition.y / scaleFactor.y,
                deltaPosition.z
            );
            // 5. スクリーン座標に変換
            Vector3 screenPosition = scaledDeltaPosition + (Vector3)screenOffset;
            // 6. Z座標を指定された深度に設定
            screenPosition.z = depth;
            // 7. スクリーン座標からワールド座標に変換
            return targetCamera.ScreenToWorldPoint(screenPosition);
        }
        public static Vector3 ConvertToWorldPositionInCamera(this RectTransform rectTransform, Canvas canvas, Camera targetCamera, float depth = 10f) => rectTransform.ConvertToWorldPositionInCamera(canvas, targetCamera, depth, Vector2.zero);
    }

    public static class CameraExtension
    {
        public static float Depth(this Camera camera, Vector3 targetPosition)
        {
            // カメラの位置とオブジェクトの位置のベクトル差を計算
            Vector3 cameraToObject = targetPosition - camera.transform.position;

            // カメラの前方ベクトルを取得
            Vector3 cameraForward = camera.transform.forward;

            // 深度（距離）を計算
            return Vector3.Dot(cameraToObject, cameraForward);
        }
    }

    public static class ColorExtension
    {
        public static Color SetAlpha(this Color color,float alpha)
        {
            return new Color(color.r,color.g,color.b,alpha);
        }
    }

    public class SLRandom
    {
        private static System.Random _random;
        public static System.Random Random => _random ??= new System.Random();
        public static T SelectRandom<T>(IEnumerable<T> pool) => pool.ElementAt(Random.Next(pool.Count()));
        public static T SelectRandom<T>(T[] pool) => pool[Random.Next(pool.Length)];
        public static int SelectRandomIndex<T>(IEnumerable<T> pool, T value) where T : IComparable<T>
        {
            var selectList = pool.Select((v, i) => (v, i)).Where((v) => v.v.Equals(value));
            return SelectRandom(selectList.Select((v) => v.i).ToList());
        }
        public static T[] ChoicesWithMinimalDuplication<T>(IEnumerable<T> pool, int k)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            var poolList = new List<T>(pool);
            int n = poolList.Count;

            if (k < 0)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be non-negative.");

            if (n == 0 && k > 0)
                throw new InvalidOperationException("Cannot choose from an empty pool.");

            T[] result = new T[k];

            if (k <= n)
            {
                // poolの長さ以下の場合は重複なしで選択
                return Choices(poolList, k);
            }
            else
            {
                // poolの長さを超える場合は、まず全ての要素を選択
                poolList.CopyTo(result, 0);

                // 残りの要素を均等に分配
                int remainingCount = k - n;
                int[] counts = new int[n];
                for (int i = 0; i < remainingCount; i++)
                {
                    counts[i % n]++;
                }

                // countsに基づいて要素を追加
                int index = n;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < counts[i]; j++)
                    {
                        result[index++] = poolList[i];
                    }
                }

                // 結果をシャッフル
                for (int i = 0; i < k; i++)
                {
                    int j = Random.Next(i, k);
                    T temp = result[i];
                    result[i] = result[j];
                    result[j] = temp;
                }
            }

            return result;
        }
        public static T Choice<T>(T[] pool, float[] weights)
        {
            if (pool == null || weights == null || pool.Length != weights.Length || pool.Length == 0)
            {
                throw new ArgumentException("Invalid input: pool and weights must be non-null, have the same length, and contain at least one element.");
            }

            float totalWeight = weights.Sum();
            if (totalWeight <= 0)
            {
                throw new ArgumentException("Total weight must be positive.");
            }

            float randomValue = (float)Random.NextDouble() * totalWeight;

            for (int i = 0; i < pool.Length; i++)
            {
                if (randomValue < weights[i])
                {
                    return pool[i];
                }
                randomValue -= weights[i];
            }

            // This should never happen if the weights sum to totalWeight
            return pool[pool.Length - 1];
        }
        public static T Choice<T>(T[] pool) => SelectRandom(pool);
        public static T Choice<T>(IEnumerable<T> pool) => SelectRandom(pool);
        public static T[] Choices<T>(IEnumerable<T> pool, int k)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            var poolList = new List<T>(pool);
            int n = poolList.Count;

            if (k < 0 || k > n)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between 0 and the size of the pool.");

            T[] result = new T[k];
            for (int i = 0; i < k; i++)
            {
                int j = Random.Next(i, n);
                result[i] = poolList[j];
                poolList[j] = poolList[i];
            }

            return result;
        }
        public static T[] Sample<T>(IEnumerable<T> pool, int k)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            var poolList = new List<T>(pool);
            int n = poolList.Count;

            if (k < 0)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be non-negative.");

            if (n == 0 && k > 0)
                throw new InvalidOperationException("Cannot sample from an empty pool.");

            T[] result = new T[k];
            for (int i = 0; i < k; i++)
            {
                int j = Random.Next(0, n);
                result[i] = poolList[j];
            }

            return result;
        }
        public static T GetRandomEnumValue<T>() where T : Enum
        {
            Type enumType = typeof(T);
            Array values = Enum.GetValues(enumType);
            // FlagsAttributeを持つかチェック
            bool isFlags = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();

            if (isFlags)
            {
                // FlagsAttributeを持つ場合、0以外の値からランダムに選択
                var nonZeroValues = values.Cast<T>().Where(v => Convert.ToInt64(v) != 0).ToArray();
                if (nonZeroValues.Length == 0)
                {
                    throw new InvalidOperationException($"Enum {enumType.Name} has no non-zero values.");
                }
                return nonZeroValues[Random.Next(nonZeroValues.Length)];
            }
            else
            {
                // 通常のEnumの場合、すべての値から選択
                return (T)values.GetValue(Random.Next(values.Length));
            }
        }
        public static T GetRandomEnumValue<T>(T enumValue) where T : Enum
        {
            Type enumType = typeof(T);
            Array values = Enum.GetValues(enumType);
            // FlagsAttributeを持つかチェック
            bool isFlags = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();

            if (isFlags)
            {
                // FlagsAttributeを持つ場合、0以外の値からランダムに選択
                var nonZeroValues = values.Cast<T>().Where(v => Convert.ToInt64(v) != 0 && enumValue.HasFlag(v)).ToArray();
                if (nonZeroValues.Length == 0)
                {
                    throw new InvalidOperationException($"Enum {enumType.Name} has no non-zero values.");
                }
                return nonZeroValues[Random.Next(nonZeroValues.Length)];
            }
            else
            {
                // 通常のEnumの場合、すべての値から選択
                return (T)values.GetValue(Random.Next(values.Length));
            }
        }

        public static Vector2 insideRing(float minRadius, float maxRadius)
        {
            var circle = UnityEngine.Random.insideUnitCircle;
            return circle * (maxRadius - minRadius) + circle.normalized*minRadius; 
        }

        public static float NextSingle(float min, float max) => Mathf.Lerp(min, max, (float)Random.NextDouble());
        public static float NextSingle(float max) => NextSingle(0, max);
    }
    public static class ShadowCaster2DExtensions
    {
        public static void SetPath(this ShadowCaster2D shadowCaster, Vector3[] path)
        {
            FieldInfo shapeField = typeof(ShadowCaster2D).GetField("m_ShapePath",
                                                                   BindingFlags.NonPublic |
                                                                   BindingFlags.Instance);
            shapeField.SetValue(shadowCaster, path);
        }

        public static void SetPathHash(this ShadowCaster2D shadowCaster, int hash)
        {
            FieldInfo hashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance);
            hashField.SetValue(shadowCaster, hash);
        }
    }
    public static class AdvancedRomanNumeralConverter
    {
        private static readonly Dictionary<int, string> romanNumerals = new Dictionary<int, string>
    {
        { 1000, "M" },
        { 900, "CM" },
        { 500, "D" },
        { 400, "CD" },
        { 100, "C" },
        { 90, "XC" },
        { 50, "L" },
        { 40, "XL" },
        { 10, "X" },
        { 9, "IX" },
        { 5, "V" },
        { 4, "IV" },
        { 1, "I" }
    };

        private const long MAX_STANDARD_VALUE = 3999;
        private const long BRACKET_THRESHOLD = 500000;
        private const long INFINITY_THRESHOLD = 1000000000000; // 1兆

        public static string ConvertToRomanNumeral(long number)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Number must be positive.");
            }

            if (number >= INFINITY_THRESHOLD)
            {
                return "∞"; // 無限大記号
            }

            StringBuilder result = new StringBuilder();

            if (number > MAX_STANDARD_VALUE)
            {
                result.Append(ConvertLargeNumber(number));
            }
            else
            {
                result.Append(ConvertStandardNumber((int)number));
            }

            return result.ToString();
        }

        private static string ConvertStandardNumber(int number)
        {
            StringBuilder result = new StringBuilder();
            foreach (var item in romanNumerals)
            {
                while (number >= item.Key)
                {
                    result.Append(item.Value);
                    number -= item.Key;
                }
            }
            return result.ToString();
        }

        private static string ConvertLargeNumber(long number)
        {
            StringBuilder result = new StringBuilder();
            int bracketCount = 0;

            while (number >= 1000)
            {
                bracketCount++;
                number /= 1000;
            }

            string core = ConvertStandardNumber((int)number);

            if (bracketCount >= 2 || number >= BRACKET_THRESHOLD / 1000)
            {
                result.Append('(', bracketCount);
                result.Append(core);
                result.Append(')', bracketCount);
            }
            else
            {
                result.Append(core);
                result.Append('M', bracketCount);
            }

            return result.ToString();
        }

        public static string ConvertToRomanNumeralOrDefault(long number, string defaultValue = "")
        {
            if (number < 1)
            {
                return defaultValue;
            }

            return ConvertToRomanNumeral(number);
        }
    }
}