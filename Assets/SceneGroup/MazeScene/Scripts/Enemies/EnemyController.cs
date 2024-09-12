using SL.Lib;
using UnityEngine;


public class EnemyInput : IVirtualInput
{
    public Vector2 Movement { get; set; }
    Vector2 IVirtualInput.MovementInput => Movement.normalized;

    bool IVirtualInput.IsActionPressed => false;
}

public abstract class EnemyController : Character<EnemyInput>
{
    public float ThinkingInterval = 0.5f;

    private float lastThinkTime;

    public virtual float EnemyDamage(PlayerController player)
    {
        return PlayerStatusManager.MaxIntensity / 2f;
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    protected override void InitializeVirtualInput()
    {
        virtualInput = new EnemyInput();
    }

    protected override void HandleMovement()
    {
        if (MazeGameScene.Instance.GameTime - lastThinkTime >= ThinkingInterval)
        {
            lastThinkTime = MazeGameScene.Instance.GameTime;
            Think();
        }
        base.HandleMovement();
    }

    protected virtual void Think()
    {
        
    }

    public override void Initialize(Vector2 position, (int rows, int cols) mazeSize)
    {
        base.Initialize(position, mazeSize);
        lastThinkTime = 0f;
        OnInitialized();
    }

    protected virtual void OnInitialized() { } 
}