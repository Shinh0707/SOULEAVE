using UnityEngine;
using System;
using System.Collections.Generic;
using SL.Lib;
using System.Linq;

public enum Direction { Up, Down, Left, Right }

[Flags]
public enum CharacterState {
    Dead = 0,
    Alive = 1, 
    Invincible = 2, 
    Stopped = 4, 
    Disabled = 5
}

public interface IVirtualInput
{
    Vector2 MovementInput { get; }
    bool IsActionPressed { get; }
}

public interface ICharacterController
{
    public string Name { get; }
    public string ID { get; }
    public Vector2 Position { get; }

    public void OnUpdate();
    public void UpdateState();
    public void HandleInput();

}

public class ICharacterControllerManagaer<TController> where TController : ICharacterController
{
    public static Dictionary<string, TController> InstantinatedControllers = new();

    public static string Add(TController controller)
    {
        string id = Guid.NewGuid().ToString();
        InstantinatedControllers.Add(id, controller);
        return id;
    }
    public static void Remove(string id)
    {
        if (InstantinatedControllers.ContainsKey(id))
        {
            InstantinatedControllers.Remove(id);
        }
    }

    public static bool HasID(string id) => InstantinatedControllers.ContainsKey(id);
    public static TController GetController(string id) => InstantinatedControllers[id];
    public static List<TController> GetOtherControllers(string id) => InstantinatedControllers.Where(p => p.Key != id).Select(p => p.Value).ToList();
    public static void OnUpdate()
    {
        var controllers = InstantinatedControllers.Select(c => c.Key).ToList();
        foreach (var id in controllers)
        {
            if (InstantinatedControllers.ContainsKey(id))
            {
                InstantinatedControllers[id].OnUpdate();
            }
        }
    }
    public static void UpdateState()
    {
        var controllers = InstantinatedControllers.Select(c => c.Key).ToList();
        foreach (var id in controllers)
        {
            if (InstantinatedControllers.ContainsKey(id))
            {
                InstantinatedControllers[id].UpdateState();
            }
        }
    }
    public static void HandleInput()
    {
        var controllers = InstantinatedControllers.Select(c => c.Key).ToList();
        foreach (var id in controllers)
        {
            if (InstantinatedControllers.ContainsKey(id))
            {
                InstantinatedControllers[id].HandleInput();
            }
        }
    }
}

public abstract class Character<TInput> : DynamicObject, ICharacterController where TInput : IVirtualInput
{
    [SerializeField] protected LayerMask collisionLayer;
    [SerializeField] public GameObject character;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    
    public string Name => character.name;
    protected string id = null;
    public string ID => id;
    public Vector2 Position
    {
        get 
        {
            if(rb == null) return character.transform.position;
            return rb.position;
        }
        set
        {
            if (rb == null) character.transform.position = value;
            else rb.position = value;
        }
    }

    protected Rigidbody2D rb;
    protected Collider2D characterCollider;

    private Dictionary<string, ContactFilter2D> contactFilters = new Dictionary<string, ContactFilter2D>();
    private List<Collider2D> collisionResults = new List<Collider2D>(4);
    public float CurrentSpeed => rb.velocity.magnitude;
    public float MaxSpeed = 5f;
    public float RotationSpeed = 0.1f;
    public float CurrentRotation = 0f;
    private CharacterState currentState = CharacterState.Disabled;
    public CharacterState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState != value)
            {
                CharacterState oldState = currentState;
                currentState = value;
                OnStateExit(oldState);
                OnStateEnter(currentState);
                OnStateChange(oldState, currentState);
            }
        }
    }

    public bool IsAlive => CurrentState.HasFlag(CharacterState.Alive);
    public bool IsDead => !IsAlive;

    public float _sightRange = 5f;

    public float SightRange { 
        get => _sightRange;
        set 
        {
            _sightRange = value;
        } 
    }

    private float _facingDirection = 0f;

    public float FacingDirection
    {
        get
        {
            return _facingDirection;
        }
        set
        {
            CurrentRotation = _facingDirection;
            _facingDirection = value;
        }
    }

    protected TInput virtualInput;

    protected virtual void Awake()
    {   
        characterCollider = character.GetOrAddComponent<Collider2D>();
        if (spriteRenderer == null)
        {
            if (!character.TryGetComponent(out spriteRenderer))
            {
                if (TryGetComponent(out spriteRenderer))
                {

                }
            }
        }
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

    public override void OnUpdate()
    {
        base.OnUpdate();
        rb.Sleep();
    }

    public void HandleInput()
    {
        if (IsAlive)
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

    protected void UpdateFacingDirection(Vector2 movement)
    {
        if (movement != Vector2.zero)
        {
            FacingDirection = movement.Atan2() * Mathf.Rad2Deg;
        }
        float current = character.transform.eulerAngles.z;
        float dir = Mathf.DeltaAngle(current, FacingDirection);
        float d = RotationSpeed * Time.deltaTime;

        if (Mathf.Abs(dir) > d)
        {
            float newAngle = current + Mathf.Sign(dir) * d;
            character.transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        else
        {
            character.transform.rotation = Quaternion.Euler(0, 0, FacingDirection);
        }
    }
    public virtual void Initialize(Vector2 position)
    {
        rb.position = position;
        FacingDirection = 0f;
        CurrentState |= CharacterState.Alive;
    }
    public virtual void Warp(Vector2 targetPosition)
    {
        characterCollider.enabled = false;
        rb.position = targetPosition;
        characterCollider.enabled = true;
    }
    protected virtual void OnAfterMove() 
    {
        if (spriteRenderer != null)
        {
            var currentPosIntensity = MazeGameScene.Instance.MazeManager.GetIntensity(Position);
            if (currentPosIntensity > 0)
            {
                spriteRenderer.color = spriteRenderer.color.SetAlpha(1f);
            }
            else
            {
                spriteRenderer.color = spriteRenderer.color.SetAlpha(0);
            }
        }
    }
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
