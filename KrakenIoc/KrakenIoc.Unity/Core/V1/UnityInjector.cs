using AOFL.KrakenIoc.Core.V1.Interfaces;
using System;
using System.Reflection;
using UnityEngine;

namespace AOFL.KrakenIoc.Core.V1
{
    public class UnityInjector : Injector, IUnityInjector
    {
        public UnityInjector(IContainer container) : base(container)
        {
        }

        public override object Resolve(Type type, object target)
        {
            return Resolve(type, target, null);
        }
        
        public override object Resolve(Type type, object target, IInjectContext injectContext)
        {
            GameObject gameObject = (GameObject)target;

            if (type.IsInterface)
            {
                LogHandler?.Invoke("Injector: Trying to resolve an interface instead of a concrete type! : {0} ", type);

                return null;
            }

            _currentlyResolvingTypes.Add(type);

            // Ensure cached reflection...
            PreInjectReflection(type);

            if (_cachedMethodConstructor.ContainsKey(type) && _cachedMethodConstructor[type] != null)
            {
                // Include additional dependencies: GameObject target, Container...
                object instance = ExecuteStaticMethodConstructor(_container, injectContext, _cachedMethodConstructor[type], type, target, _container);

                _currentlyResolvingTypes.Remove(type);

                return instance;
            }

            _currentlyResolvingTypes.Remove(type);

            return gameObject.AddComponent(type);
        }

        protected override bool IsRootType(Type type)
        {
            return base.IsRootType(type) || type == typeof(MonoBehaviour);
        }

        protected override bool MightHaveInjectAttribute(MemberInfo memberInfo)
        {
            var declaringType = memberInfo.DeclaringType;

            return declaringType != typeof(MonoBehaviour)
                && declaringType != typeof(Behaviour)
                && declaringType != typeof(Component)
                && declaringType != typeof(UnityEngine.Object)
                && declaringType != typeof(object);
        }

        public void InjectGameObject(GameObject gameObject, bool recurseChildren = false)
        {
            Component[] components = gameObject.GetComponents<Component>();

            foreach (Component component in components)
            {
                if (component != null)
                {
                    // Inject component...
                    Inject(component);
                }
            }

            if (recurseChildren)
            {
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    // Recurse each child...
                    InjectGameObject(gameObject.transform.GetChild(i).gameObject, true);
                }
            }
        }

    }
}
