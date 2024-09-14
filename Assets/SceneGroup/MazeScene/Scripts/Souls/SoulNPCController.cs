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

    public struct SoulNPCStatus
    {
        public float MaxMP;
        public float MaxIntensity;
        public float RestoreMPPerSecond;
        public float RestoreIntensityPerSecond;
    }
    public class SoulNPCController: SoulController<SoulNPCInput>
    {
        private SoulNPCStatus status;
        public override float MaxMP => status.MaxMP;

        public override float MaxIntensity => status.MaxIntensity;

        public override float RestoreIntensityPerSecond => status.RestoreIntensityPerSecond;

        public override float RestoreMPPerSecond => status.RestoreMPPerSecond;
        protected override void HandleMovement()
        {
            Think();
            base.HandleMovement();
        }
        public void InitializeStatus(SoulNPCStatus status)
        {
            this.status = status;
        }
        protected override void InitializeVirtualInput()
        {
            virtualInput = new SoulNPCInput();
        }
        private void Think()
        {
            // Movement‚É‚¢‚«‚½‚¢•ûŒü‚ð“ü‚ê‚é
            virtualInput.Movement = Random.insideUnitCircle;
        }
    }

}