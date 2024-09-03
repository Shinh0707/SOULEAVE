using UnityEngine;
using System.Xml;
using System;
using System.Collections.Generic;

public class MazeGameStats : SingletonMonoBehaviour<MazeGameStats>
{
    private bool isInitialized = false;
    private Dictionary<string, float> stats = new Dictionary<string, float>();

    public delegate void OnInitializationComplete();
    public static event OnInitializationComplete OnInitialized;

    protected override void Awake()
    {
        base.Awake();
        LoadStatsFromXml();
    }

    private void LoadStatsFromXml()
    {
        TextAsset xmlFile = Resources.Load<TextAsset>("Parameters/MazeGameStats");
        if (xmlFile == null)
        {
            Debug.LogError("MazeGameStats.xml not found in Resources folder");
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlFile.text);

        XmlNodeList statNodes = xmlDoc.SelectNodes("//stat");
        foreach (XmlNode statNode in statNodes)
        {
            string name = statNode.Attributes["name"].Value;
            float value = float.Parse(statNode.Attributes["value"].Value);
            stats[name] = value;
        }

        isInitialized = true;
        OnInitialized?.Invoke();
    }

    public float GetStat(string statName)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Attempting to access stats before initialization is complete");
            return 0f;
        }

        if (stats.TryGetValue(statName, out float value))
        {
            return value;
        }
        else
        {
            Debug.LogWarning($"Stat '{statName}' not found");
            return 0f;
        }
    }

    public float MaxMP => GetStat("MaxMP");
    public float MaxSight => GetStat("MaxSight");
    public float RestoreMPPerSecond => GetStat("RestoreMPPerSecond");
    public float RestoreSightPerSecond => GetStat("RestoreSightPerSecond");
    public float HintDuration => GetStat("HintDuration");
    public float HintMPCost => GetStat("HintMPCost");
    public float TeleportMPCost => GetStat("TeleportMPCost");
    public float TeleportSightCostPerDistance => GetStat("TeleportSightCostPerDistance");
    public float MinSightForTeleport => GetStat("MinSightForTeleport");
    public float MPForBrightnessValuePerSecond => GetStat("MPForBrightnessValuePerSecond");
    public float MPForBrightnessCostPerSecond => GetStat("MPForBrightnessCostPerSecond");
    public float MPForBrightnessDecayPerSecond => GetStat("MPForBrightnessDecayPerSecond");
    public float TransparentDuration => GetStat("TransparentDuration");
    public float MonsterAddingInterval => GetStat("MonsterAddingInterval");
    public float VisibleBorder => GetStat("VisibleBorder");
    public float EnemyDamage => GetStat("EnemyDamage");
}