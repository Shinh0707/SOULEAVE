using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float requiredHoldTime = 1.0f;
    public TextMeshProUGUI textMesh;
    public string Text
    {
        get { return textMesh.text; }
        set { textMesh.text = value; }
    }
    public UnityEngine.Events.UnityEvent onLongPress;
    public UnityEngine.Events.UnityEvent onPress;

    private bool isPointerDown = false;
    private bool longPressTriggered = false;
    private float pointerDownTimer = 0f;

    private void Start()
    {
        // Ensure the GameObject has a Graphic component (like Image)
        if (GetComponent<Graphic>() == null)
        {
            Debug.LogWarning("LongPressButton requires a Graphic component to register input events. Adding a transparent Image component.");
            gameObject.AddComponent<Image>().color = Color.clear;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down detected");
        isPointerDown = true;
        longPressTriggered = false;
        pointerDownTimer = 0f;
        onPress.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Pointer Up detected");
        isPointerDown = false;
        longPressTriggered = false;
        pointerDownTimer = 0f;
    }

    private void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= requiredHoldTime)
            {
                longPressTriggered = true;
                onLongPress.Invoke();
                Debug.Log("Long Press detected");
            }
        }
    }
}