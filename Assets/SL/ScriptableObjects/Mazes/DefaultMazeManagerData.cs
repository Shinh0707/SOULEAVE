using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DefaultMazeManagerData : SingletonScriptableObject<DefaultMazeManagerData>
{
    [SerializeField] private AutoCreateMazeDataParameter AutoCreateMazeDataParameter = new();

    public MazeData Create() => AutoCreateMazeDataParameter?.Create();
}
