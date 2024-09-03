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
        // TODO: �A�C�e���̏��������W�b�N������
        // - �v���C���[���J�n���Ɏ��A�C�e�������X�g�ɒǉ�
        // - �e�A�C�e���̏�����Ԃ�ݒ�
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
                // ���S��Ԃ̏����� MazeGameScene �ōs��
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
        // TODO: �e���|�[�g���̓��͏���������
        // - �����L�[�Ńe���|�[�g���I��
        // - ����L�[�Ńe���|�[�g�����s
        // - �L�����Z���L�[�Œʏ��Ԃɖ߂�
    }

    private void HandleUsingItemStateInput()
    {
        // TODO: �A�C�e���g�p���̓��͏���������
        // - �A�C�e���̌��ʂ��p�����͑��̑���𐧌�
        // - �K�v�ɉ����ăA�C�e���̌��ʂ��L�����Z������@�\��ǉ�
    }

    private void HandleDamagedState()
    {
        // TODO: �_���[�W��Ԃ̏���������
        // - ��莞�ԑ���𐧌�
        // - �_���[�W�A�j���[�V�����̍Đ�
        // - ��莞�Ԍo�ߌ�A�ʏ��Ԃɖ߂�
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = position + direction;
        if (mazeManager.IsValidMove(newPosition))
        {
            position = newPosition;
            transform.position = mazeManager.GetWorldPosition(position);
            CheckForEnemies();

            // TODO: �ړ����̃T�E���h�Đ�
            // TODO: �ړ��A�j���[�V�����̍Đ�
        }
    }

    private void UseHint()
    {
        if (mp >= MazeGameStats.Instance.HintMPCost)
        {
            mp -= MazeGameStats.Instance.HintMPCost;
            uiManager.ShowHint();
            // TODO: �q���g�g�p���̃T�E���h�Đ�
        }
    }

    private void StartTeleport()
    {
        if (mp >= MazeGameStats.Instance.TeleportMPCost && sight >= MazeGameStats.Instance.MinSightForTeleport && extraSight == 0)
        {
            SetState(PlayerState.Teleporting);
            uiManager.UpdateTeleportMode(true);
            // TODO: �e���|�[�g���[�h�J�n���̃G�t�F�N�g�Đ�
        }
    }

    private void UseMpToBrightness()
    {
        float mpCost = MazeGameStats.Instance.MPForBrightnessCostPerSecond * Time.deltaTime;
        if (mp >= mpCost && currentState == PlayerState.Normal)
        {
            mp -= mpCost;
            extraSight += MazeGameStats.Instance.MPForBrightnessValuePerSecond * Time.deltaTime;
            // TODO: ���邳�������̃G�t�F�N�g�Đ�
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
                // TODO: �A�C�e���g�p���̃G�t�F�N�g�ƃT�E���h���Đ�
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
        // TODO: �_���[�W���̃G�t�F�N�g�ƃT�E���h���Đ�
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
        // TODO: �v���C���[�̏�Ԃɉ�����UI���X�V�i��F�A�C�e���g�p���̃N�[���_�E���\���j
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
        // TODO: ���݂̏�Ԃ��I������ۂ̏���������
        // ��: �A�j���[�V�����̒�~�A�G�t�F�N�g�̃N���[���A�b�v�Ȃ�
    }

    private void EnterNewState()
    {
        // TODO: �V������Ԃɓ���ۂ̏���������
        // ��: ��Ԃɉ������A�j���[�V�����̊J�n�AUI�̍X�V�Ȃ�
    }

    // TODO: �Z�[�u�f�[�^�̕ۑ��Ɠǂݍ��݋@�\������
    // TODO: �v���C���[�̃C���x���g���Ǘ��V�X�e�����g��
    // TODO: �v���C���[�̃X�L���c���[������
    // TODO: �v���C���[�̏�Ԉُ�i�ŁA��ჂȂǁj�̊Ǘ��V�X�e��������
}