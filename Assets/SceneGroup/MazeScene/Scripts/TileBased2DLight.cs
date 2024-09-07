using SL.Lib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
public class TileBased2DLight : MonoBehaviour
{   
    [SerializeField] private GameObject Tile3D;
    [SerializeField] private Transform TileArea;
    [SerializeField] private Transform floorCast;
    [SerializeField] private LightFlicker1fNoise targetLight;
    private List<GameObject> tiles = new List<GameObject>();
    private List<int> unactiveTiles = new();
    private List<int> activeTiles = new();
    private Vector2 lastPosition = Vector2.zero;

    public float LightRange
    {
        get
        {
            return targetLight.BaseRange;
        }
        set 
        {
            if (targetLight.BaseRange != value)
            {
                targetLight.BaseRange = value;
                ForceUpdateLight();
            }
        }
    }

    private void FixedUpdate()
    {
        if(Vector2.Distance(lastPosition, LightCenter) >= 0.001)
        {
            lastPosition = LightCenter;
            ForceUpdateLight();
        }
    }

    private void ForceUpdateLight()
    {
        int TileRange = LightFieldSize() * 2 + 1;
        int currentTiles = tiles.Count;
        for (; currentTiles < TileRange * TileRange; currentTiles++)
        {
            CreateTile();
        }
        floorCast.localScale = new Vector3(1.0f, 0f, 1.0f) * targetLight.BaseRange *2 + Vector3.up * 0.1f;
        floorCast.position = new Vector3(LightCenter.x, LightCenter.y, floorCast.localScale.y*2);
        UpdateTiles();
    }

    public int LightFieldSize()
    {
        return Mathf.CeilToInt(Mathf.Abs(LightRange));
    }

    private Vector2 LightCenter => (Vector2)targetLight.targetLight.transform.position;

    public void CreateTile()
    {
        var tile = Instantiate(Tile3D, TileArea);
        tile.SetActive(false);
        unactiveTiles.Add(tiles.Count);
        tiles.Add(tile);
    }

    private void UpdateTiles()
    {
        SetUnactiveAllTiles();
        var centerTileIndex = MazeGameScene.Instance.MazeManager.GetTensorPosition(LightCenter);
        int lfs = LightFieldSize();
        MazeGameScene.Instance.MazeManager.BoundRange(centerTileIndex.i - lfs - 1, lfs * 2 + 1, centerTileIndex.j - lfs - 1, lfs * 2 + 1, UpdateTile);
        var bM = MazeGameScene.Instance.MazeManager.GetBaseMap();
        ESEResult ESE = ESEResult.Get(new Indice[] { centerTileIndex.i, centerTileIndex.j }, bM);
        var r = ESE.Extract(bM,1);
        Debug.Log($"{r}");
        (int i, int j) offset = (centerTileIndex.i - 1, centerTileIndex.j - 1);
        Vector3 halfOffset = Vector2.one * 0.5f;
        foreach (var index in new (int i,int j)[] { (0, 0), (2, 0), (0, 2), (2, 2) })
        {
            if (r[index.i, index.j] == 0 && r[1,index.j]==1 && r[index.i,1]==1)
            {
                PushTile(MazeGameScene.Instance.MazeManager.GetTilePosition((index.i+offset.i,index.j+offset.j))+halfOffset);
            }
        }
    }

    private void SetUnactiveAllTiles()
    {
        unactiveTiles = Enumerable.Range(0, tiles.Count).ToList();
        activeTiles.Clear();
        foreach(var tile in tiles)
        {
            tile.SetActive(false);
        }
    }

    private void UpdateTile(Vector3 position, int data)
    {
        if(data == 1)
        {
            PushTile(position);
        }
    }

    private void PushTile(Vector3 position)
    {
        if(unactiveTiles.Count == 0)
        {
            UnactiveTile(activeTiles[0]);
        }
        ActiveTile(position, unactiveTiles[0]);
    }

    private void ActiveTile(Vector3 position, int tileId)
    {
        if (unactiveTiles.Contains(tileId))
        {
            activeTiles.Add(tileId);
            unactiveTiles.Remove(tileId);
            tiles[tileId].transform.position = position;
            tiles[tileId].SetActive(true);
        }
    }

    private void UnactiveTile(int tileId)
    {
        if (activeTiles.Contains(tileId)){
            tiles[tileId].SetActive(false);
            activeTiles.Remove(tileId);
            unactiveTiles.Add(tileId);
        }
    }
}