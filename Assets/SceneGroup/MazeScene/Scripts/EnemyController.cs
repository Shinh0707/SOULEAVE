using SL.Lib;
using UnityEngine;

public class EnemyController : Character
{
    public float ThinkingInterval = 0.5f;
    public Vector2 TargetPosition { get; protected set; }
    public Tensor<int> Memory { get; protected set; }
    public enum EnemyState { Exploring, Chasing, Waiting }
    public EnemyState State { get; protected set; } = EnemyState.Exploring;

    private float lastThinkTime;

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void HandleInput()
    {
        if (MazeGameScene.Instance.GameTime - lastThinkTime >= ThinkingInterval)
        {
            lastThinkTime = MazeGameScene.Instance.GameTime;
            Think();
        }
    }

    protected void Think()
    {
        // Implement enemy AI logic here
        for (int i = 0; i < 4; i++)
        {
            MoveEnemy((MoveType)(SLRandom.Random.Next(3)));
        }
    }

    public enum MoveType
    {
        Forward,
        Backward,
        Left,
        Right
    }

    protected void MoveEnemy(MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.Forward: MoveForward(); break;
            case MoveType.Backward: MoveBackward(); break;
            case MoveType.Left: MoveLeft(); break;
            case MoveType.Right: MoveRight(); break;
        }
        
    }

    public override void Initialize(Vector2 position, (int rows, int cols) mazeSize)
    {
        base.Initialize(position, mazeSize);
        State = EnemyState.Exploring;
        Memory = Tensor<int>.Full(-1,mazeSize.rows, mazeSize.cols); // Initialize memory with maze size
        lastThinkTime = 0f;
    }
}