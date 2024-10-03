using SL.Lib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SL.Lib
{
    public class CommonEnemy : EnemyController
    {
        [SerializeField] private float defaultSpeed = 0.5f; // •ûŒü•ÏXŠÔŠu
        [SerializeField] private float maxStackTime = 2f;

        private Vector2 randomDirection;
        private float lastDirectionChangeTime;
        private List<Vector2> targetPath = new();
        private Vector2 beforePosition;
        private float stackTimer = 0f;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            randomDirection = Random.insideUnitCircle.normalized;
            lastDirectionChangeTime = 0f;
            stackTimer = 0f;
        }

        protected override void Think()
        {
            if (targetedSoulIds.Count > 0)
            {
                UntargetAll(id => !(SoulControllerManager.HasID(id) && (Vector2.Distance(Position, SoulControllerManager.GetController(id).Position) <= SightRange)));
                if (targetedSoulIds.Count > 0)
                {
                    string targetSoul = targetedSoulIds.OrderByDescending(id => SoulControllerManager.GetController(id).Intensity).First();
                    targetPath = MazeGameScene.Instance.MazeManager.GetPath(Position, SoulControllerManager.GetController(targetSoul).Position);
                }
            }
            else 
            {
                if (targetPath.Count == 0 || stackTimer > maxStackTime)
                {
                    stackTimer = 0f;
                    List<ISoulController> visibleSouls = FindVisibleSouls();
                    if (visibleSouls.Count > 0)
                    {
                        MaxSpeed = defaultSpeed * 2f;
                        // °‚ªŒ©‚¦‚Ä‚¢‚éê‡‚Í’ÇÕ
                        ISoulController targetSoul = visibleSouls.OrderByDescending(s => s.Intensity).First();
                        targetPath = MazeGameScene.Instance.MazeManager.GetPath(Position, targetSoul.Position);
                        TargetSoul(targetSoul);
                    }
                    else
                    {
                        MaxSpeed = defaultSpeed;
                        // °‚ªŒ©‚¦‚Ä‚¢‚È‚¢ê‡‚Íƒ‰ƒ“ƒ_ƒ€‚È•ûŒü‚ÉˆÚ“®
                        targetPath = MazeGameScene.Instance.MazeManager.GetMostLightPath(Position, Mathf.CeilToInt(SightRange));
                    }
                    if (targetPath.Count == 0)
                    {
                        MaxSpeed = defaultSpeed;
                        targetPath.Add(Position + MazeGameScene.Instance.MazeManager.SelectRandomValidDirection(Position) * (SLRandom.NextSingle(0.2f, 1f)*MaxSpeed));
                    }
                }
            }
            if (Vector2.Distance(beforePosition, Position) < MaxSpeed * Time.deltaTime * 0.5f)
            {
                stackTimer = 0f;
                targetPath.Clear();
            }
            else
            {
                if (Vector2.Distance(targetPath[0], Position) < 0.1f)
                {
                    stackTimer = 0f;
                    targetPath.RemoveAt(0);
                }
                else
                {
                    virtualInput.Movement = (targetPath[0] - Position);
                    stackTimer += DeltaThinkTime;
                }
            }
            beforePosition = Position;
        }

        private List<ISoulController> FindVisibleSouls()
        {
            return SoulControllerManager.InstantinatedControllers.Values
                .Where(soul => Vector2.Distance(Position, soul.Position) <= SightRange)
                .ToList();
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            if (targetPath == null) return;
            if (targetPath.Count == 0) return;
            Gizmos.color = targetedSoulIds.Count > 0?Color.red:Color.yellow;
            for (int i = 1; i < targetPath.Count; i++) 
            {
                Gizmos.DrawLine(targetPath[i - 1], targetPath[i]);
            }
        }
    }
}
