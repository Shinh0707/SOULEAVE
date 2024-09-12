using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using SL.Lib;
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(CompositeCollider2D))]
public class TilemapShadowCaster : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private CompositeCollider2D compositeCollider;

    [SerializeField] bool m_UseRendererSilhouette = true;
    [SerializeField] bool m_CastsShadows = true;
    [SerializeField] bool m_SelfShadows = false;
    [SerializeField] int[] m_ApplyToSortingLayers = null;
    private CompositeShadowCaster2D m_CompositeShadowCaster2D;

    private bool m_NeedsUpdate = false;
    private List<GameObject> m_ShadowCasterObjects = new List<GameObject>();

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        if (compositeCollider == null)
            compositeCollider = GetComponent<CompositeCollider2D>();

        m_CompositeShadowCaster2D = GetComponent<CompositeShadowCaster2D>();
        if (m_CompositeShadowCaster2D == null)
            m_CompositeShadowCaster2D = gameObject.AddComponent<CompositeShadowCaster2D>();
    }

    private void Start()
    {
        UpdateShadowCasters();
    }

    private void Update()
    {
        if (m_NeedsUpdate)
        {
            StartCoroutine(ApplyShadowCasters());
            m_NeedsUpdate = false;
        }
    }

    private void OnEnable()
    {
        UpdateShadowCasters();
    }

    private void OnDisable()
    {
        ClearShadowCasters();
    }

    public IEnumerator ApplyShadowCasters()
    {
        if (tilemap != null && compositeCollider != null)
        {
            ClearShadowCasters();

            int i = 0;
            Vector3 halfOffset = Vector2.one * 0.5f;
            foreach(var position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(position))
                {
                    GameObject go = new GameObject($"ShadowCaster_{i}");
                    ShadowCaster2D shadowCaster = go.AddComponent<ShadowCaster2D>();
                    ApplyShadowCasterProperties(shadowCaster);
                    SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
                    if (tilemap.TryGetComponent(out TilemapRenderer tilemapRenderer)) {
                        spriteRenderer.sharedMaterial = tilemapRenderer.sharedMaterial;
                        spriteRenderer.sortingLayerID = tilemapRenderer.sortingLayerID;
                    }
                    spriteRenderer.receiveShadows = true;
                    spriteRenderer.enabled = true;
                    spriteRenderer.sprite = tilemap.GetSprite(position);
                    go.transform.SetParent(transform, false);
                    go.transform.position = position + halfOffset;
                    m_ShadowCasterObjects.Add(go);
                    i++;
                }
            }

            // Generate new shadow casters
            //int pathCount = compositeCollider.pathCount;
            //List<Vector2> pointsInPath = new List<Vector2>();

            //for (int i = 0; i < pathCount; ++i)
            //{
            //    compositeCollider.GetPath(i, pointsInPath);

            //    GameObject newShadowCaster = new GameObject($"ShadowCaster2D_{i}");
            //    newShadowCaster.transform.SetParent(transform, false);
            //    m_ShadowCasterObjects.Add(newShadowCaster);

            //    ShadowCaster2D shadowCaster = newShadowCaster.AddComponent<ShadowCaster2D>();
            //    ApplyShadowCasterProperties(shadowCaster);

            //    Vector3[] path3D = new Vector3[pointsInPath.Count];
            //    for (int j = 0; j < pointsInPath.Count; ++j)
            //    {
            //        path3D[j] = new Vector3(pointsInPath[j].x, pointsInPath[j].y, 0);
            //    }

            //    shadowCaster.SetPath(path3D);
            //    shadowCaster.SetPathHash(Random.Range(int.MinValue, int.MaxValue));

            //    m_CompositeShadowCaster2D.RegisterShadowCaster2D(shadowCaster);
            //    newShadowCaster.AddComponent<SpriteRenderer>();
            //}

            yield return null;
        }
    }

    private void ApplyShadowCasterProperties(ShadowCaster2D shadowCaster)
    {
        shadowCaster.useRendererSilhouette = m_UseRendererSilhouette;
        shadowCaster.castsShadows = m_CastsShadows;
        shadowCaster.selfShadows = m_SelfShadows;

        // Apply sorting layers
        FieldInfo sortingLayerField = typeof(ShadowCaster2D).GetField("m_ApplyToSortingLayers",
                                                              BindingFlags.NonPublic |
                                                              BindingFlags.Instance);
        sortingLayerField.SetValue(shadowCaster, m_ApplyToSortingLayers);
    }

    public void UpdateShadowCasters()
    {
        m_NeedsUpdate = true;
    }

    private void ClearShadowCasters()
    {
        foreach (var obj in m_ShadowCasterObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        m_ShadowCasterObjects.Clear();
    }

    private void OnValidate()
    {
        m_NeedsUpdate = true;
    }
}