using AOFL.KrakenIoc.Core.V1.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AOFL.KrakenIoc.Core.V1
{
    public class Context : MonoBehaviour, IContext
    {
        public static event Action<Context> Initialized;
        public static event Action<Context> Destroyed;

        public static readonly Dictionary<Type, IContext> ContextInstances = new Dictionary<Type, IContext>();

        public IContainer Container { get; private set; }

        public static IContext GetContext(Type contextType)
        {
            if (ContextInstances.ContainsKey(contextType))
            {
                return ContextInstances[contextType];
            }

            return null;
        }

        public static T GetContext<T>() where T : IContext
        {
            // Check for exact type...
            if (ContextInstances.ContainsKey(typeof(T)))
            {
                return (T)ContextInstances[typeof(T)];
            }

            // Check child types...
            foreach(var pair in ContextInstances)
            {
                if(typeof(T).IsAssignableFrom(pair.Key))
                {
                    return (T)pair.Value;
                }
            }

            return default(T);
        }

        protected void Awake()
        {
            if (!ContextInstances.ContainsKey(GetType()))
            {
                Container = new Container();
                Container.Injector = new UnityInjector(Container);
                Container.Injector.LogHandler = Debug.LogErrorFormat;
                Container.LogHandler = Debug.LogErrorFormat;

                // Middleware for components etc...
                Binding.AddBindingMiddleware(new UnityBindingMiddleware());

                ContextInstances.Add(GetType(), this);
                OnSetupBindings();

                Initialized?.Invoke(this);
            }
            else
            {
                throw new Exception("Duplicate key exists for Context type: " + GetType());
            }
        }

        protected virtual void Start()
        {
            OnStart();
        }

        protected void OnDestroy()
        {
            Destroyed?.Invoke(this);

            this.OnDestroyed();

            Container.Dispose();
            Container = null;
            ContextInstances.Remove(GetType());

            GC.Collect();
        }

        protected virtual void OnSetupBindings()
        {

        }

        protected virtual void OnStart()
        {

        }

        protected virtual void OnDestroyed()
        {

        }

        public void Inherit<T>() where T : IContext
        {
            IContext context = GetContext<T>();

            if(context != null)
            {
                Container.Inherit(context.Container);
            }
            else
            {
                throw new NullReferenceException("Context instance doesn't exist for Context to Inherit: " + typeof(T));
            }
        }
    }
}