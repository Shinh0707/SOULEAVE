using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SL.Lib
{
    public struct EnemySpawnData
    {
        public (int i, int j) TensorIndex;
        public EnemyTypeSelect EnemyType;
        public Vector2 Position;
    }
    [Serializable]
    public struct EnemyRateSet
    {
        public EnemyType EnemyType;
        public float Rate;
    }
    [Serializable]
    public class AutoCreateMazeDataParameter
    {
        [SerializeField]
        private Vector2Int mazeSize = Vector2Int.one * 10;
        public List<TileBase> tileList = new();
        [Range(0, 1)]
        public float mazeRouteRate = 0.1f;
        public EnemyTypeSelect enemyTypes = EnemyTypeSelect.Common;
        [SerializeField] private EnemyRateSet[] enemyRates;
        public int GetMazeRouteSize() => Mathf.FloorToInt(mazeSize.x * mazeSize.y * mazeRouteRate);

        public MazeData Create()
        {
            MazeData mazeData = new(mazeSize.x, mazeSize.y, tileList, GetMazeRouteSize());
            var IntBaseMap = mazeData.GetBaseMap();
            var BaseMap = mazeData.BaseMap;

            // EnemyRateSetsに従って、敵の出現マップを設定
            float totalEnemyArea = IntBaseMap.Sum() * mazeRouteRate;
            float remainingArea = totalEnemyArea;

            // 有効な敵タイプのみを処理
            var validEnemyRates = enemyRates.Where(rate => (enemyTypes & (EnemyTypeSelect)rate.EnemyType) != 0).ToArray();

            // レートの合計を計算
            float totalRate = validEnemyRates.Sum(rate => rate.Rate);

            // レートを正規化
            if (totalRate > 0)
            {
                for (int i = 0; i < validEnemyRates.Length; i++)
                {
                    var normalizedRate = validEnemyRates[i].Rate / totalRate;
                    var areaToFill = Mathf.FloorToInt(totalEnemyArea * normalizedRate);

                    var enemyType = (EnemyTypeSelect)validEnemyRates[i].EnemyType;
                    var mask = Tensor<bool>.Randoms(BaseMap, areaToFill / (float)(mazeData.mazeSize.rows * mazeData.mazeSize.cols));
                    mazeData.SetEnemyArea(enemyType, mask & BaseMap);

                    remainingArea -= areaToFill;
                }
            }

            // 残りのエリアをランダムな有効な敵タイプで埋める
            if (remainingArea > 0)
            {
                var remainingMask = Tensor<bool>.Randoms(BaseMap, remainingArea / (float)(mazeData.mazeSize.rows * mazeData.mazeSize.cols));
                var validEnemyTypes = Enum.GetValues(typeof(EnemyTypeSelect))
                    .Cast<EnemyTypeSelect>()
                    .Where(e => e != EnemyTypeSelect.None && (enemyTypes & e) != 0)
                    .ToList();

                foreach (var index in remainingMask.ArgWhere())
                {
                    if (mazeData.GetBaseMap()[index[0], index[1]] == 1)
                    {
                        var randomEnemyType = SLRandom.SelectRandom(validEnemyTypes);
                        mazeData.SetEnemyArea(randomEnemyType, new int[] { index[0], index[1] });
                    }
                }
            }

            return mazeData;
        }
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(AutoCreateMazeDataParameter))]
        public class AutoCreateMazeDataParameterDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                var mazeSizeProperty = property.FindPropertyRelative("mazeSize");
                var tileListProperty = property.FindPropertyRelative("tileList");
                var mazeRouteRateProperty = property.FindPropertyRelative("mazeRouteRate");
                var enemyTypesProperty = property.FindPropertyRelative("enemyTypes");
                var enemyRatesProperty = property.FindPropertyRelative("enemyRates");

                float yOffset = 0f;

                // MazeSize
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight),
                    mazeSizeProperty);
                yOffset += EditorGUIUtility.singleLineHeight;

                // TileList
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight),
                    tileListProperty, true);
                yOffset += EditorGUI.GetPropertyHeight(tileListProperty);

                // MazeRouteRate
                EditorGUI.Slider(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight),
                    mazeRouteRateProperty, 0f, 1f, new GUIContent("Maze Route Rate"));
                yOffset += EditorGUIUtility.singleLineHeight;

                // EnemyTypes
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight),
                    enemyTypesProperty);
                yOffset += EditorGUIUtility.singleLineHeight;

                // EnemyRates
                EditorGUI.LabelField(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), "Enemy Rates");
                yOffset += EditorGUIUtility.singleLineHeight;

                var enemyTypes = (EnemyTypeSelect)enemyTypesProperty.intValue;
                var validEnemyTypes = GetValidEnemyTypes(enemyTypes);

                // Ensure enemyRates has the correct number of elements
                while (enemyRatesProperty.arraySize > validEnemyTypes.Count)
                {
                    enemyRatesProperty.DeleteArrayElementAtIndex(enemyRatesProperty.arraySize - 1);
                }
                while (enemyRatesProperty.arraySize < validEnemyTypes.Count)
                {
                    enemyRatesProperty.InsertArrayElementAtIndex(enemyRatesProperty.arraySize);
                    var newElement = enemyRatesProperty.GetArrayElementAtIndex(enemyRatesProperty.arraySize - 1);
                    newElement.FindPropertyRelative("EnemyType").intValue = (int)validEnemyTypes[enemyRatesProperty.arraySize - 1];
                    newElement.FindPropertyRelative("Rate").floatValue = 0f;
                }

                float totalRate = 0f;
                for (int i = 0; i < validEnemyTypes.Count; i++)
                {
                    var enemyRateSet = enemyRatesProperty.GetArrayElementAtIndex(i);
                    var enemyTypeProperty = enemyRateSet.FindPropertyRelative("EnemyType");
                    var rateProperty = enemyRateSet.FindPropertyRelative("Rate");

                    EditorGUI.LabelField(
                        new Rect(position.x, position.y + yOffset, position.width * 0.6f, EditorGUIUtility.singleLineHeight),
                        validEnemyTypes[i].ToString());

                    rateProperty.floatValue = EditorGUI.Slider(
                        new Rect(position.x + position.width * 0.6f, position.y + yOffset, position.width * 0.4f, EditorGUIUtility.singleLineHeight),
                        rateProperty.floatValue, 0f, 1f);

                    totalRate += rateProperty.floatValue;
                    yOffset += EditorGUIUtility.singleLineHeight;
                }

                // Normalize rates if total exceeds 1
                if (totalRate > 1f)
                {
                    for (int i = 0; i < enemyRatesProperty.arraySize; i++)
                    {
                        var rateProperty = enemyRatesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Rate");
                        rateProperty.floatValue /= totalRate;
                    }
                }

                EditorGUI.EndProperty();
            }

            private List<EnemyType> GetValidEnemyTypes(EnemyTypeSelect enemyTypes)
            {
                var validTypes = new List<EnemyType>();
                if ((enemyTypes & EnemyTypeSelect.Common) != 0) validTypes.Add(EnemyType.Common);
                if ((enemyTypes & EnemyTypeSelect.Shot) != 0) validTypes.Add(EnemyType.Shot);
                return validTypes;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var tileListProperty = property.FindPropertyRelative("tileList");
                var enemyTypesProperty = property.FindPropertyRelative("enemyTypes");
                var enemyTypes = (EnemyTypeSelect)enemyTypesProperty.intValue;
                var validEnemyTypes = GetValidEnemyTypes(enemyTypes);

                return EditorGUIUtility.singleLineHeight * (5 + validEnemyTypes.Count) +
                       EditorGUI.GetPropertyHeight(tileListProperty);
            }
        }
