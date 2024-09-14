using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeleportEffect", menuName = "SkillSystem/Effects/TeleportEffect")]
public class TeleportEffectUnit : EffectUnit
{
    public float MoveIntensityCost = 0.01f;

    public override IEnumerator ApplyEffect(PlayerController player, int level, KeyCode triggerKey)
    {
        MazeGameScene.Instance.SetFreezeInput(true);
        bool confirmed = false;
        Vector2 direction = Vector2.zero;
        while (Input.GetKeyDown(triggerKey))
        {
            yield return null;
        }
        Debug.Log($"Select Direction");
        while (!confirmed)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                direction = Vector2.up;
                confirmed = true;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                direction = Vector2.left;
                confirmed = true;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                direction = Vector2.down;
                confirmed = true;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                direction = Vector2.right;
                confirmed = true;
            }
            else if (Input.GetKeyDown(triggerKey))
            {
                // �A������������邽�߂ɗ����܂őҋ@
                while (Input.GetKey(triggerKey))
                {
                    yield return null;
                }
                Success = false;
                MazeGameScene.Instance.SetFreezeInput(false);
                yield break;
            }
            yield return null;
        }

        // ���[�v����
        player.CurrentState |= CharacterState.Invincible;

        Vector2 startPosition = player.Position;
        Vector2 currentPosition = startPosition;
        float quant = 0.01f;
        int maxRange = Mathf.CeilToInt(player.Intensity / (MoveIntensityCost * quant)) - 1;

        for (int i = 0;i < maxRange;i++)
        {
            Vector2 nextPosition = currentPosition + direction * quant;

            // �ǂƂ̏Փ˃`�F�b�N
            if (player.CollisionWall(nextPosition))
            {
                break;
            }

            currentPosition = nextPosition;
        }

        // �v���C���[�̈ʒu���X�V
        player.Warp(currentPosition);
        player.Intensity -= Vector2.Distance(currentPosition,startPosition)* MoveIntensityCost;

        // �G�t�F�N�g�̕\��
        yield return ShowWarpEffect(startPosition, currentPosition);

        MazeGameScene.Instance.SetFreezeInput(false);
        player.CurrentState -= CharacterState.Invincible;
        yield return player.InvincibilityCoroutine();
        Success = true;
    }

    private IEnumerator ShowWarpEffect(Vector2 start, Vector2 end)
    {
        // �����Ƀ��[�v�G�t�F�N�g�̎�����ǉ�
        // ��: �p�[�e�B�N���V�X�e���̍Đ��A���C�������_���[�̎g�p�Ȃ�
        Debug.Log($"Warped from {start} to {end}");
        yield return new WaitForSeconds(0.5f); // �G�t�F�N�g�\������
    }
}
