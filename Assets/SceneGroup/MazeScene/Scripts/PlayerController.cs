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
    public override void Initialize(Vector2 position)
    {
        SkillBank ??= PlayerStatusManager.Instance.GetSkills();
        base.Initialize(position);
    }
    protected override void UpdateFlux()
    {
        MazeGameScene.Instance.UIManager.UpdatePlayerStats(Flux, Intensity);
    }
    protected override void UpdateSightRange()
    {
        base.UpdateSightRange();
        MazeGameScene.Instance.UIManager.UpdatePlayerStats(Flux, Intensity);
    }

    protected override void OnCollision(Collision2D collision)
    {
        //Debug.Log($"Player collision to {collision.transform.name}");
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
                    MazeGameScene.Instance.MazeGameMemory.CollectedLP++;
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

    protected override void OnStartTargeted()
    {
        base.OnStartTargeted();
        MazeGameScene.Instance.UIManager.StartTargeted();
    }
    protected override void OnStayTargeted()
    {
        base.OnStayTargeted();
    }
    protected override void OnEndTargeted()
    {
        base.OnEndTargeted();
        MazeGameScene.Instance.UIManager.EndTargeted();
    }
}