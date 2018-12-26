using AOFL.KrakenIoc.Core.V1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AOFL.KrakenIoc.Core.V1
{
    public class UnityBindingMiddleware : IBindingMiddleware
    {
        public object Resolve(IBinding binding, object target = null)
        {
            return Resolve(binding, null, target);
        }

        public object Resolve(IBinding binding, IInjectContext injectContext, object target = null)
        {
            if (typeof(Component).IsAssignableFrom(binding.BoundType))
            {
                return InternalResolveComponent(binding, injectContext, (GameObject)target);
            }

            // Default binding resolve will handle....
            return null;
        }

        private object InternalResolveComponent(IBinding binding, IInjectContext injectContext, GameObject gameObject)
        {
            object addedComponent;

            binding.BoundObjects = binding.BoundObjects ?? new List<object>();

            switch (binding.BindingType)
            {
                case BindingType.Singleton:
                    if (binding.BoundObjects.Count != 0)
                    {
                        binding.NotifyResolved(false, gameObject);

                        return binding.BoundObjects.FirstOrDefault();
                    }

                    gameObject = (gameObject) ?? new GameObject(binding.BoundType.Name);
                    addedComponent = binding.Container.Injector.Resolve(binding.BoundType, gameObject, injectContext);
                    binding.Container.Injector.Inject(addedComponent, injectContext);
                    binding.BoundObjects.Add(addedComponent);

                    binding.NotifyResolved(true, gameObject);

                    return binding.BoundObjects.FirstOrDefault();
                case BindingType.Transient:
                    gameObject = (gameObject) ?? new GameObject(binding.BoundType.Name);
                    addedComponent = binding.Container.Injector.Resolve(binding.BoundType, gameObject, injectContext);
                    binding.Container.Injector.Inject(addedComponent, injectContext);

                    binding.NotifyResolved(true, gameObject);

                    return addedComponent;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
