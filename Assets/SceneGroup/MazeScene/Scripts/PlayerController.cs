using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Character
{
    [SerializeField] private TileBased2DLight tileBased2DLight;

    private float _mp;
    public float MP
    {
        get { return _mp; }
        set {
            if (_mp != value)
            {
                _mp = value;
                UpdateMP();
            }
        }
    }

    private float _intensity;
    public float Intensity {
        get { return _intensity; }
        set
        {
            if (_intensity != value)
            {
                _intensity = value;
                UpdateSightRange();
            }
        }
    }

    public Dictionary<KeyCode, SkillManager> SkillBank { get; private set; }

    public bool IsDead => Intensity <= 0;

    private bool _isTransparent;
    private Coroutine _transparencyCoroutine;

    public override void UpdateState()
    {
        RestoreMP();
        RestoreSight();
    }

    public override void HandleInput()
    {
        if (CurrentCondition != Condition.Alive && CurrentCondition != Condition.Invincible)
            return;

        // Movement
        if (Input.GetKey(KeyCode.W)) MoveForward();
        if (Input.GetKey(KeyCode.A)) MoveLeft();
        if (Input.GetKey(KeyCode.S)) MoveBackward();
        if (Input.GetKey(KeyCode.D)) MoveRight();

        // Skill
        foreach (var key in SkillBank.Keys)
        {
            if (Input.GetKey(key))
            {
                Debug.Log($"Key Pressed {key}");
                StartCoroutine(SkillBank[key].Use(this,key));
            }
        }

        // Items
        if (Input.GetKeyDown(KeyCode.Alpha1)) ;
        if (Input.GetKeyDown(KeyCode.Alpha2)) ;
    }

    private void RestoreMP()
    {
        MP = Mathf.Min(MP + MazeGameStats.Instance.RestoreMPPerSecond * Time.fixedDeltaTime, MazeGameStats.Instance.MaxMP);
    }

    private void RestoreSight()
    {
        Intensity = Mathf.Min(Intensity + MazeGameStats.Instance.RestoreIntensityPerSecond * Time.fixedDeltaTime, MazeGameStats.Instance.MaxIntensity);
    }

    public override void Initialize(Vector2 position, (int rows, int cols) mazeSize)
    {
        base.Initialize(position, mazeSize);
        MP = MazeGameStats.Instance.MaxMP;
        Intensity = MazeGameStats.Instance.MaxIntensity;
        SkillBank ??= PlayerStatusManager.Instance.GetSkills();
        UpdateSightRange();
    }

    protected void UpdateSightRange()
    {
        SightRange = MazeGameStats.Instance.VisibleBorder + Intensity;
        MazeGameScene.Instance.UIManager.UpdatePlayerStats(MP, Intensity);
        tileBased2DLight.LightRange = SightRange;
    }
    protected void UpdateMP()
    {
        MazeGameScene.Instance.UIManager.UpdatePlayerStats(MP, Intensity);
    }

    protected override void OnAfterMove()
    {
        base.OnAfterMove();
        UpdateSightRange();
    }

    public void TakeDamage(float damage)
    {
        if (!_isTransparent && CurrentCondition == Condition.Alive)
        {
            // TODO: Implement damage logic
            Debug.Log($"Player took {damage} damage");
            Intensity -= damage;
            Intensity = Mathf.Max(Intensity, 0 );
            UpdateSightRange();
            StartCoroutine(InvincibilityCoroutine());
            
        }
    }

    public IEnumerator InvincibilityCoroutine()
    {
        CurrentCondition = Condition.Invincible;
        // TODO: Implement invincibility visual effect
        yield return new WaitForGameSeconds(1f); // Adjust invincibility duration as needed
        CurrentCondition = Condition.Alive;
        // TODO: Remove invincibility visual effect
    }

    protected override void OnStartCondition(Condition startCondition)
    {
        switch (startCondition)
        {
            case Condition.Invincible:
                characterCollider.AddExcludeLayer("Character");
                break;
        }
    }
    protected override void OnEndCondition(Condition startCondition)
    {
        switch (startCondition)
        {
            case Condition.Invincible:
                characterCollider.RemoveExcludeLayer("Character");
                break;
        }
    }

    protected override void OnCollision(Collision2D collision)
    {
        Debug.Log($"Player collision to {collision.transform.name}");
        if(collision.transform.tag == "Goal")
        {
            MazeGameScene.Instance.Victory();
        }
        else if(CurrentCondition == Condition.Alive && collision.transform.TryGetComponent(out EnemyController enemyController))
        {
            //‰½‚ç‚©‚Ìˆ—
            TakeDamage(MazeGameStats.Instance.EnemyDamage);
        }
    }
}