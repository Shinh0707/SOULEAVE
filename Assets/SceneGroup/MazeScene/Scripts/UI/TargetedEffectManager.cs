using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL.Lib
{

    public class TargetedEffectManager : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [AnimationTrigger("animator"), SerializeField] private string OnStartTargeted;
        [AnimationTrigger("animator"), SerializeField] private string OnEndTargeted;

        private void Reset()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }
        public void StartTargeted()
        {
            animator?.SetTrigger(OnStartTargeted);
        }
        public void EndTargeted()
        {
            animator?.SetTrigger(OnEndTargeted);
        }
    }
}