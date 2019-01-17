using AOFL.KrakenIoc.Core.V1.Exceptions;
using AOFL.KrakenIoc.Core.V1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AOFL.KrakenIoc.Core.V1
{
    public delegate IContainer GlobalContainerFactoryHandler();

    /// <inheritdoc/>
    public class Container : IContainer, IDisposable
    {
        private Dictionary<Type, BindingCollection> _bindings = new Dictionary<Type, BindingCollection>();
        private Dictionary<Type, BindingCollection> _clonedBindings = new Dictionary<Type, BindingCollection>();
        private readonly Dictionary<Type, BindingCollection> _inheritedBindings = new Dictionary<Type, BindingCollection>();
        private IInjector _injector;

        public LogHandler LogHandler { get; set; }

        public Container() { }
        public Container(IInjector injector)
        {
            Injector = injector;
        }

        public void Dispose()
        {
            // Dispose bindings...
            var keys = _bindings.Keys;

            for (int i = keys.Count - 1; i >= 0; i--)
            {
                var key = keys.ElementAt(i);
                Dissolve(key);
            }

            // Dispose cloned bindings...
            var clonedKeys = _clonedBindings.Keys;

            for (int i = clonedKeys.Count - 1; i >= 0; i--)
            {
                var key = clonedKeys.ElementAt(i);
                Dissolve(key);
            }

            _bindings = null;
            _clonedBindings = null;
        }

        public bool ShouldLog { get; set; }

        public IInjector Injector
        {
            get
            {
                if (_injector == null)
                {
                    _injector = new Injector(this);
                }

                return _injector;
            }
            set
            {
                _injector = value;
            }
        }

        public IBinding Bind<T>()
        {
            return Bind<T>(typeof(T));
        }

        public IBinding Bind<TInterface, TImplementation>() where TImplementation : TInterface
        {
            return Bind<TInterface>().To<TImplementation>();
        }

        public IBinding Bind<T>(object category)
        {
            return Bind<T>().WithCategory(category);
        }

        public IBinding Bind<T>(T value)
        {
            IBinding binding = Bind<T>(typeof(T));
            binding.BoundObjects.Add(value);
            binding.AsSingleton();

            return binding;
        }
        
        public IBinding Bind<T>(Type type)
        {
            if (!_bindings.ContainsKey(typeof(T)))
            {
                _bindings.Add(typeof(T), new BindingCollection());
            }

            Binding binding = new Binding
            {
                BinderType = typeof(T),
                BoundType = type,
                Container = this
            };

            _bindings[typeof(T)].Add(binding);

            return binding;
       
        }

        private void LogError(string format, params object[] args)
        {
            if (!ShouldLog)
            {
                return;
            }

           LogHandler?.Invoke(format, args);
        }

        public IBinding GetBinding<T>(object category)
        {
            return GetBinding(typeof(T), category);
        }

        public IBinding GetBinding<T>()
        {
            return GetBinding(typeof(T), null);
        }

        public IBinding GetBinding(Type type)
        {
            return GetBinding(type, null);
        }

        public IBinding GetBinding(Type type, object category)
        {
            // Local bindings...
            if (_bindings.ContainsKey(type))
            {
                IBinding binding = _bindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding;
                }
            }

            // Cloned bindings...
            if (_clonedBindings.ContainsKey(type))
            {
                IBinding binding = _clonedBindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding;
                }
            }

            // Inherited bindings...
            if (_inheritedBindings.ContainsKey(type))
            {
                IBinding binding = _inheritedBindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding;
                }
            }

            return null;
        }

        public List<IBinding> GetBindings()
        {
            List<IBinding> bindings = new List<IBinding>();

            // Self bindings...
            for (int i = 0; i < _bindings.Count; i++)
            {
                var bindingCollection = _bindings.ElementAt(i).Value;

                bindings.AddRange(bindingCollection.GetBindings());
            }

            // Cloned bindings...
            for (int i = 0; i < _clonedBindings.Count; i++)
            {
                var bindingCollection = _clonedBindings.ElementAt(i).Value;

                bindings.AddRange(bindingCollection.GetBindings());
            }

            // Inherited bindings...
            for (int i = 0; i < _inheritedBindings.Count; i++)
            {
                var bindingCollection = _inheritedBindings.ElementAt(i).Value;

                bindings.AddRange(bindingCollection.GetBindings());
            }

            return bindings;
        }

        public List<Type> GetBindedTypes()
        {
            return _bindings.Keys.ToList();
        }

        /// <summary>
        /// Dissolves any bindings for type.
        /// </summary>
        /// <param name="type">Type.</param>
        public void Dissolve(Type type)
        {
            // Self...
            if (_bindings.ContainsKey(type))
            {
                var bindingCollection = _bindings[type];

                bindingCollection.Dissolve();

                _bindings.Remove(type);
            }

            // Cloned bindings...
            if(_clonedBindings.ContainsKey(type))
            {
                var bindingCollection = _clonedBindings[type];

                bindingCollection.Dissolve();

                _clonedBindings.Remove(type);
            }
        }

        /// <summary>
        /// Dissolves any bindings for type.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Dissolve<T>()
        {
            Dissolve(typeof(T));
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public T Resolve<T>(object target)
        {
            return (T)Resolve(typeof(T), target);
        }

        public T Resolve<T>(string category)
        {
            return ResolveWithCategory<T>(category);
        }

        public object Resolve(Type type)
        {
            return Resolve(type, null);
        }

        public object Resolve(Type type, IInjectContext parentContext)
        {
            return ResolveWithCategory(type, null, parentContext);
        }

        public object Resolve(Type type, object target)
        {
            return ResolveWithCategory(type, target, null, null);
        }

        public T ResolveWithCategory<T>(object category)
        {
            return (T)ResolveWithCategory(typeof(T), category);
        }

        public T ResolveWithCategory<T>(object target, object category)
        {
            return (T)ResolveWithCategory(typeof(T), target, category);
        }
        
        public object ResolveWithCategory(Type type, object category)
        {
            return ResolveWithCategory(type, category, null);
        }

        public object ResolveWithCategory(Type type, object category, IInjectContext parentContext)
        {
            // Check self-bindings...
            if (_bindings.ContainsKey(type))
            {
                IBinding binding = _bindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding.Resolve(parentContext);
                }
                else
                {
                    throw new MissingBindingException(type, category);
                }
            }

            // Cloned self-bindings...
            if (_clonedBindings.ContainsKey(type))
            {
                IBinding binding = _clonedBindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding.Resolve(parentContext);
                }
                else
                {
                    throw new MissingBindingException(type, category);
                }
            }

            // Check inherited bindings...
            if (_inheritedBindings.ContainsKey(type))
            {
                IBinding binding = _inheritedBindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding.Resolve(parentContext);
                }
                else
                {
                    throw new MissingBindingException(type, category);
                }
            }


            throw new MissingBindingException(type, category);
        }

        public object ResolveWithCategory(Type type, object target, object category)
        {
            return ResolveWithCategory(type, target, category, null);
        }

        public object ResolveWithCategory(Type type, object target, object category, IInjectContext parentContext)
        {
            // Check self-bindings...
            if (_bindings.ContainsKey(type))
            {
                IBinding binding = _bindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding.Resolve(parentContext, target);
                }
                else
                {
                    throw new MissingBindingException(type, category);
                }
            }

            // Check cloned bindings...
            if (_clonedBindings.ContainsKey(type))
            {
                IBinding binding = _clonedBindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding.Resolve(parentContext, target);
                }
                else
                {
                    throw new MissingBindingException(type, category);
                }
            }

            // Check inherited-bindings...
            if (_inheritedBindings.ContainsKey(type))
            {
                IBinding binding = _inheritedBindings[type].GetBindingWithCategory(category);

                if (binding != null)
                {
                    return binding.Resolve(parentContext, target);
                }
                else
                {
                    throw new MissingBindingException(type, category);
                }
            }

            throw new MissingBindingException(type, category);
        }

        public bool HasBindingFor<T>()
        {
            return HasBindingFor(typeof(T));
        }

        public bool HasBindingFor(Type type)
        {
            return _inheritedBindings.ContainsKey(type) || _clonedBindings.ContainsKey(type) || _bindings.ContainsKey(type);
        }

        public bool HasBindingForCategory<T>(object category)
        {
            return HasBindingForCategory(typeof(T), category);
        }

        public bool HasBindingForCategory(Type type, object category)
        {
            return (_inheritedBindings.ContainsKey(type) && _inheritedBindings[type].HasCategory(category)) ||
                (_clonedBindings.ContainsKey(type) && _clonedBindings[type].HasCategory(category)) ||
                (_bindings.ContainsKey(type) && _bindings[type].HasCategory(category));
        }

        public void Inherit(IContainer container)
        {
            List<IBinding> bindings = container.GetBindings();

            for(int i = 0; i < bindings.Count; i++)
            {
                IBinding binding = bindings[i];

                if (binding.BindingType == BindingType.Singleton)
                {
                    Binding inheritedBinding = new Binding
                    {
                        Container = this
                    };

                    inheritedBinding.Inherit(binding);
                    AddInheritedBinding(inheritedBinding);
                }
                else
                {
                    // Clone transient bindings, allowing to resolve from this, inherited container (instead of parent)
                    Binding clonedBinding = new Binding()
                    {
                        Container = this
                    };
                    clonedBinding.CloneFrom(binding);
                    AddClonedBinding(clonedBinding);
                }
            }
        }

        private void AddInheritedBinding(Binding binding)
        {
            if (!_inheritedBindings.ContainsKey(binding.BinderType))
            {
                _inheritedBindings.Add(binding.BinderType, new BindingCollection());
            }

            var bindingCollection = _inheritedBindings[binding.BinderType];

            IBinding existingBinding = bindingCollection.GetBindingWithCategory(binding.Category);

            if (existingBinding == null)
            {
                bindingCollection.Add(binding);
            }
            else
            {
                if (binding.Category == null)
                {
                    throw new TypeAlreadyBoundException(binding.BinderType);
                }
                else
                {
                    throw new TypeCategoryAlreadyBoundException(binding.BinderType, binding.Category);
                }
            }
        }

        private void AddClonedBinding(Binding binding)
        {
            if (!_clonedBindings.ContainsKey(binding.BinderType))
            {
                _clonedBindings.Add(binding.BinderType, new BindingCollection());
            }

            var bindingCollection = _clonedBindings[binding.BinderType];

            IBinding existingBinding = bindingCollection.GetBindingWithCategory(binding.Category);

            if (existingBinding == null)
            {
                bindingCollection.Add(binding);
            }
            else
            {
                if (binding.Category == null)
                {
                    throw new TypeAlreadyBoundException(binding.BinderType);
                }
                else
                {
                    throw new TypeCategoryAlreadyBoundException(binding.BinderType, binding.Category);
                }
            }
        }


        public void Bootstrap<T>() where T : IBootstrap
        {
            Bootstrap(typeof(T));
        }

        public void Bootstrap(Type type)
        {
            IBootstrap bootstrap = (IBootstrap)Activator.CreateInstance(type);
            bootstrap?.SetupBindings(this);
        }
    }

}