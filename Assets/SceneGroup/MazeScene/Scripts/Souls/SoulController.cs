using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SL.Lib
{

    public interface SoulInput : IVirtualInput
    {
    }

    public interface ISoulController : ICharacterController
    {
        
        public float Intensity { get; set; }
        public float SightRange { get; set; }
        public void OnTargeted(IEnemyController enemy);
        public void OnUntargeted(IEnemyController enemy);

    }
    public class SoulControllerManager : ICharacterControllerManagaer<ISoulController>
    {
    }
    public abstract class SoulController<TInput> : Character<TInput>, ISoulController where TInput : SoulInput
    {
        [SerializeField] private LightFlicker1fNoise targetLight;
        [SerializeField] private ParticleSystem wispParticle;

        protected abstract CharacterStatus Status { get; }

        protected List<string> targetFrom = new();

        private float _flux;
        public float Flux
        {
            get { return _flux; }
            set
            {
                if (_flux != value)
                {
                    _flux = value;
                    UpdateFlux();
                }
            }
        }

        private float _intensity;
        public float Intensity
        {
            get { return _intensity; }
            set
            {
                if (_intensity != value)
                {
                    _intensity = value;
                    UpdateSightRange();
                }
            }
        }

        private float _extreIntensity = 0f;
        public float ExtraIntensity
        {
            get
            {
                return _extreIntensity;
            }
            set
            {
                if (_extreIntensity != value)
                {
                    _extreIntensity = value;
                    UpdateSightRange();
                }
            }
        }
        public float MaxFlux => Status.MaxFlux;
        public float MaxIntensity => Status.MaxIntensity;
        public float RestoreIntensityPerSecond => Status.RestoreIntensityPerSecond;
        public float RestoreFluxPerSecond => Status.RestoreFluxPerSecond;
        public float InvincibilityDuration => Status.InvincibilityDuration;
        private float SharedLight
        {
            get
            {
                float total = 0f;
                var nears = SoulControllerManager.GetOtherControllers(id).Select(p => (p, Vector2.Distance(p.Position, Position))).Where(p => p.Item2 < Mathf.Max(SightRange, p.p.SightRange));
                if (nears.Count() == 0) return 0;
                foreach (var souls in nears)
                {
                    if (souls.Item2 < 1f)
                    {
                        total += souls.p.Intensity;
                    }
                    else
                    {
                        total += souls.p.Intensity / souls.Item2;
                    }
                }
                return total / nears.Count();
            }
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (IsAlive)
            {
                RestoreMP();
                RestoreSight();
                if (targetFrom.Count > 0)
                {
                    targetFrom.RemoveAll(id => !EnemyControllerManager.HasID(id));
                    if (targetFrom.Count > 0) 
                    {
                        OnStayTargeted();
                    }
                }
            }
        }
        protected virtual void RestoreMP()
        {
            Flux = Mathf.Min(Flux + RestoreFluxPerSecond * Time.deltaTime, MaxFlux);
        }
        protected virtual void RestoreSight()
        {
            if (Intensity >= MaxIntensity)
            {
                Intensity = MaxIntensity;
                return;
            }
            Intensity = Mathf.Min(Intensity + (RestoreIntensityPerSecond + SharedLight) * Time.deltaTime, MaxIntensity);
        }

        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            Reinitialize();
        }
        public void Reinitialize()
        {
            Flux = MaxFlux;
            Intensity = MaxIntensity;
            targetFrom = new();
            if (id == null)
            {
                id = SoulControllerManager.Add(this);
            }
            UpdateSightRange();
        }

        protected virtual void UpdateSightRange()
        {
            SightRange = Mathf.Sqrt(Intensity + ExtraIntensity);
            MazeGameScene.Instance.MazeManager.SetIntensity(Position, SightRange);
            targetLight.BaseRange = SightRange;
            targetLight.BaseIntensity = Intensity + ExtraIntensity;
            wispParticle.transform.localScale = Vector3.one * Intensity / MaxIntensity;
        }
        protected virtual void UpdateFlux()
        {
        }

        protected override void OnAfterMove()
        {
            base.OnAfterMove();
            UpdateSightRange();
        }

        public void TakeDamage(float damage)
        {
            if (IsAlive)
            {
                Debug.Log($"{character.name} took {damage} damage");
                Intensity -= damage;
                Intensity = Mathf.Max(Intensity, 0);
                if (Intensity == 0)
                {
                    CurrentState -= CharacterState.Alive;
                }
                else
                {
                    UpdateSightRange();
                    StartInvincible();
                }
            }
        }

        public void StartInvincible()
        {
            if(CurrentState.HasFlag(CharacterState.Alive) && !CurrentState.HasFlag(CharacterState.Invincible))
            {
                StartCoroutine(InvincibilityCoroutine());
            }
        }
        public IEnumerator InvincibilityCoroutine()
        {
            CurrentState |= CharacterState.Invincible;
            // TODO: Implement invincibility visual effect
            yield return new WaitForGameSeconds(InvincibilityDuration); // Adjust invincibility duration as needed
            CurrentState -= CharacterState.Invincible;
            ForceCollisionCheck();
            // TODO: Remove invincibility visual effect
        }
        private void ForceCollisionCheck()
        {
            if (!IsAlive) return;
            Collider2D[] overlappingColliders = new Collider2D[10]; // 適切なサイズに調整
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            filter.SetLayerMask(LayerMask.GetMask("Enemy")); // 敵のレイヤーを指定

            int numColliders = Physics2D.OverlapCollider(characterCollider, filter, overlappingColliders);

            for (int i = 0; i < numColliders; i++)
            {
                if (overlappingColliders[i].TryGetComponent(out EnemyController enemyController))
                {
                    // 衝突処理を手動で呼び出す
                    OnEnemyCollision(enemyController);
                }
            }
        }

        private void OnEnemyCollision(EnemyController enemyController)
        {
            if (IsAlive && !CurrentState.HasFlag(CharacterState.Invincible))
            {
                TakeDamage(enemyController.EnemyDamage(this));
            }
        }

        protected override void OnStateEnter(CharacterState newState)
        {
            Debug.Log($"On State Enter[{character.gameObject.name}] {newState}");
            if (newState.HasFlag(CharacterState.Invincible))
            {
                characterCollider.AddExcludeLayer("Enemy");
            }
        }
        protected override void OnStateExit(CharacterState oldState)
        {
            if (!CurrentState.HasFlag(CharacterState.Invincible))
            {
                characterCollider.RemoveExcludeLayer("Enemy");
            }
        }

        protected override void OnCollision(Collision2D collision)
        {
            //Debug.Log($"{character.name} collision to {collision.transform.name}");
            if (IsAlive)
            {
                if (!CurrentState.HasFlag(CharacterState.Invincible) && collision.transform.TryGetComponent(out EnemyController enemyController))
                {
                    TakeDamage(enemyController.EnemyDamage(this));
                    enemyController.OnHitSoul(this);
                }
            }
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(id))
            {
                SoulControllerManager.Remove(id);
            }
            id = null;
        }
        protected virtual void OnStayTargeted()
        {
        }
        public void OnTargeted(IEnemyController enemy)
        {
            Debug.Log($"{Name} targeted by {enemy.Name}");
            if (!targetFrom.Contains(enemy.ID)) {
                targetFrom.Add(enemy.ID);
                if (targetFrom.Count == 1) OnStartTargeted();
            }
        }

        protected virtual void OnStartTargeted()
        {

        }

        public void OnUntargeted(IEnemyController enemy)
        {
            Debug.Log($"{Name} untargeted by {enemy.Name}");
            if (targetFrom.Contains(enemy.ID))
            {
                targetFrom.Remove(enemy.ID);
                if (targetFrom.Count == 0) OnEndTargeted();
            }
        }
        protected virtual void OnEndTargeted()
        {

        }
    }

}