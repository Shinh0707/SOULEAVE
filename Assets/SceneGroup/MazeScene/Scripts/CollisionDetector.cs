using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class CollisionDetector : MonoBehaviour
{
    public event Action<Collision2D> OnCollisionDetected;

    public Rigidbody2D rigidbody2d => transform.GetComponent<Rigidbody2D>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionDetected?.Invoke(collision);
    }
}