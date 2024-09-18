using Sl.Lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL.Lib {

    public class SoulNPCInput : SoulInput
    {
        public Vector2 Movement;

        Vector2 IVirtualInput.MovementInput => Movement.normalized;

        bool IVirtualInput.IsActionPressed { get => false; }
    }
    public class SoulNPCController: SoulController<SoulNPCInput>
    {
        private CharacterStatus _status;
        protected override CharacterStatus Status => _status;

        private bool eatenByPlayer = false;
        public bool EatenByPlayer => eatenByPlayer;
        public void InitializeStatus(CharacterStatus status)
        {
            _status = status;
            eatenByPlayer = false;
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
            // Movement‚É‚¢‚«‚½‚¢•ûŒü‚ð“ü‚ê‚é
            float rate = 0.6f;
            virtualInput.Movement = rate*virtualInput.Movement + (1f- rate)*Random.insideUnitCircle;
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