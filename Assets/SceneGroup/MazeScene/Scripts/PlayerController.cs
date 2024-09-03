using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Vector2Int position;
    private float mp;
    private float sight;
    private float extraSight;
    private bool isTeleportMode;
    private float transparentTimer;
    private List<GameItem> items = new List<GameItem>();

    private MazeManager mazeManager;
    private GameUIManager uiManager;

    public bool IsDead => sight <= 0;
    public Vector2Int Position => position;

    public void Initialize(Vector2Int startPosition)
    {
        position = startPosition;
        mazeManager = MazeGameScene.Instance.MazeManager;
        uiManager = MazeGameScene.Instance.UIManager;

        mp = MazeGameStats.Instance.MaxMP;
        sight = MazeGameStats.Instance.MaxSight;
        extraSight = 0;
        isTeleportMode = false;
        transparentTimer = 0;

        transform.position = mazeManager.GetWorldPosition(position);
        InitializeItems();
        UpdateUI();
    }

    private void InitializeItems()
    {
        // アイテムの初期化ロジックを実装
    }

    private void Update()
    {
        if (MazeGameScene.Instance.CurrentState == MazeGameScene.GameState.Playing)
        {
            HandleInput();
            UpdateStats();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) TryMove(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) TryMove(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) TryMove(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) TryMove(Vector2Int.right);

        if (Input.GetKeyDown(KeyCode.Space)) UseHint();
        if (Input.GetKeyDown(KeyCode.Q)) ToggleTeleportMode();
        if (Input.GetKey(KeyCode.E)) UseMpToBrightness();

        for (int i = 0; i < items.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseItem(i);
            }
        }
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = position + direction;
        if (mazeManager.IsValidMove(newPosition))
        {
            position = newPosition;
            transform.position = mazeManager.GetWorldPosition(position);
            CheckForEnemies();
        }
    }

    private void UseHint()
    {
        if (mp >= MazeGameStats.Instance.HintMPCost)
        {
            mp -= MazeGameStats.Instance.HintMPCost;
            uiManager.ShowHint();
        }
    }

    private void ToggleTeleportMode()
    {
        if (mp >= MazeGameStats.Instance.TeleportMPCost && sight >= MazeGameStats.Instance.MinSightForTeleport && extraSight == 0)
        {
            isTeleportMode = !isTeleportMode;
            uiManager.UpdateTeleportMode(isTeleportMode);
        }
    }

    private void UseMpToBrightness()
    {
        float mpCost = MazeGameStats.Instance.MPForBrightnessCostPerSecond * Time.deltaTime;
        if (mp >= mpCost && !isTeleportMode)
        {
            mp -= mpCost;
            extraSight += MazeGameStats.Instance.MPForBrightnessValuePerSecond * Time.deltaTime;
        }
        else if (extraSight > 0)
        {
            extraSight -= MazeGameStats.Instance.MPForBrightnessDecayPerSecond * Time.deltaTime;
            extraSight = Mathf.Max(0, extraSight);
        }
    }

    private void UseItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            items[index].Use();
        }
    }

    private void CheckForEnemies()
    {
        if (MazeGameScene.Instance.EnemyManager.IsEnemyAtPosition(position))
        {
            TakeDamage(MazeGameStats.Instance.EnemyDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        sight -= damage;
        if (sight <= 0)
        {
            sight = 0;
            // Game Over logic will be handled in MazeGameScene
        }
        UpdateUI();
    }

    private void UpdateStats()
    {
        mp = Mathf.Min(MazeGameStats.Instance.MaxMP, mp + MazeGameStats.Instance.RestoreMPPerSecond * Time.deltaTime);
        sight = Mathf.Min(MazeGameStats.Instance.MaxSight, sight + MazeGameStats.Instance.RestoreSightPerSecond * Time.deltaTime);

        if (transparentTimer > 0)
        {
            transparentTimer -= Time.deltaTime;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        uiManager.UpdatePlayerStats(mp, MazeGameStats.Instance.MaxMP, sight + extraSight, MazeGameStats.Instance.MaxSight);
    }

    public float GetTotalSight()
    {
        return sight + extraSight;
    }

    public bool IsTransparent()
    {
        return transparentTimer > 0;
    }
}