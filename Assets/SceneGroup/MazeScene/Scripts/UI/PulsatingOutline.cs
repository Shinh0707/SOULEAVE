using UnityEngine;
using UnityEngine.UI;

public class PulsatingOutline : MonoBehaviour
{
    [Header("UI Components")]
    public Image outlineImage;
    public Image coverImage;

    [Header("Sprite Options")]
    public bool useSpriteSwitching = false;
    public Sprite outlineSprite;
    public Sprite coverSprite;

    [Header("Scale Settings")]
    [Range(0.5f, 2f)]
    public float minScale = 1f;
    [Range(0.5f, 2f)]
    public float maxScale = 1.2f;

    [Header("Color Settings")]
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;
    public Color chargeColor = Color.yellow;

    [Header("Animation Settings")]
    public float pulseDuration = 2.0f;
    public float pauseDuration = 1.0f;

    private float elapsedTime = 0f;
    private bool isPulsing = false;
    private float currentValue = 0f;

    private void Start()
    {
        if (useSpriteSwitching && (outlineSprite == null || coverSprite == null))
        {
            Debug.LogError("Sprites are not assigned for sprite switching mode!");
            enabled = false;
            return;
        }

        if (!useSpriteSwitching && (outlineImage == null || coverImage == null))
        {
            Debug.LogError("Images are not assigned for separate image mode!");
            enabled = false;
            return;
        }

        // 初期スケールを設定
        SetScale(minScale);
    }

    public void StartPulsing()
    {
        isPulsing = true;
        elapsedTime = 0f;
        SetValue(0f);
    }

    public void SetValue(float value)
    {
        currentValue = Mathf.Clamp01(value);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (currentValue == 0f)
        {
            // パルス状態
            if (isPulsing)
            {
                float t = (elapsedTime % (pulseDuration + pauseDuration)) / pulseDuration;
                if (t <= 1f)
                {
                    float pulseScale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(t * Mathf.PI * 2 - Mathf.PI / 2) + 1) / 2);
                    SetScale(pulseScale);
                }
                else
                {
                    SetScale(minScale);
                }
                SetColor(activeColor);
            }
        }
        else if (currentValue == 1f)
        {
            // 無効状態
            SetScale(minScale);
            SetColor(inactiveColor);
            isPulsing = false;
        }
        else
        {
            // チャージ状態
            float chargeScale = Mathf.Lerp(minScale, maxScale, currentValue);
            SetScale(chargeScale);
            SetColor(Color.Lerp(activeColor, chargeColor, currentValue));
            isPulsing = false;
        }

        UpdateCoverVisibility();
    }

    private void SetScale(float scale)
    {
        if (useSpriteSwitching)
        {
            outlineImage.transform.localScale = Vector3.one * scale;
        }
        else
        {
            outlineImage.transform.localScale = Vector3.one * scale;
            coverImage.transform.localScale = Vector3.one * scale;
        }
    }

    private void SetColor(Color color)
    {
        if (useSpriteSwitching)
        {
            outlineImage.color = color;
        }
        else
        {
            outlineImage.color = color;
            coverImage.color = color;
        }
    }

    private void UpdateCoverVisibility()
    {
        if (useSpriteSwitching)
        {
            outlineImage.sprite = (currentValue > 0f) ? coverSprite : outlineSprite;
        }
        else
        {
            coverImage.enabled = (currentValue > 0f);
        }
    }

    void FixedUpdate()
    {
        if (isPulsing)
        {
            elapsedTime += Time.fixedDeltaTime;
            UpdateVisuals();
        }
    }
}