using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sl.Lib
{

    public interface SoulInput : IVirtualInput
    {
    }
    public abstract class SoulController<TInput> : Character<TInput> where TInput: SoulInput
    {
        [SerializeField] private LightFlicker1fNoise targetLight;
        [SerializeField] private ParticleSystem wispParticle;

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

        public bool IsDead => Intensity <= 0;

        private bool _isTransparent;
        private Coroutine _transparencyCoroutine;

        public abstract float MaxMP { get;}
        public abstract float MaxIntensity { get;}
        public abstract float RestoreIntensityPerSecond { get; }
        public abstract float RestoreMPPerSecond { get; }

        public override void UpdateState()
        {
            RestoreMP();
            RestoreSight();
        }

        protected virtual void RestoreMP()
        {
            MP = Mathf.Min(MP + RestoreMPPerSecond * Time.deltaTime, MaxMP);
        }


        protected virtual void RestoreSight()
        {
            Intensity = Mathf.Min(Intensity + RestoreIntensityPerSecond * Time.deltaTime, MaxIntensity);
        }

        public override void Initialize(Vector2 position, (int rows, int cols) mazeSize)
        {
            base.Initialize(position, mazeSize);
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
            if (!_isTransparent && CurrentState == CharacterState.Alive)
            {
                // TODO: Implement damage logic
                Debug.Log($"{character.name} took {damage} damage");
                Intensity -= damage;
                Intensity = Mathf.Max(Intensity, 0);
                UpdateSightRange();
                StartCoroutine(InvincibilityCoroutine());

            }
        }

        public IEnumerator InvincibilityCoroutine()
        {
            CurrentState = CharacterState.Invincible;
            // TODO: Implement invincibility visual effect
            yield return new WaitForGameSeconds(1f); // Adjust invincibility duration as needed
            CurrentState = CharacterState.Alive;
            // TODO: Remove invincibility visual effect
        }

        protected override void OnStateEnter(CharacterState newState)
        {
            switch (newState)
            {
                case CharacterState.Invincible:
                    characterCollider.AddExcludeLayer("Character");
                    break;
            }
        }
        protected override void OnStateExit(CharacterState newState)
        {
            switch (newState)
            {
                case CharacterState.Invincible:
                    characterCollider.RemoveExcludeLayer("Character");
                    break;
            }
        }

        protected override void OnCollision(Collision2D collision)
        {
            Debug.Log($"{character.name} collision to {collision.transform.name}");
            if (CurrentState == CharacterState.Alive)
            {
                if (collision.transform.TryGetComponent(out EnemyController enemyController))
                {
                    TakeDamage(enemyController.EnemyDamage(this));
                }
            }
        }
    }

}