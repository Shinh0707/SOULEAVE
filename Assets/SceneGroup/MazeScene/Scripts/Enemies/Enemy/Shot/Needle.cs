using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace SL.Lib
{
    public class Needle : EnemyController
    {
        public enum TargetState
        {
            None,
            Targeting,
            ShootQueued,
            Shooting
        }
        private TargetState targetState;
        private Vector2 targetDirection;
        [SerializeField] private float shootInterval = 0.8f;
        [SerializeField] private float shootSpeed = 5f;
        private float shootIntervalTimer = 0f;
        private float shootingRange = 10f;
        private float targetingTimer = 0f;
        private Vector2 startPosition;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            targetState = TargetState.None;
            shootIntervalTimer = 0f;
            startPosition = Position;
            SetIgnoreWall(true);
            SetIgnoreSoul(true);
        }
        public void SetTarget(string id)
        {
            if (targetState == TargetState.None || targetState == TargetState.Targeting)
            {
                if (targetedSoulIds.Count > 0)
                {
                    UntargetAll();
                }
                TargetSoul(id);
            }
        }

        public void StartTargeting(float duration)
        {
            if (targetState == TargetState.None)
            {
                targetingTimer = duration;
                targetState = TargetState.Targeting;
            }
        }

        protected override void Think()
        {
            virtualInput.Movement = Vector2.zero;
            switch (targetState)
            {
                case TargetState.None:
                    MaxSpeed = 0f;
                    break;
                case TargetState.Targeting:
                    MaxSpeed = 0f;
                    if (targetingTimer > 0f)
                    {
                        targetingTimer -= DeltaThinkTime;
                        if (targetedSoulIds.Count > 0)
                        {
                            UntargetAll(id => !(SoulControllerManager.HasID(id)));
                            if (targetedSoulIds.Count > 0)
                            {
                                string targetSoul = targetedSoulIds[0];
                                targetDirection = (SoulControllerManager.GetController(targetSoul).Position- Position).normalized;
                            }
                        }
                    }
                    else
                    {
                        targetState = TargetState.ShootQueued;
                    }
                    break;
                case TargetState.ShootQueued:
                    MaxSpeed = 0;
                    shootIntervalTimer += Time.deltaTime;
                    if (shootIntervalTimer > shootInterval)
                    {
                        startPosition = Position;
                        SetIgnoreSoul(false);
                        targetState = TargetState.Shooting;
                    }
                    break;
                case TargetState.Shooting:
                    MaxSpeed = shootSpeed;
                    virtualInput.Movement = targetDirection;
                    if (!(MazeGameScene.Instance.MazeManager.IsInMaze(Position) && Vector2.Distance(startPosition, Position) <= shootingRange))
                    {
                        Kill();
                    }
                    break;
            }
        }

        public override float EnemyDamage<TInput>(SoulController<TInput> soulController)
        {
            return base.EnemyDamage(soulController) * 1.2f;
        }

        public override void OnHitSoul(ISoulController soul)
        {
            base.OnHitSoul(soul);
            Kill();
        }
    }

}