using Sl.Lib;
using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input = UnityEngine.Input;
public class PlayerInput : SoulInput
{
    public Vector2 MovementInput => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    public bool IsActionPressed => Input.GetButtonDown("Fire1");
}

public class PlayerController : SoulController<PlayerInput>
{
    public Dictionary<KeyCode, SkillManager> SkillBank { get; private set; }

    protected override CharacterStatus Status => PlayerStatusManager.Instance.RuntimeStatus;

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
    public override void Initialize(Vector2 position, (int rows, int cols) mazeSize)
    {
        SkillBank ??= PlayerStatusManager.Instance.GetSkills();
        base.Initialize(position, mazeSize);
    }
    protected override void UpdateMP()
    {
        MazeGameScene.Instance.UIManager.UpdatePlayerStats(MP, Intensity);
    }
    protected override void UpdateSightRange()
    {
        base.UpdateSightRange();
        MazeGameScene.Instance.UIManager.UpdatePlayerStats(MP, Intensity);
    }

    protected override void OnCollision(Collision2D collision)
    {
        Debug.Log($"Player collision to {collision.transform.name}");
        if(collision.transform.tag == "Goal")
        {
            MazeGameScene.Instance.Victory();
        }
        else
        {
            if (collision.gameObject.TryGetComponentInParent(out SoulNPCController controller))
            {
                Debug.Log($"Player Collision to {controller.gameObject.name}, [{controller.Intensity}/{controller.MaxIntensity}]");
                if (controller.Intensity/controller.MaxIntensity >= 0.9f)
                {
                    PlayerStatus.Instance.PlayerParameter.LP += 1;
                    controller.ForceKill();
                }
            }
            else
            {
                base.OnCollision(collision);
            }
        }
    }

    protected override void InitializeVirtualInput()
    {
        virtualInput = new PlayerInput();
    }
}