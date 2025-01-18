using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ProtoLib.Library.Facet
{
    /// <summary>
    /// API for managing facet callbacks.
    /// </summary>
    public class FacetAPI
    {
        private Dictionary<string, IFacetCallback> callbacks = new();
        private Dictionary<string, List<(object subscriber, string location)>> subscriptionInfo = new();
        private bool isGloballyPaused = false;

        /// <summary>
        /// Pauses all callbacks in the system. While paused, no callbacks will be invoked.
        /// </summary>
        public void PauseAll()
        {
            isGloballyPaused = true;
            foreach (var callback in callbacks.Values)
            {
                callback.Pause();
            }
        }

        /// <summary>
        /// Resumes all callbacks in the system.
        /// </summary>
        public void ResumeAll()
        {
            isGloballyPaused = false;
            foreach (var callback in callbacks.Values)
            {
                callback.Resume();
            }
        }

        /// <summary>
        /// Pauses specific callbacks by their names.
        /// </summary>
        /// <param name="callbackNames">Names of the callbacks to pause.</param>
        public void PauseSpecific(params string[] callbackNames)
        {
            foreach (var name in callbackNames)
            {
                if (callbacks.TryGetValue(name, out var callback))
                {
                    callback.Pause();
                }
            }
        }

        /// <summary>
        /// Resumes specific callbacks by their names.
        /// </summary>
        /// <param name="callbackNames">Names of the callbacks to resume.</param>
        public void ResumeSpecific(params string[] callbackNames)
        {
            foreach (var name in callbackNames)
            {
                if (callbacks.TryGetValue(name, out var callback))
                {
                    callback.Resume();
                }
            }
        }

        /// <summary>
        /// Gets whether the callback system is globally paused.
        /// </summary>
        public bool IsGloballyPaused => isGloballyPaused;

        /// <summary>
        /// Gets whether a specific callback is paused.
        /// </summary>
        /// <param name="callbackName">Name of the callback to check.</param>
        /// <returns>True if the callback exists and is paused, false otherwise.</returns>
        public bool IsCallbackPaused(string callbackName)
        {
            return callbacks.TryGetValue(callbackName, out var callback) && callback.IsPaused;
        }

        /// <summary>
        /// Gets a callback by name with the specified delegate type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of delegate for the callback.</typeparam>
        /// <param name="name">The name of the callback.</param>
        /// <returns>The facet callback instance.</returns>
        public IFacetCallback<TDelegate> Get<TDelegate>(string name) where TDelegate : Delegate
            => (IFacetCallback<TDelegate>)callbacks[name];

        // Debug methods
        public class CallbackInfo
        {
            public string Name { get; set; }
            public Type DelegateType { get; set; }
            public bool IsReactive { get; set; }
            public List<(object subscriber, string location)> Subscribers { get; set; }
            public ParameterInfo[] Parameters { get; set; }
        }

        /// <summary>
        /// Returns debug information about all registered callbacks.
        /// </summary>
        public IEnumerable<CallbackInfo> GetCallbacksInfo()
        {
            foreach (var (name, callback) in callbacks)
            {
                var delegateType = callback.GetType().GetGenericArguments()[0];
                yield return new CallbackInfo
                {
                    Name = name,
                    DelegateType = delegateType,
                    IsReactive = callback.IsReactive,
                    Subscribers = subscriptionInfo.TryGetValue(name, out var subs) ? subs : new(),
                    Parameters = delegateType.GetMethod("Invoke")?.GetParameters()
                };
            }
        }

        /// <summary>
        /// Logs debug information about all callbacks to the console.
        /// </summary>
        public void LogCallbacksInfo()
        {
            foreach (var info in GetCallbacksInfo())
            {
                Debug.Log($"=== Callback: {info.Name} ===");
                Debug.Log($"Type: {info.DelegateType.Name}");
                Debug.Log($"Reactive: {info.IsReactive}");
                Debug.Log("Parameters:");
                foreach (var param in info.Parameters)
                {
                    Debug.Log($"  - {param.ParameterType.Name} {param.Name}");
                }
                Debug.Log("Subscribers:");
                foreach (var (subscriber, location) in info.Subscribers)
                {
                    Debug.Log($"  - {subscriber.GetType().Name} at {location}");
                }
                Debug.Log("==================");
            }
        }

        // Track subscription locations
        private void TrackSubscription(string callbackName, object subscriber)
        {
            var stackTrace = new System.Diagnostics.StackTrace(2, true);
            var frame = stackTrace.GetFrame(0);
            var location = $"{frame.GetFileName()}:{frame.GetFileLineNumber()}";

            if (!subscriptionInfo.ContainsKey(callbackName))
                subscriptionInfo[callbackName] = new List<(object, string)>();
            
            subscriptionInfo[callbackName].Add((subscriber, location));
        }

        private void AddCallback<TDelegate>(string name, FacetCallback<TDelegate> callback) where TDelegate : Delegate
            => callbacks[name] = callback;

        /// <summary>
        /// Creates a new FacetAPI builder instance.
        /// </summary>
        /// <returns>A new builder for configuring the FacetAPI.</returns>
        public static FacetBuilder Create() => new();

        /// <summary>
        /// Builder for configuring FacetAPI instances.
        /// </summary>
        public class FacetBuilder
        {
            private Dictionary<string, (Type type, object callback)> callbacks = new();

            /// <summary>
            /// Adds a regular callback.
            /// </summary>
            /// <typeparam name="TDelegate">The delegate type for the callback.</typeparam>
            /// <param name="name">The name of the callback.</param>
            public FacetBuilder Callback<TDelegate>(string name) where TDelegate : Delegate
            {
                callbacks[name] = (typeof(TDelegate), new FacetCallback<TDelegate>(reactive: false));
                return this;
            }

            /// <summary>
            /// Adds a realtime callback that updates frequently.
            /// </summary>
            /// <typeparam name="TDelegate">The delegate type for the callback.</typeparam>
            /// <param name="name">The name of the callback.</param>
            public FacetBuilder Realtime<TDelegate>(string name) where TDelegate : Delegate
            {
                callbacks[name] = (typeof(TDelegate), new FacetCallback<TDelegate>(reactive: true));
                return this;
            }

            /// <summary>
            /// Builds and returns the configured FacetAPI instance.
            /// </summary>
            public FacetAPI Build()
            {
                var api = new FacetAPI();
                foreach (var (name, (type, callback)) in callbacks)
                {
                    typeof(FacetAPI)
                        .GetMethod(nameof(AddCallback), BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.MakeGenericMethod(type)
                        .Invoke(api, new[] { name, callback });
                }
                return api;
            }
        }
    }
}