#endif

    }


    [Serializable]
    public class MazeData
    {
        //見る用でしかない
        [SerializeField] private int mazeWidth = 20;
        [SerializeField] private int mazeHeight = 20;
        [SerializeField] public List<TileBase> tileList; // 0: 通路, 1: 壁
        [SerializeField] private Tensor<bool> baseMap;
        [SerializeField] private Tensor<IEnemyTypeSelect> enemyMap;
        public (int rows, int cols) mazeSize => (baseMap.Shape[0], baseMap.Shape[1]);
        public Tensor<bool> BaseMap => baseMap.DeepCopy();

        private Dictionary<int, List<((int, int), (int, int))>> startAndGoalPoints;
        private TensorLabel mazeLabel;

        public MazeData(int mazeWidth, int mazeHeight, List<TileBase> tileList, int minRouteArea)
        {
            this.mazeWidth = mazeWidth;
            this.mazeHeight = mazeHeight;
            this.tileList = tileList;
            GenerateBaseMaze(minRouteArea);
        }
        /// <summary>
        /// 自動で迷路を生成する。
        /// </summary>
        /// <param name="minRouteArea">最小の移動可能範囲</param>
        public void GenerateBaseMaze(int minRouteArea)
        {
            Tensor<int> intBaseMap;
            (intBaseMap, mazeLabel) = MazeCreater.CreatePlainMaze(mazeWidth, mazeHeight, minRouteArea);
            baseMap = intBaseMap == 0;
            enemyMap = Tensor<IEnemyTypeSelect>.Zeros(baseMap);
            CalcrateStartAndGoalPoints();
        }

        public void UpdateMazeData()
        {
            ReCalcrateLabel();
            enemyMap[!baseMap] = new IEnemyTypeSelect().Zero();
        }

        /// <summary>
        /// 迷路のラベリング
        /// 迷路を手作りしたあとに実行するといい
        /// </summary>
        public void ReCalcrateLabel()
        {
            mazeLabel = new TensorLabel(baseMap);
        }

        /// <summary>
        /// 自動でスタート地点とゴール地点の候補を計算する
        /// </summary>
        private void CalcrateStartAndGoalPoints()
        {
            startAndGoalPoints = new();
            foreach (var label in mazeLabel.RouteLabels)
            {
                LongestPathFinder LPF = new(baseMap, (mazeLabel.Label == label));
                startAndGoalPoints[label] = LPF.FindLongestShortestPaths();
            }
        }

        /// <summary>
        /// 敵の出現エリアマップを設定する
        /// </summary>
        /// <param name="enemyType">設定する敵</param>
        /// <param name="mask">出現可能エリア</param>
        public void SetEnemyArea(EnemyTypeSelect enemyType, Tensor<bool> mask)
        {
            enemyMap[mask] = (IEnemyTypeSelect)enemyType;
        }
        /// <summary>
        /// 敵の出現位置を設定する
        /// </summary>
        /// <param name="enemyType">設定する敵</param>
        /// <param name="index">出現位置</param>
        public void SetEnemyArea(EnemyTypeSelect enemyType, int[] index)
        {
            enemyMap[index] = (IEnemyTypeSelect)enemyType;
        }

        public void SetEnemyArea(EnemyTypeSelect enemyType, Tensor<bool> mask, List<(int, int)> notSelectPoints)
        {
            foreach (var pos in notSelectPoints)
            {
                mask[pos.Item1, pos.Item2] = false;
            }
            SetEnemyArea(enemyType, mask);
        }
        public void SetEnemyArea(EnemyTypeSelect enemyType, int region, List<(int, int)> notSelectPoints)
        {
            SetEnemyArea(enemyType, mazeLabel.Label == region, notSelectPoints);
        }
        public void SetEnemyArea(EnemyTypeSelect enemyType, int region)
        {
            SetEnemyArea(enemyType, mazeLabel.Label == region);
        }
        public EnemyTypeSelect GetEnemy((int i, int j) index) => enemyMap[index.i, index.j];
        public List<EnemyTypeSelect> GetEnemies(List<(int, int)> indexes) => indexes.Select(index => GetEnemy(index)).ToList();
        public void GetEnemySpawnData(EnemySpawnData[] enemySpawnDatas,int region, params (int,int)[] invalidPositions)
        {
            int num = enemySpawnDatas.Length;
            var mask = (enemyMap != EnemyTypeSelect.None) & mazeLabel.GetAreaMask(region);
            foreach(var (i,  j) in invalidPositions)
            {
                mask[i, j] = false;
            }
            var spawnable = SLRandom.ChoicesWithMinimalDuplication(mask.ArgWhere(), num);
            for (int i = 0; i < num; i++)
            {
                enemySpawnDatas[i] = new() { TensorIndex = (spawnable[i][0], spawnable[i][1]), EnemyType = enemyMap[spawnable[i]] };
            }
        }
        public void GetRandomStartAndGoal(out int selectedRegion, out (int, int) start, out (int, int) goal)
        {
            int region = SLRandom.SelectRandom(startAndGoalPoints.Keys.ToList());
            selectedRegion = region;
            (start, goal) = SLRandom.SelectRandom(startAndGoalPoints[region]);
        }

        public int[][] GetRandomPositions(int region, int num, params (int i, int j)[] notSelectPoints)
        {
            var mask = mazeLabel.Label == region;
            foreach (var (i, j) in notSelectPoints)
            {
                mask[i, j] = false;
            }
            var predIndices = mask.ArgWhere();
            var indices = SLRandom.Choices(predIndices, Mathf.Min(num, predIndices.Count));
            return indices;
        }
        public int[] GetRandomPosition(int region, (int i, int j)[] notSelectPoints)
        {
            var mask = mazeLabel.Label == region;
            foreach (var (i, j) in notSelectPoints)
            {
                mask[i, j] = false;
            }
            var predIndices = mask.ArgWhere();
            return SLRandom.Choice(predIndices);
        }
        public int[] GetRandomPosition(int region)
        {
            var mask = mazeLabel.Label == region;
            var predIndices = mask.ArgWhere();
            return SLRandom.Choice(predIndices);
        }

        public Tensor<int> GetBaseMap()
        {
            return (!baseMap).Cast<int>();
        }

        public int GetRegionSize(int region) => mazeLabel.GetAreaSize(region);
        public override string ToString()
        {
            string mazeString = "";
            for (int y = 0; y < mazeHeight; y++)
            {
                for (int x = 0; x < mazeWidth; x++)
                {
                    if (!baseMap[y, x])
                        mazeString += "*";
                    else
                        mazeString += " ";
                }
                mazeString += "\n";
            }
            return mazeString;
        }
    }

}