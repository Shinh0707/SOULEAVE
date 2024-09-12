using UnityEngine;
using System;
using System.Collections.Generic;
using SL.Lib;

public enum Direction { Up, Down, Left, Right }
public enum CharacterState { Alive, Dead, Invincible, Stopped, Disabled }

public interface IVirtualInput
{
    Vector2 MovementInput { get; }
    bool IsActionPressed { get; }
}

public abstract class Character<TInput> : DynamicObject where TInput : IVirtualInput
{
    [SerializeField] protected LayerMask collisionLayer;
    [SerializeField] public GameObject character;
    
    public Vector2 Position
    {
        get 
        {
            return rb.position;
        }
        set
        {
            rb.position = value;
        }
    }

    protected Rigidbody2D rb;
    protected Collider2D characterCollider;

    private Dictionary<string, ContactFilter2D> contactFilters = new Dictionary<string, ContactFilter2D>();
    private List<Collider2D> collisionResults = new List<Collider2D>(4);

    public Direction FacingDirection { get; protected set; }
    public float CurrentSpeed => rb.velocity.magnitude;
    public float MaxSpeed = 5f;

    private CharacterState currentState = CharacterState.Disabled;
    public CharacterState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState != value)
            {
                CharacterState oldState = currentState;
                OnStateExit(oldState);
                currentState = value;
                OnStateEnter(currentState);
                OnStateChange(oldState, currentState);
            }
        }
    }

    public float SightRange = 5f;

    protected TInput virtualInput;

    protected virtual void Awake()
    {   
        characterCollider = character.GetOrAddComponent<Collider2D>();
        CollisionDetector collisionDetector = character.GetOrAddComponent<CollisionDetector>();
        rb = collisionDetector.rigidbody2d;
        collisionDetector.OnCollisionDetected -= OnCollisionCheck;
        collisionDetector.OnCollisionDetected += OnCollisionCheck;

        InitializeRigidbody();
        InitializeVirtualInput();
    }

    protected abstract void InitializeVirtualInput();

    private void InitializeRigidbody()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        rb.Sleep();
    }

    public void HandleInput()
    {
        if (CurrentState == CharacterState.Alive || CurrentState == CharacterState.Invincible)
        {
            rb.WakeUp();
            HandleMovement();
            HandleAction();
        }
    }

    protected virtual void HandleMovement()
    {
        Vector2 movement = virtualInput.MovementInput * MaxSpeed;

        // Option 1: Use velocity for smoother movement
        rb.velocity = movement;

        // Option 2: Manual position update (uncomment to use)
        // Vector2 newPosition = rb.position + movement * Time.deltaTime;
        // if (!CollisionWall(newPosition) && !CollisionOtherCharacter(newPosition))
        // {
        //     rb.position = newPosition;
        // }

        UpdateFacingDirection(movement);
        OnAfterMove();
    }

    protected virtual void HandleAction()
    {
        if (virtualInput.IsActionPressed)
        {
            PerformAction();
        }
    }

    protected virtual void PerformAction()
    {
        // Override this in derived classes to implement specific actions
    }

    public bool CheckCollision(string layerName, Vector2? position = null)
    {
        if (!contactFilters.TryGetValue(layerName, out ContactFilter2D filter))
        {
            filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = LayerMask.GetMask(layerName)
            };
            contactFilters[layerName] = filter;
        }

        Vector2 originalPosition = rb.position;
        if (position.HasValue)
        {
            rb.position = position.Value;
        }

        int collisionCount = Physics2D.OverlapCollider(characterCollider, filter, collisionResults);

        if (position.HasValue)
        {
            rb.position = originalPosition;
        }

        return collisionCount > 0;
    }

    public bool CollisionWall(Vector2? position = null) => CheckCollision("Wall", position);
    public bool CollisionOtherCharacter(Vector2? position = null) => CheckCollision("Character", position);

    protected void UpdateFacingDirection(Vector2 movement)
    {
        if (movement != Vector2.zero)
        {
            FacingDirection = Mathf.Abs(movement.x) > Mathf.Abs(movement.y)
                ? (movement.x > 0 ? Direction.Right : Direction.Left)
                : (movement.y > 0 ? Direction.Up : Direction.Down);
        }
    }
    public virtual void Initialize(Vector2 position, (int rows, int cols) mazeSize)
    {
        rb.position = position;
        FacingDirection = Direction.Up;
        CurrentState = CharacterState.Alive;
    }
    public virtual void Warp(Vector2 targetPosition)
    {
        characterCollider.enabled = false;
        rb.position = targetPosition;
        characterCollider.enabled = true;
    }
    protected virtual void OnAfterMove() { }
    protected virtual void OnStateEnter(CharacterState newState) { }
    protected virtual void OnStateExit(CharacterState oldState) { }
    protected virtual void OnStateChange(CharacterState oldState, CharacterState newState) { }

    private void OnCollisionCheck(Collision2D collision)
    {
        if (!characterCollider.enabled) return;
        if (MazeGameScene.Instance.CurrentState != MazeGameScene.GameState.Playing) return;
        OnCollision(collision);
    }

    protected virtual void OnCollision(Collision2D collision)
    {

    }

}
