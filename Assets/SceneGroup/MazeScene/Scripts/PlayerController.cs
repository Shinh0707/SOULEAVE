using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Character
{
    public float MP { get; private set; }
    public float Intensity { get; private set; }

    public bool IsDead => Intensity <= 0;

    private float _lastMPRestoreTime;
    private float _lastSightRestoreTime;
    private bool _isTransparent;
    private Coroutine _transparencyCoroutine;

    private void Update()
    {
        HandleInput();
        RestoreMP();
        RestoreSight();
    }

    private void HandleInput()
    {
        if (CurrentCondition != Condition.Alive && CurrentCondition != Condition.Invincible)
            return;

        // Movement
        if (Input.GetKey(KeyCode.W)) MoveForward();
        if (Input.GetKey(KeyCode.A)) MoveLeft();
        if (Input.GetKey(KeyCode.S)) MoveBackward();
        if (Input.GetKey(KeyCode.D)) MoveRight();

        // Magic
        if (Input.GetKeyDown(KeyCode.E)) UseHint();
        if (Input.GetKeyDown(KeyCode.R)) UseTeleport();
        if (Input.GetKey(KeyCode.T)) UseIntensify();

        // Items
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseTransparency();
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseItem(1);
    }

    private void RestoreMP()
    {
        if (Time.time - _lastMPRestoreTime >= 1f)
        {
            MP = Mathf.Min(MP + MazeGameStats.Instance.RestoreMPPerSecond, MazeGameStats.Instance.MaxMP);
            _lastMPRestoreTime = Time.time;
        }
    }

    private void RestoreSight()
    {
        if (Time.time - _lastSightRestoreTime >= 1f)
        {
            Intensity = Mathf.Min(Intensity + MazeGameStats.Instance.RestoreSightPerSecond, MazeGameStats.Instance.MaxSight);
            _lastSightRestoreTime = Time.time;
            UpdateSightRange();
        }
    }

    protected virtual void UseHint()
    {
        if (CanUseMagic(MazeGameStats.Instance.HintMPCost))
        {
            MP -= MazeGameStats.Instance.HintMPCost;
            // TODO: Implement hint logic
            Debug.Log("Using Hint");
            StartCoroutine(HintCoroutine());
        }
    }

    protected virtual void UseTeleport()
    {
        if (CanUseMagic(MazeGameStats.Instance.TeleportMPCost) && Intensity >= MazeGameStats.Instance.MinSightForTeleport)
        {
            MP -= MazeGameStats.Instance.TeleportMPCost;
            // TODO: Implement teleport logic
            Debug.Log("Using Teleport");
        }
    }

    protected virtual void UseIntensify()
    {
        if (CanUseMagic(MazeGameStats.Instance.MPForBrightnessCostPerSecond * Time.deltaTime))
        {
            MP -= MazeGameStats.Instance.MPForBrightnessCostPerSecond * Time.deltaTime;
            Intensity += MazeGameStats.Instance.MPForBrightnessValuePerSecond * Time.deltaTime;
            Intensity = Mathf.Min(Intensity, MazeGameStats.Instance.MaxSight);
            UpdateSightRange();
        }
    }

    protected virtual void UseTransparency()
    {
        if (!_isTransparent)
        {
            _isTransparent = true;
            if (_transparencyCoroutine != null)
            {
                StopCoroutine(_transparencyCoroutine);
            }
            _transparencyCoroutine = StartCoroutine(TransparencyCoroutine());
        }
    }

    protected virtual void UseItem(int itemIndex)
    {
        // TODO: Implement other item usage
        Debug.Log($"Using item {itemIndex}");
    }

    protected virtual bool CanUseMagic(float cost) => MP >= cost;

    public override void Initialize(Vector2 position, (int rows, int cols) mazeSize)
    {
        base.Initialize(position, mazeSize);
        MP = MazeGameStats.Instance.MaxMP;
        Intensity = MazeGameStats.Instance.MaxSight;
        UpdateSightRange();
        _lastMPRestoreTime = Time.time;
        _lastSightRestoreTime = Time.time;
    }

    protected void UpdateSightRange()
    {
        SightRange = MazeGameStats.Instance.VisibleBorder + Intensity;
        MazeGameScene.Instance.UIManager.UpdatePlayerStats(MP, Intensity);
        MazeGameScene.Instance.MazeManager.UpdateVisibility(transform.position, Intensity, Color.cyan);
    }

    protected override void OnAfterMove()
    {
        base.OnAfterMove();
        UpdateSightRange();
    }

    private IEnumerator HintCoroutine()
    {
        // TODO: Implement hint visual effect
        yield return new WaitForSeconds(MazeGameStats.Instance.HintDuration);
        // TODO: Remove hint visual effect
    }

    private IEnumerator TransparencyCoroutine()
    {
        // TODO: Implement transparency visual effect
        yield return new WaitForSeconds(MazeGameStats.Instance.TransparentDuration);
        _isTransparent = false;
        // TODO: Remove transparency visual effect
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

    private IEnumerator InvincibilityCoroutine()
    {
        CurrentCondition = Condition.Invincible;
        // TODO: Implement invincibility visual effect
        yield return new WaitForSeconds(1f); // Adjust invincibility duration as needed
        CurrentCondition = Condition.Alive;
        // TODO: Remove invincibility visual effect
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Player collision to {collision.transform.name}");
        if(collision.transform.tag == "Goal")
        {
            MazeGameScene.Instance.Victory();
        }
        else if(CurrentCondition == Condition.Alive && collision.transform.TryGetComponent(out EnemyController enemyController))
        {
            //âΩÇÁÇ©ÇÃèàóù
            TakeDamage(MazeGameStats.Instance.EnemyDamage);
        }
    }
}