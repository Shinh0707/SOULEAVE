using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SpriteButtonHandler : MonoBehaviour
{
    public Action OnPointerDown;
    public Action OnPointerUp;
    public Action OnClick;

    private bool isPointerDown = false;
    private Camera mainCamera;
    private Collider2D spriteCollider;

    private void Start()
    {
        mainCamera = Camera.main;
        spriteCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (spriteCollider.OverlapPoint(mousePosition))
            {
                isPointerDown = true;
                OnPointerDown?.Invoke();
                Debug.Log("押され始めました");
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isPointerDown)
            {
                OnPointerUp?.Invoke();
                Debug.Log("離されました");

                if (spriteCollider.OverlapPoint(mousePosition))
                {
                    OnClick?.Invoke();
                    Debug.Log("クリックされました");
                }
            }
            isPointerDown = false;
        }
    }
}