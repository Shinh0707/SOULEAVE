using UnityEngine;
using UnityEngine.UI;
using SL.Lib;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomUIGauge : MonoBehaviour
{
    public Sprite backgroundSprite;
    public Sprite gaugeSprite;
    public Shader gaugeShader;
    public Color backgroundColor = Color.white;
    public Color gaugeStartColor = Color.white;
    public Color gaugeEndColor = Color.white;

    [SerializeField]
    private GameObject backgroundObject;
    [SerializeField]
    private GameObject gaugeObject;
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Image gaugeImage;
    private Material gaugeMaterial;

    [Range(0f, 1f)]
    public float currentValue = 1f;

    public bool inverse = false;

    public float CurrentValue
    {
        get
        {
            return inverse? (1f-currentValue):currentValue;
        }
        set
        {
            if (value != currentValue)
            {
                currentValue = value;
                UpdateGaugeAppearance();
            }

        }
    }

    private bool isInitialized = false;

    public void GenerateGauge()
    {
        isInitialized &= backgroundObject != null && gaugeObject != null;
        if (isInitialized)
        {
            UpdateGaugeAppearance();
            return;
        }

        Transform canvasTransform = transform;

        // Create background
        backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(canvasTransform);
        backgroundObject.transform.localPosition = Vector3.zero;
        backgroundObject.AddComponent<CanvasRenderer>();
        backgroundImage = backgroundObject.AddComponent<Image>();
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();

        // Create gauge
        gaugeObject = new GameObject("Gauge");
        gaugeObject.transform.SetParent(canvasTransform);
        gaugeObject.transform.localPosition = Vector3.zero;
        gaugeObject.AddComponent<CanvasRenderer>();
        gaugeImage = gaugeObject.AddComponent<Image>();
        RectTransform gaugeRect = gaugeObject.GetComponent<RectTransform>();

        UpdateGaugeAppearance();
        Vector2 anchorMin = new Vector2(0.5f, 0f);
        Vector2 anchorMax = new Vector2(0.5f, 1f);
        backgroundRect.anchorMin = anchorMin;
        backgroundRect.anchorMax = anchorMax;
        backgroundRect.sizeDelta = Vector2.zero;
        gaugeRect.anchorMin = anchorMin;
        gaugeRect.anchorMax = anchorMax;
        gaugeRect.sizeDelta = Vector2.zero;

        isInitialized = true;
    }

    private void UpdateGaugeAppearance()
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = backgroundColor;
            SetAspect(backgroundObject.transform, backgroundSprite);
        }

        if (gaugeImage != null)
        {
            gaugeImage.sprite = gaugeSprite;
            SetAspect(gaugeObject.transform, gaugeSprite);

            if (gaugeShader != null)
            {
                if (gaugeMaterial == null || gaugeMaterial.shader != gaugeShader)
                {
                    gaugeMaterial = new Material(gaugeShader);
                }
                gaugeImage.material = gaugeMaterial;
            }
            else
            {
                gaugeImage.material = null;
            }
        }

        UpdateGaugeFill();
    }

    private void SetAspect(Transform targetTransform, Sprite sprite)
    {
        var width = sprite.rect.width;
        var height = sprite.rect.height;
        var ratio = width / height;
        var aspectRatioFitter = targetTransform.GetOrAddComponent<AspectRatioFitter>();
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        aspectRatioFitter.aspectRatio = ratio;
    }

    private void UpdateGaugeFill()
    {
        if (gaugeMaterial != null)
        {
            gaugeMaterial.SetFloat("_Shift", CurrentValue);
            gaugeMaterial.SetColor("_StartColor", gaugeStartColor);
            gaugeMaterial.SetColor("_EndColor", gaugeEndColor);
        }
    }

    private void OnValidate()
    {
        if (isInitialized)
        {
            UpdateGaugeAppearance();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomUIGauge))]
    public class CustomUIGaugeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CustomUIGauge gauge = (CustomUIGauge)target;

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                gauge.UpdateGaugeAppearance();
            }

            if (GUILayout.Button("Generate/Update Gauge"))
            {
                gauge.GenerateGauge();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(gauge);
            }
        }
    }
#endif
}