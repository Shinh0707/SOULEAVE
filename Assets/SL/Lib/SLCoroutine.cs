namespace SL.Lib {

    using UnityEngine;
    using System.Collections;
    using System;

    public class SLCoroutine
    {
        private IEnumerator enumerator;
        private Coroutine coroutine;
        private MonoBehaviour executor;

        public bool IsBusy { get; private set; }
        public bool IsCompleted { get; private set; }

        public event Action OnCompleted;

        public SLCoroutine(IEnumerator enumerator = null)
        {
            this.enumerator = enumerator;
            IsBusy = false;
            IsCompleted = false;
        }

        public SLCoroutine Start(MonoBehaviour executor)
        {
            if (IsBusy)
            {
                Debug.LogWarning("Coroutine is already running.");
                return this;
            }

            this.executor = executor;
            coroutine = executor.StartCoroutine(WrapEnumerator());
            return this;
        }

        public void Stop()
        {
            if (coroutine != null && executor != null)
            {
                executor.StopCoroutine(coroutine);
                Reset();
            }
        }

        public void Restart(MonoBehaviour executor)
        {
            Stop();
            Start(executor);
        }

        public void Restart(MonoBehaviour executor, IEnumerator newEnumerator)
        {
            Stop();
            this.enumerator = newEnumerator;
            Start(executor);
        }
        public void RunOrRestart(MonoBehaviour executor, IEnumerator newEnumerator)
        {
            if (IsBusy)
            {
                Restart(executor, newEnumerator);
            }
            else
            {
                this.enumerator = newEnumerator;
                Start(executor);
            }
        }
        private IEnumerator WrapEnumerator()
        {
            IsBusy = true;
            IsCompleted = false;

            yield return enumerator;

            Reset();
            OnCompleted?.Invoke();
        }

        private void Reset()
        {
            IsBusy = false;
            IsCompleted = true;
            coroutine = null;
        }
    }

}
