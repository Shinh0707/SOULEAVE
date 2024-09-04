using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileBased2DLight : MonoBehaviour
{
    public int tileResolution = 32; // Number of tiles per side
    public float lightRadius = 5f;
    public Color lightColor = Color.white;
    public LayerMask shadowLayers;

    private SpriteRenderer spriteRenderer;
    private Texture2D lightTexture;
    private Color[] colorBuffer;
    private bool[,] lightMap;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitializeLight();
    }

    void Update()
    {
        UpdateLight();
    }

    void InitializeLight()
    {
        lightTexture = new Texture2D(tileResolution, tileResolution);
        lightTexture.filterMode = FilterMode.Point; // Use point filtering for sharp tile edges
        lightTexture.wrapMode = TextureWrapMode.Clamp;

        colorBuffer = new Color[tileResolution * tileResolution];
        lightMap = new bool[tileResolution, tileResolution];
        // Set the sprite's pixels per unit to match the light radius
        float pixelsPerUnit = tileResolution / (lightRadius * 2);

        Sprite lightSprite = Sprite.Create(lightTexture, new Rect(0, 0, tileResolution, tileResolution), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        spriteRenderer.sprite = lightSprite;
        UpdateLight();
    }

    void UpdateLight()
    {
        ClearLightMap();
        CastLight();
        ApplyLightMap();

        lightTexture.SetPixels(colorBuffer);
        lightTexture.Apply();
    }

    void ClearLightMap()
    {
        for (int x = 0; x < tileResolution; x++)
        {
            for (int y = 0; y < tileResolution; y++)
            {
                lightMap[x, y] = false;
            }
        }
    }

    void CastLight()
    {
        Vector2 centerTile = new Vector2(tileResolution / 2, tileResolution / 2);
        float tileSize = lightRadius * 2 / tileResolution;

        for (int octant = 0; octant < 8; octant++)
        {
            CastLightInOctant(octant, centerTile, tileSize);
        }
    }

    void CastLightInOctant(int octant, Vector2 centerTile, float tileSize)
    {
        float slope = 0.0f;
        for (int tx = 0; tx <= tileResolution / 2; tx++)
        {
            for (int ty = 0; ty <= tileResolution / 2; ty++)
            {
                float dx = tx + 0.5f;
                float dy = ty + 0.5f;

                if (dx * dx + dy * dy <= (tileResolution / 2) * (tileResolution / 2))
                {
                    int tileX = TransformOctant(tx, ty, octant);
                    int tileY = TransformOctant(ty, tx, octant);

                    Vector2 rayDir = new Vector2(tileX - centerTile.x, tileY - centerTile.y).normalized;
                    float distance = Vector2.Distance(new Vector2(tileX, tileY), centerTile) * tileSize;

                    RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, distance, shadowLayers);

                    if (hit.collider == null)
                    {
                        lightMap[tileX, tileY] = true;
                    }
                }
            }
        }
    }

    int TransformOctant(int x, int y, int octant)
    {
        switch (octant)
        {
            case 0: return tileResolution / 2 + x;
            case 1: return tileResolution / 2 + y;
            case 2: return tileResolution / 2 - y;
            case 3: return tileResolution / 2 - x;
            case 4: return tileResolution / 2 - x;
            case 5: return tileResolution / 2 - y;
            case 6: return tileResolution / 2 + y;
            case 7: return tileResolution / 2 + x;
            default: return 0;
        }
    }

    void ApplyLightMap()
    {
        for (int x = 0; x < tileResolution; x++)
        {
            for (int y = 0; y < tileResolution; y++)
            {
                if (lightMap[x, y])
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(tileResolution / 2, tileResolution / 2));
                    float intensity = 1 - (distance / (tileResolution / 2));
                    intensity = Mathf.Clamp01(intensity);
                    colorBuffer[y * tileResolution + x] = lightColor * intensity;
                }
                else
                {
                    colorBuffer[y * tileResolution + x] = Color.clear;
                }
            }
        }
    }
}