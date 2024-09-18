using SL.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sl.Lib
{

    public interface SoulInput : IVirtualInput
    {
    }

    public interface ISoulController
    {
        public Vector2 Position { get; }
        public float Intensity { get; set; }
        public float SightRange { get; set; }

    }
    public class SoulControllerManager
    {
        public static Dictionary<string, ISoulController> InstantinatedControllers = new();

        public static string Add(ISoulController soulController)
        {
            string id = Guid.NewGuid().ToString();
            InstantinatedControllers.Add(id, soulController);
            return id;
        }
        public static void Remove(string id)
        {
            if (InstantinatedControllers.ContainsKey(id))
            {
                InstantinatedControllers.Remove(id);
            }
        }

        public static List<ISoulController> GetOtherControllers(string id) => InstantinatedControllers.Where(p => p.Key != id).Select(p => p.Value).ToList();
    }
    public abstract class SoulController<TInput> : Character<TInput>, ISoulController where TInput : SoulInput
    {
        [SerializeField] private LightFlicker1fNoise targetLight;
        [SerializeField] private ParticleSystem wispParticle;

        protected abstract CharacterStatus Status { get; }

        private string id = null;

        private float _mp;
        public float MP
        {
            get { return _mp; }
            set
            {
                if (_mp != value)
                {
                    _mp = value;
                    UpdateMP();
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

        private bool _isTransparent;
        private Coroutine _transparencyCoroutine;

        public float MaxMP => Status.MaxMP;
        public float MaxIntensity => Status.MaxIntensity;
        public float RestoreIntensityPerSecond => Status.RestoreIntensityPerSecond;
        public float RestoreMPPerSecond => Status.RestoreMPPerSecond;
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
            if (IsAlive)
            {
                RestoreMP();
                RestoreSight();
            }
        }
        protected virtual void RestoreMP()
        {
            MP = Mathf.Min(MP + RestoreMPPerSecond * Time.deltaTime, MaxMP);
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

        public override void Initialize(Vector2 position, (int rows, int cols) mazeSize)
        {
            base.Initialize(position, mazeSize);
            MP = MaxMP;
            Intensity = MaxIntensity;
            if (id == null)
            {
                id = SoulControllerManager.Add(this);
            }
            UpdateSightRange();
        }
        public void Reinitialize()
        {
            MP = MaxMP;
            Intensity = MaxIntensity;
            UpdateSightRange();
        }

        protected virtual void UpdateSightRange()
        {
            SightRange = Intensity + ExtraIntensity;
            targetLight.BaseRange = SightRange;
            targetLight.BaseIntensity = Intensity + ExtraIntensity;
            wispParticle.transform.localScale = Vector3.one * Intensity / MaxIntensity;
        }
        protected virtual void UpdateMP()
        {
        }

        protected override void OnAfterMove()
        {
            base.OnAfterMove();
            UpdateSightRange();
        }

        public void TakeDamage(float damage)
        {
            if (!_isTransparent && IsAlive)
            {
                // TODO: Implement damage logic
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
                    StartCoroutine(InvincibilityCoroutine());
                }
            }
        }

        public IEnumerator InvincibilityCoroutine()
        {
            CurrentState |= CharacterState.Invincible;
            // TODO: Implement invincibility visual effect
            yield return new WaitForGameSeconds(InvincibilityDuration); // Adjust invincibility duration as needed
            CurrentState -= CharacterState.Invincible;
            // TODO: Remove invincibility visual effect
        }

        protected override void OnStateEnter(CharacterState newState)
        {
            if (newState.HasFlag(CharacterState.Invincible))
            {
                characterCollider.AddExcludeLayer("Character");
            }
        }
        protected override void OnStateExit(CharacterState newState)
        {
            if (newState.HasFlag(CharacterState.Invincible))
            {
                characterCollider.RemoveExcludeLayer("Character");
            }
        }

        protected override void OnCollision(Collision2D collision)
        {
            Debug.Log($"{character.name} collision to {collision.transform.name}");
            if (IsAlive)
            {
                if (!CurrentState.HasFlag(CharacterState.Invincible) && collision.transform.TryGetComponent(out EnemyController enemyController))
                {
                    TakeDamage(enemyController.EnemyDamage(this));
                }
            }
        }

        private void OnDestroy()
        {
            SoulControllerManager.Remove(id);
            id = null;
        }
    }

}