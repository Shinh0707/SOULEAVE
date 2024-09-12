using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input = UnityEngine.Input;
public class PlayerInput : IVirtualInput
{
    public Vector2 MovementInput => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    public bool IsActionPressed => Input.GetButtonDown("Fire1");
}

public class PlayerController : Character<PlayerInput>
{
    [SerializeField] private LightFlicker1fNoise targetLight;
    [SerializeField] private ParticleSystem wispParticle;

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
    private float _extreIntensity = 0f;
    public float ExtraIntensity
    {
        get 
        {
            return _extreIntensity;
        }
        set
        {
            if (_extreIntensity != value)
            {
                _extreIntensity = value;
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

    protected override void HandleAction()
    {
        // Skill
        foreach (var key in SkillBank.Keys)
        {
            if (Input.GetKey(key))
            {
                Debug.Log($"Key Pressed {key}");
                StartCoroutine(SkillBank[key].Use(this,key));
            }
        }
    }

    private void RestoreMP()
    {
        MP = Mathf.Min(MP + PlayerStatusManager.RestoreMPPerSecond * Time.fixedDeltaTime, PlayerStatusManager.MaxMP);
    }

    private void RestoreSight()
    {
        Intensity = Mathf.Min(Intensity + PlayerStatusManager.RestoreIntensityPerSecond * Time.fixedDeltaTime, PlayerStatusManager.MaxIntensity);
    }

    public override void Initialize(Vector2 position, (int rows, int cols) mazeSize)
    {
        base.Initialize(position, mazeSize);
        MP = PlayerStatusManager.MaxMP;
        Intensity = PlayerStatusManager.MaxIntensity;
        MaxSpeed = PlayerStatusManager.MaxSpeed;
        SkillBank ??= PlayerStatusManager.Instance.GetSkills();
        UpdateSightRange();
    }

    protected void UpdateSightRange()
    {
        SightRange = Intensity + ExtraIntensity;
        MazeGameScene.Instance.UIManager.UpdatePlayerStats(MP, Intensity);
        targetLight.BaseRange = SightRange;
        targetLight.BaseIntensity = Intensity + ExtraIntensity;
        wispParticle.transform.localScale = Vector3.one * Intensity/PlayerStatusManager.MaxIntensity;
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
        if (!_isTransparent && CurrentState == CharacterState.Alive)
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
        CurrentState = CharacterState.Invincible;
        // TODO: Implement invincibility visual effect
        yield return new WaitForGameSeconds(1f); // Adjust invincibility duration as needed
        CurrentState = CharacterState.Alive;
        // TODO: Remove invincibility visual effect
    }

    protected override void OnStateEnter(CharacterState newState)
    {
        switch (newState)
        {
            case CharacterState.Invincible:
                characterCollider.AddExcludeLayer("Character");
                break;
        }
    }
    protected override void OnStateExit(CharacterState newState)
    {
        switch (newState)
        {
            case CharacterState.Invincible:
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
        else if(CurrentState == CharacterState.Alive && collision.transform.TryGetComponent(out EnemyController enemyController))
        {
            TakeDamage(enemyController.EnemyDamage(this));
        }
    }

    protected override void InitializeVirtualInput()
    {
        virtualInput = new PlayerInput();
    }
}