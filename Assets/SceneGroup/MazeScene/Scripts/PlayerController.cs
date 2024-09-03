using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Normal,
        Teleporting,
        UsingItem,
        Damaged,
        Dead
    }

    [SerializeField] private float moveSpeed = 5f;

    private Vector2Int position;
    private float mp;
    private float sight;
    private float extraSight;
    private float transparentTimer;
    private List<GameItem> items = new List<GameItem>();

    private PlayerState currentState = PlayerState.Normal;
    private MazeManager mazeManager;
    private GameUIManager uiManager;

    public bool IsDead => currentState == PlayerState.Dead;
    public Vector2Int Position => position;

    public void Initialize(Vector2Int startPosition)
    {
        position = startPosition;
        mazeManager = MazeGameScene.Instance.MazeManager;
        uiManager = MazeGameScene.Instance.UIManager;

        mp = MazeGameStats.Instance.MaxMP;
        sight = MazeGameStats.Instance.MaxSight;
        extraSight = 0;
        transparentTimer = 0;

        transform.position = mazeManager.GetWorldPosition(position);
        InitializeItems();
        UpdateUI();
        SetState(PlayerState.Normal);
    }

    private void InitializeItems()
    {
        // TODO: アイテムの初期化ロジックを実装
        // - プレイヤーが開始時に持つアイテムをリストに追加
        // - 各アイテムの初期状態を設定
    }

    private void Update()
    {
        if (MazeGameScene.Instance.CurrentState == MazeGameScene.GameState.Playing)
        {
            UpdateCurrentState();
            UpdateStats();
        }
    }

    private void UpdateCurrentState()
    {
        switch (currentState)
        {
            case PlayerState.Normal:
                HandleNormalStateInput();
                break;
            case PlayerState.Teleporting:
                HandleTeleportingStateInput();
                break;
            case PlayerState.UsingItem:
                HandleUsingItemStateInput();
                break;
            case PlayerState.Damaged:
                HandleDamagedState();
                break;
            case PlayerState.Dead:
                // 死亡状態の処理は MazeGameScene で行う
                break;
        }
    }

    private void HandleNormalStateInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) TryMove(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) TryMove(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) TryMove(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) TryMove(Vector2Int.right);

        if (Input.GetKeyDown(KeyCode.Space)) UseHint();
        if (Input.GetKeyDown(KeyCode.Q)) StartTeleport();
        if (Input.GetKey(KeyCode.E)) UseMpToBrightness();

        for (int i = 0; i < items.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseItem(i);
            }
        }
    }

    private void HandleTeleportingStateInput()
    {
        // TODO: テレポート中の入力処理を実装
        // - 方向キーでテレポート先を選択
        // - 決定キーでテレポートを実行
        // - キャンセルキーで通常状態に戻る
    }

    private void HandleUsingItemStateInput()
    {
        // TODO: アイテム使用中の入力処理を実装
        // - アイテムの効果が継続中は他の操作を制限
        // - 必要に応じてアイテムの効果をキャンセルする機能を追加
    }

    private void HandleDamagedState()
    {
        // TODO: ダメージ状態の処理を実装
        // - 一定時間操作を制限
        // - ダメージアニメーションの再生
        // - 一定時間経過後、通常状態に戻る
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = position + direction;
        if (mazeManager.IsValidMove(newPosition))
        {
            position = newPosition;
            transform.position = mazeManager.GetWorldPosition(position);
            CheckForEnemies();

            // TODO: 移動時のサウンド再生
            // TODO: 移動アニメーションの再生
        }
    }

    private void UseHint()
    {
        if (mp >= MazeGameStats.Instance.HintMPCost)
        {
            mp -= MazeGameStats.Instance.HintMPCost;
            uiManager.ShowHint();
            // TODO: ヒント使用時のサウンド再生
        }
    }

    private void StartTeleport()
    {
        if (mp >= MazeGameStats.Instance.TeleportMPCost && sight >= MazeGameStats.Instance.MinSightForTeleport && extraSight == 0)
        {
            SetState(PlayerState.Teleporting);
            uiManager.UpdateTeleportMode(true);
            // TODO: テレポートモード開始時のエフェクト再生
        }
    }

    private void UseMpToBrightness()
    {
        float mpCost = MazeGameStats.Instance.MPForBrightnessCostPerSecond * Time.deltaTime;
        if (mp >= mpCost && currentState == PlayerState.Normal)
        {
            mp -= mpCost;
            extraSight += MazeGameStats.Instance.MPForBrightnessValuePerSecond * Time.deltaTime;
            // TODO: 明るさ増加時のエフェクト再生
        }
        else if (extraSight > 0)
        {
            extraSight -= MazeGameStats.Instance.MPForBrightnessDecayPerSecond * Time.deltaTime;
            extraSight = Mathf.Max(0, extraSight);
        }
    }

    private void UseItem(int index)
    {
        if (index >= 0 && index < items.Count && currentState == PlayerState.Normal)
        {
            if (items[index].Use())
            {
                SetState(PlayerState.UsingItem);
                // TODO: アイテム使用時のエフェクトとサウンドを再生
            }
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
            SetState(PlayerState.Dead);
        }
        else
        {
            SetState(PlayerState.Damaged);
        }
        UpdateUI();
        // TODO: ダメージ時のエフェクトとサウンドを再生
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
        // TODO: プレイヤーの状態に応じてUIを更新（例：アイテム使用中のクールダウン表示）
    }

    public float GetTotalSight()
    {
        return sight + extraSight;
    }

    public bool IsTransparent()
    {
        return transparentTimer > 0;
    }

    private void SetState(PlayerState newState)
    {
        if (currentState != newState)
        {
            ExitCurrentState();
            currentState = newState;
            EnterNewState();
        }
    }

    private void ExitCurrentState()
    {
        // TODO: 現在の状態を終了する際の処理を実装
        // 例: アニメーションの停止、エフェクトのクリーンアップなど
    }

    private void EnterNewState()
    {
        // TODO: 新しい状態に入る際の処理を実装
        // 例: 状態に応じたアニメーションの開始、UIの更新など
    }

    // TODO: セーブデータの保存と読み込み機能を実装
    // TODO: プレイヤーのインベントリ管理システムを拡張
    // TODO: プレイヤーのスキルツリーを実装
    // TODO: プレイヤーの状態異常（毒、麻痺など）の管理システムを実装
}