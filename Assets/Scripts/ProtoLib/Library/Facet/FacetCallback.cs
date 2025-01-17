using System;
using System.Collections.Generic;
using System.Reflection;

namespace ProtoLib.Library.Facet
{
    public class FacetCallback<TDelegate> : IFacetCallback<TDelegate> where TDelegate : Delegate
    {
        private event FacetDelegate Handlers;
        private readonly MethodInfo invokeMethod;
        private readonly Dictionary<TDelegate, FacetDelegate> handlerMappings = new();
        private object[] lastStasisValue; // Store last stasis value
        private bool hasStasisValue; // Track if we have a stasis value
        
        public bool IsReactive { get; }
        public bool IsPaused { get; private set; }

        public FacetCallback(bool reactive = false)
        {
            IsReactive = reactive;
            IsPaused = false;
            invokeMethod = typeof(TDelegate).GetMethod("Invoke");
        }

        public void Subscribe(TDelegate handler)
        {
            var wrappedHandler = new FacetDelegate((args) => handler.DynamicInvoke(args));
            handlerMappings[handler] = wrappedHandler;
            Handlers += wrappedHandler;

            // Immediately invoke with last stasis value if available
            if (hasStasisValue && IsReactive)
            {
                wrappedHandler(lastStasisValue);
            }
        }

        public void Unsubscribe(TDelegate handler)
        {
            if (handlerMappings.TryGetValue(handler, out var wrappedHandler))
            {
                Handlers -= wrappedHandler;
                handlerMappings.Remove(handler);
            }
        }

        public void UnsubscribeAll()
        {
            Handlers = null;
            handlerMappings.Clear();
        }
        
        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
            // Optionally re-invoke last stasis value when resuming
            if (hasStasisValue && IsReactive)
            {
                Handlers?.Invoke(lastStasisValue);
            }
        }

        public void Invoke(params object[] parameters)
        {
            ValidateParameters(parameters);
            Handlers?.Invoke(parameters);
        }

        public void StasisInvoke(params object[] parameters)
        {
            ValidateParameters(parameters);
            lastStasisValue = parameters;
            hasStasisValue = true;
            Handlers?.Invoke(parameters);
        }

        private void ValidateParameters(object[] parameters)
        {
            var delegateParams = invokeMethod.GetParameters();
            if (parameters.Length != delegateParams.Length)
                throw new ArgumentException($"Expected {delegateParams.Length} parameters, got {parameters.Length}");

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != null && !delegateParams[i].ParameterType.IsAssignableFrom(parameters[i].GetType()))
                    throw new ArgumentException($"Parameter {i} type mismatch. Expected {delegateParams[i].ParameterType}, got {parameters[i].GetType()}");
            }
        }
    }
}