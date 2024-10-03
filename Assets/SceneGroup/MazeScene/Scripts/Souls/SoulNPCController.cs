using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SL.Lib
{

    public class SoulNPCInput : SoulInput
    {
        public Vector2 Movement;

        Vector2 IVirtualInput.MovementInput => Movement.normalized;

        bool IVirtualInput.IsActionPressed { get => false; }
    }
    public class SoulNPCController : SoulController<SoulNPCInput>
    {
        private CharacterStatus _status;
        protected override CharacterStatus Status => _status;

        private bool eatenByPlayer = false;
        public bool EatenByPlayer => eatenByPlayer;

        [SerializeField] private float escapeDistance = 5f; // 敵から逃げ始める距離
        [SerializeField] private float groupingDistance = 3f; // 他の魂と群れを形成する距離
        [SerializeField] private float wanderStrength = 0.3f; // ランダムな動きの強さ

        public void InitializeStatus(CharacterStatus status)
        {
            _status = status;
            eatenByPlayer = false;
            escapeDistance = ((float)SLRandom.Random.NextDouble() + 0.5f)* escapeDistance;
            groupingDistance = ((float)SLRandom.Random.NextDouble() + 0.5f) * groupingDistance;
            wanderStrength = (float)SLRandom.Random.NextDouble();
        }

        protected override void HandleMovement()
        {
            Think();
            base.HandleMovement();
        }

        protected override void InitializeVirtualInput()
        {
            virtualInput = new SoulNPCInput();
        }

        private void Think()
        {
            Vector2 escapeDirection = CalculateEscapeDirection();
            Vector2 groupingDirection = CalculateGroupingDirection();
            Vector2 wanderDirection = Random.insideUnitCircle;

            // 合計移動方向を計算
            Vector2 totalDirection = escapeDirection + groupingDirection + wanderDirection * wanderStrength;

            // 前フレームの移動方向との補間
            float smoothRate = 0.6f;
            virtualInput.Movement = Vector2.Lerp(virtualInput.Movement, totalDirection.normalized, 1 - smoothRate);
        }

        private Vector2 CalculateEscapeDirection()
        {
            Vector2 escapeDirection = Vector2.zero;
            var enemies = EnemyControllerManager.InstantinatedControllers.Values.Where(enemy => Vector2.Distance(Position,enemy.Position) <= SightRange);

            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(Position, enemy.Position);
                if (distance < escapeDistance)
                {
                    Vector2 awayFromEnemy = (Position - enemy.Position).normalized;
                    float escapeStrength = 1 - (distance / escapeDistance); // 近いほど強く逃げる
                    escapeDirection += awayFromEnemy * escapeStrength;
                }
            }

            return escapeDirection.normalized;
        }

        private Vector2 CalculateGroupingDirection()
        {
            Vector2 groupingDirection = Vector2.zero;
            List<ISoulController> nearbySouls = SoulControllerManager.GetOtherControllers(id)
                .Select(s => (s, Vector2.Distance(Position, s.Position))).Where(s => s.Item2 < groupingDistance && s.Item2 < (s.s.SightRange + SightRange))
                .Select(s => s.s).ToList();

            if (nearbySouls.Count > 0)
            {
                Vector2 averagePosition = nearbySouls.Aggregate(Vector2.zero, (sum, soul) => sum + soul.Position) / nearbySouls.Count;
                groupingDirection = (averagePosition - Position).normalized;
            }

            return groupingDirection;
        }

        public float ForceKill()
        {
            float remain = Intensity;
            Intensity = 0f;
            CurrentState -= CharacterState.Alive;
            eatenByPlayer = true;
            return remain;
        }
    }
}