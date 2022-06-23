using System;
using System.Collections;
using UnityEngine;

namespace UnityUtilitiesCode.Managers
{
    /// <summary>
    /// This script contains some useful coroutine setups
    /// </summary>
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager instance_;
        public static CoroutineManager Instance => instance_;
        
        private readonly WaitForEndOfFrame oneUpdateFrameDelay_ = new WaitForEndOfFrame();
        private readonly WaitForFixedUpdate oneFixedUpdateFrameDelay_ = new WaitForFixedUpdate();

        private void Awake() => instance_ = this;

        /// <summary>
        /// Runs the passed function after one normal update frame delay
        /// </summary>
        /// <param name="func"> The function that should run after one frame </param>
        /// <returns> The started coroutine </returns>
        public Coroutine RunNextUpdate(Action func)
        {
            var c = StartCoroutine(Routine());
            return c;
            
            IEnumerator Routine()
            {
                yield return oneUpdateFrameDelay_;
                func();
            }
        }
        
        /// <summary>
        /// Runs the passed function after one fixed update frame delay
        /// </summary>
        /// <param name="func"> The function that should run after one frame </param>
        /// <returns> The started coroutine </returns>
        public Coroutine RunNextFixedUpdate(Action func)
        {
            var c = StartCoroutine(Routine());
            return c;
            
            IEnumerator Routine()
            {
                yield return oneFixedUpdateFrameDelay_;
                func();
            }
        }
        
        /// <summary>
        /// Rund the passed function after the given delay in scaled time
        /// </summary>
        /// <param name="func"> The function that should run after the given delay </param>
        /// <param name="delay"> The delay before running the function </param>
        /// <returns> The started coroutine </returns>
        public Coroutine RunWithDelay(Action func, float delay)
        {
            var c = StartCoroutine(Routine());
            return c;

            IEnumerator Routine()
            {
                yield return new WaitForSeconds(delay);
                func();
            }
        }
    }
}