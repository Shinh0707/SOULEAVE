using UnityEngine;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform m_RectTransform;
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private int showCount = 5;
    [SerializeField] private float startX = 0f;
    [SerializeField] private float startScale = 10f;
    [SerializeField] private float endX = 0f;
    [SerializeField] private float endScale = 100f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float endShowDuration = 1f;
    [SerializeField] private bool firstHide = true;

    private float elapsedTime = 0f;
    private int currentStep = 0;
    private float stepDuration;
    private Vector2 initialSize;

    private void OnEnable()
    {
        if (m_RectTransform == null || targetGraphic == null)
        {
            Debug.LogError("Required components are missing!");
            enabled = false;
            return;
        }
        elapsedTime = 0;
        currentStep = 0;
        initialSize = m_RectTransform.sizeDelta.normalized;
        stepDuration = (duration - endShowDuration) / (showCount * 2 - 1);
        SetInitialState();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= duration)
        {
            elapsedTime = 0;
            currentStep = 0;
            SetInitialState();
        }
        else if (elapsedTime >= duration - endShowDuration)
        {
            SetFinalState();
        }
        else
        {
            UpdateStep();
        }
    }

    private void SetInitialState()
    {
        m_RectTransform.anchoredPosition = new Vector2(startX, m_RectTransform.anchoredPosition.y);
        m_RectTransform.sizeDelta = initialSize * startScale;
        targetGraphic.enabled = !firstHide;
    }

    private void SetFinalState()
    {
        m_RectTransform.anchoredPosition = new Vector2(endX, m_RectTransform.anchoredPosition.y);
        m_RectTransform.sizeDelta = initialSize * endScale;
        targetGraphic.enabled = true;
    }

    private void UpdateStep()
    {
        int newStep = Mathf.FloorToInt(elapsedTime / stepDuration);
        if (newStep != currentStep)
        {
            currentStep = newStep;
            targetGraphic.enabled = currentStep % 2 == (firstHide ? 1 : 0);

            if (targetGraphic.enabled)
            {
                float t = (float)currentStep / (showCount * 2 - 2);
                m_RectTransform.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, t), m_RectTransform.anchoredPosition.y);
                float scale = Mathf.Lerp(startScale, endScale, t);
                m_RectTransform.sizeDelta = initialSize * scale;
            }
        }
    }
}
