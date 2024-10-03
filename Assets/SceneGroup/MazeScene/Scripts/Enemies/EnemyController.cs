using SL.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SL.Lib
{
    public interface IEnemyController : ICharacterController
    {
        public void Kill();
    }
    public class EnemyControllerManager : ICharacterControllerManagaer<IEnemyController>
    {
        
    }

    public class EnemyInput : IVirtualInput
    {
        public Vector2 Movement { get; set; }
        Vector2 IVirtualInput.MovementInput => Vector2.ClampMagnitude(Movement, 1f);

        bool IVirtualInput.IsActionPressed => false;
    }

    public abstract class EnemyController : Character<EnemyInput>, IEnemyController
    {

        protected List<string> targetedSoulIds = new();

        protected void TargetSoul(ISoulController soul)
        {
            soul.OnTargeted(this);
            targetedSoulIds.Add(soul.ID);
        }
        protected void TargetSoul(string id)
        {
            if (SoulControllerManager.HasID(id))
            {
                SoulControllerManager.GetController(id).OnTargeted(this);
                targetedSoulIds.Add(id);
            }
        }
        protected void UntargetSoul(string id)
        {
            if (SoulControllerManager.HasID(id))
            {
                var soul = SoulControllerManager.GetController(id);
                soul.OnUntargeted(this);
            }
            if (targetedSoulIds.Contains(id))
            {
                targetedSoulIds.Remove(id);
            }
        }
        protected void UntargetAll()
        {
            foreach (var id in targetedSoulIds)
            {
                if (SoulControllerManager.HasID(id))
                {
                    var soul = SoulControllerManager.GetController(id);
                    soul.OnUntargeted(this);
                }
            }
            targetedSoulIds.Clear();
        }
        protected void UntargetAll(Func<string, bool> predicate)
        {
            foreach (var id in targetedSoulIds)
            {
                if (predicate(id))
                {
                    if (SoulControllerManager.HasID(id))
                    {
                        var soul = SoulControllerManager.GetController(id);
                        soul.OnUntargeted(this);
                    }
                }
            }
            targetedSoulIds.RemoveAll(id => predicate(id));
        }

        protected List<ISoulController> GetTargetSouls() => targetedSoulIds.Where(id => SoulControllerManager.InstantinatedControllers.ContainsKey(id)).Select(id => SoulControllerManager.InstantinatedControllers[id]).ToList();

        public float ThinkingInterval = 0.5f;

        protected float DeltaThinkTime => ThinkingInterval + Time.deltaTime;

        private float lastThinkTime;

        public LayerMask ObstacleLayer;

        public virtual float EnemyDamage<TInput>(SoulController<TInput> soulController) where TInput : SoulInput
        {
            return soulController.MaxIntensity / 2f;
        }

        public override void UpdateState()
        {
            base.UpdateState();
        }

        protected override void InitializeVirtualInput()
        {
            virtualInput = new EnemyInput();
        }

        protected override void HandleMovement()
        {
            if (MazeGameScene.Instance.GameTime - lastThinkTime >= ThinkingInterval)
            {
                lastThinkTime = MazeGameScene.Instance.GameTime;
                Think();
            }
            base.HandleMovement();
        }
        protected virtual void MoveTowards(Vector2 target)
        {
            Vector2 direction = (target - Position).normalized;
            if (!IsObstacleInWay(target))
            {
                virtualInput.Movement = direction / MaxSpeed;
            }
            else
            {
                Vector2 newDirection = FindAlternativeDirection(direction);
                virtualInput.Movement = 2 * newDirection.normalized / MaxSpeed;
            }
        }

        protected bool IsObstacleInWay(Vector2 target)
        {
            Vector2 direction = target - Position;
            RaycastHit2D hit = Physics2D.Raycast(Position, direction, 1f, ObstacleLayer);
            return hit.collider != null;
        }

        protected virtual Vector2 FindAlternativeDirection(Vector2 originalDirection)
        {
            float[] rotations = new float[20];
            for (int i = 0; i < rotations.Length; i++)
            {
                rotations[i] = 360f * ((float)i / rotations.Length);
            }
            foreach (float rotation in rotations)
            {
                Vector2 newDirection = Rotate(originalDirection, rotation);
                if (!IsObstacleInWay(Position + newDirection))
                {
                    return newDirection;
                }
            }
            return Vector2.zero; // ˆÚ“®‰Â”\‚È•ûŒü‚ªŒ©‚Â‚©‚ç‚È‚¢ê‡
        }

        protected Vector2 Rotate(Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
            return new Vector2(
                cos * v.x - sin * v.y,
                sin * v.x + cos * v.y
            );
        }
        protected ISoulController FindNearestSoul()
        {
            var allSouls = SoulControllerManager.InstantinatedControllers.Values;
            return allSouls
                .OrderBy(soul => Vector2.Distance(Position, soul.Position) <= soul.SightRange)
                .FirstOrDefault();
        }
        protected abstract void Think();

        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            lastThinkTime = 0f;
            if (id == null)
            {
                id = EnemyControllerManager.Add(this);
            }
            if (targetedSoulIds != null && targetedSoulIds.Count > 0) 
            {
                UntargetAll();
            }
            characterCollider.AddExcludeLayer("Enemy");
            OnInitialized();
        }

        protected virtual void OnInitialized() { }
        public virtual void Kill()
        {
            Destroy(gameObject);
        }
        private void OnDestroy()
        {
            UntargetAll();
            if (!string.IsNullOrEmpty(id))
            {
                EnemyControllerManager.Remove(id);
            }
            id = null;
        }

        public void SetIgnoreWall(bool ignore)
        {
            if (ignore)
            {
                characterCollider.AddExcludeLayer("Wall");
            }
            else
            {
                characterCollider.RemoveExcludeLayer("Wall");
            }
        }
        public void SetIgnoreSoul(bool ignore)
        {
            if (ignore)
            {
                characterCollider.AddExcludeLayer("Soul");
            }
            else
            {
                characterCollider.RemoveExcludeLayer("Soul");
            }
        }
        public virtual void OnHitSoul(ISoulController soul)
        {

        }
        protected virtual void OnDrawGizmosSelected()
        {
            if (targetedSoulIds.Count > 0)
            {
                Gizmos.color = Color.red;
                foreach(var soul in GetTargetSouls())
                {
                    Gizmos.DrawLine(Position, soul.Position);
                }
            }
        }
    }
}