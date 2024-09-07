using UnityEngine;
using UnityEngine.UI;

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

    private GameObject backgroundObject;
    private GameObject gaugeObject;
    private Image backgroundImage;
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
        if (isInitialized && backgroundObject != null && gaugeObject != null)
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

        // Set size
        Vector2 size = new Vector2(backgroundSprite.rect.width, backgroundSprite.rect.height);
        backgroundRect.sizeDelta = size;
        gaugeRect.sizeDelta = size;

        isInitialized = true;
    }

    private void UpdateGaugeAppearance()
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = backgroundColor;
        }

        if (gaugeImage != null)
        {
            gaugeImage.sprite = gaugeSprite;

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