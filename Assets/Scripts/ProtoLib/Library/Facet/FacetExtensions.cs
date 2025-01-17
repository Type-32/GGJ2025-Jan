using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ProtoLib.Library.Facet
{
    /// <summary>
    /// Extension methods for facet callbacks.
    /// </summary>
    public static class FacetExtensions
    {
        // Tracks conditions for WatchAndInvoke
        private static Dictionary<string, (bool previousState, Coroutine routine)> watchStates = new();

        /// <summary>
        /// Invokes the callback only if the specified condition is met.
        /// </summary>
        /// <typeparam name="T">Type of the parameter.</typeparam>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="arg">The argument to pass to the callback.</param>
        /// <param name="condition">The condition that must be met for the invoke to occur.</param>
        public static void InvokeIf<T>(this IFacetCallback<Action<T>> callback, T arg, Func<T, bool> condition)
        {
            if (condition(arg))
                callback.Invoke(arg);
        }

        /// <summary>
        /// Watches for a condition and invokes the callback when the condition becomes true.
        /// </summary>
        /// <typeparam name="T">Type of the parameter.</typeparam>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="context">MonoBehaviour context for the coroutine.</param>
        /// <param name="arg">The argument to pass when invoking.</param>
        /// <param name="condition">The condition to watch for.</param>
        /// <param name="watchId">Optional identifier for the watch operation.</param>
        public static void WatchAndInvoke<T>(
            this IFacetCallback<Action<T>> callback, 
            MonoBehaviour context,
            T arg, 
            Func<bool> condition,
            string watchId = null)
        {
            string key = watchId ?? $"{callback.GetHashCode()}_{arg?.GetHashCode()}";
            
            // Stop existing watch if any
            if (watchStates.TryGetValue(key, out var existingWatch))
            {
                if (existingWatch.routine != null)
                    context.StopCoroutine(existingWatch.routine);
                watchStates.Remove(key);
            }

            // Initialize state before starting coroutine
            watchStates[key] = (condition(), null);

            // Create and store coroutine
            Coroutine newRoutine = context.StartCoroutine(WatchRoutine(key));
            watchStates[key] = (watchStates[key].previousState, newRoutine);

            IEnumerator WatchRoutine(string stateKey)
            {
                while (true)
                {
                    bool currentState = condition();
                    
                    if (watchStates.TryGetValue(stateKey, out var state))
                    {
                        if (currentState && !state.previousState)
                        {
                            callback.Invoke(arg);
                        }
                        watchStates[stateKey] = (currentState, state.routine);
                    }
                    else
                    {
                        // State was removed, stop watching
                        yield break;
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
        }

        /// <summary>
        /// Stops watching a specific condition.
        /// </summary>
        /// <param name="callback">The callback containing the watch.</param>
        /// <param name="watchId">The identifier of the watch to stop.</param>
        public static void StopWatch(this IFacetCallback callback, string watchId)
        {
            if (watchStates.TryGetValue(watchId, out var watch))
            {
                if (watch.routine != null)
                    MonoBehaviour.FindObjectOfType<MonoBehaviour>()?.StopCoroutine(watch.routine);
                watchStates.Remove(watchId);
            }
        }
    }
}