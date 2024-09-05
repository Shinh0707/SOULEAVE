using UnityEngine;
using System.Collections.Generic;
using SL.Lib;
using System;
using Unity.VisualScripting;
public enum Direction { Up, Down, Left, Right }
public enum Condition { Alive, Dead, Invincible, Stopped, Disabled }

public class Character : MonoBehaviour
{
    [SerializeField] protected Collider2D characterCollider;
    [SerializeField] protected LayerMask collisionLayer;
    public Transform character;
    public new Transform transform => character;
    private CollisionDetector collisionDetector;
    protected Rigidbody2D rb;
    protected RaycastHit2D[] hitColliders = new RaycastHit2D[1];
    protected Vector2 movement;

    protected Queue<Vector2> movementQueue = new Queue<Vector2>();

    public Direction Direction { get; protected set; }
    public float CurrentSpeed => movement.magnitude;
    public float MaxSpeed = 5f;
    private Condition _currentCondition = Condition.Disabled;
    public Condition CurrentCondition
    {
        get
        {
            return _currentCondition;
        }
        set
        {
            if (_currentCondition != value)
            {
                var oldCondition = _currentCondition;
                OnEndCondition(oldCondition);
                OnStartCondition(value);
                _currentCondition = value;
                OnChangeCondition(oldCondition,CurrentCondition);
            }
        }
    }

    protected virtual void OnChangeCondition(Condition before, Condition after)
    {
    }
    protected virtual void OnEndCondition(Condition endCondition) { }
    protected virtual void OnStartCondition(Condition startCondition) { }

    public float SightRange = 5f;

    protected virtual void Awake()
    {
        if (!character.TryGetComponent(out collisionDetector))
        {
            collisionDetector = character.AddComponent<CollisionDetector>();
        }
        rb = collisionDetector.rigidbody2d;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.interpolation = RigidbodyInterpolation2D.Extrapolate;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        characterCollider.contactCaptureLayers = collisionLayer;
        characterCollider.AddIncludeLayer("Character"); 
        collisionDetector.OnCollisionDetected += OnCollision;
    }

    protected virtual void FixedUpdate()
    {
        if (MazeGameScene.Instance.CurrentState == MazeGameScene.GameState.Playing)
        {
            if (CurrentCondition == Condition.Alive || CurrentCondition == Condition.Invincible)
            {
                ProcessMovementQueue();
                Move();
            }
        }
    }

    public void SetColliderActive(bool isActive) => characterCollider.enabled = isActive;

    public virtual void MoveForward() => EnqueueMovement(Vector2.up);
    public virtual void MoveLeft() => EnqueueMovement(Vector2.left);
    public virtual void MoveRight() => EnqueueMovement(Vector2.right);
    public virtual void MoveBackward() => EnqueueMovement(Vector2.down);

    protected virtual void EnqueueMovement(Vector2 direction)
    {
        movementQueue.Enqueue(direction.normalized);
    }

    protected virtual void ProcessMovementQueue()
    {
        Vector2 totalMovement = Vector2.zero;
        while (movementQueue.Count > 0)
        {
            totalMovement += movementQueue.Dequeue();
        }
        movement = Vector2.ClampMagnitude(totalMovement, 1f) * MaxSpeed;
    }

    public virtual void Pause() => CurrentCondition = Condition.Stopped;
    public virtual void Resume() => CurrentCondition = Condition.Alive;

    protected virtual void OnBeforeMove() { }
    protected virtual void OnAfterMove() { }

    public virtual void Initialize(Vector2 position, (int rows, int cols) mazeSize)
    {
        movement = Vector2.zero;
        Direction = Direction.Up;
        CurrentCondition = Condition.Alive;
        rb.position = position;
        movementQueue.Clear();
    }

    protected virtual void Move()
    {
        OnBeforeMove();

        rb.MovePosition(rb.position+movement*Time.fixedDeltaTime);

        if (movement != Vector2.zero)
        {
            Direction = GetDirectionFromVector(movement);
        }

        OnAfterMove();
    }

    protected Direction GetDirectionFromVector(Vector2 vector)
    {
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
        {
            return vector.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return vector.y > 0 ? Direction.Up : Direction.Down;
        }
    }

    protected virtual void OnCollision(Collision2D collision)
    {

    }
}