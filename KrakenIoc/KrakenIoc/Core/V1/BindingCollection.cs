using AOFL.KrakenIoc.Core.V1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOFL.KrakenIoc.Core.V1
{
    /// <summary>
    /// Contains information about bindings and categories
    /// </summary>
    internal class BindingCollection
    {
        private List<IBinding> _bindings = new List<IBinding>();

        /// <summary>
        /// This cache is used to lookup a binding by category in ~O(n)
        /// </summary>
        private Dictionary<object, IBinding> _categoryCache = new Dictionary<object, IBinding>();

        /// <summary>
        /// Binding with no category - cached
        /// </summary>
        private IBinding _defaultBindingCached = null;

        public void Add(IBinding binding)
        {
            _bindings.Add(binding);
        }

        public void Remove(IBinding binding)
        {
            // Invalidate binding in the cache
            if (binding.Category == null)
            {
                if (_defaultBindingCached == binding)
                {
                    // Remove default binding from the cache
                    _defaultBindingCached = null;
                }

                // Check that we don't have non-cached bindings cached with a default category (Remove() called before any Resolve())
                foreach(var kvp in _categoryCache.Where(kvp => kvp.Value.Category == null).ToList())
                {
                    _categoryCache.Remove(kvp.Key);
                }
            }
            else
            {
                if (_categoryCache.ContainsKey(binding.Category))
                {
                    // Remove cached category
                    _categoryCache.Remove(binding.Category);
                }

                // Check that we don't have non-cached binding with that category
                foreach (var kvp in _categoryCache.Where(kvp => kvp.Value.Category != null && kvp.Value.Category.Equals(binding.Category)).ToList())
                {
                    _categoryCache.Remove(kvp.Key);
                }
            }

            _bindings.Remove(binding);
        }

        public bool HasCategory(object category)
        {
            return GetBindingWithCategory(category) != null;
        }

        public IBinding GetBindingWithCategory(object category)
        {
            // Default category (no category)
            if (category == null)
            {
                return GetDefaultBinding();
            }

            IBinding binding;

            // Check if category cache contains this binding
            if (_categoryCache.ContainsKey(category))
            {
                binding = _categoryCache[category];
                
                if(category.Equals(binding.Category))
                {
                    return binding;
                }
                else
                {
                    // Category was changed i.e. using WithCategory(), invalidate the cache
                    _categoryCache[binding.Category] = binding;
                }
            }

            // Category cache does not contain this binding, OR it contained the wrong one and it was invalidated. Run a lookup and cache it.
            binding = _bindings.FirstOrDefault(b => category.Equals(b.Category));

            if (binding != null)
            {
                _categoryCache[category] = binding;
            }

            return binding;
            
        }

        public IEnumerable<IBinding> GetBindings()
        {
            return _bindings;
        }

        public void Dissolve()
        {
            foreach(var binding in _bindings)
            {
                binding.Dissolve();
            }

            _bindings.Clear();
        }

        private IBinding GetDefaultBinding()
        {
            // Check default binding cache
            if (_defaultBindingCached == null)
            {
                // No default binding cache - run the lookup
                IBinding binding = _bindings.FirstOrDefault(b => b.Category == null);

                if (binding == null)
                {
                    return null;
                }
                else
                {
                    _defaultBindingCached = binding; // Cache
                    return binding;
                }
            }
            else
            {
                // Check if default binding has category changed and needs to be invalidated
                if (_defaultBindingCached.Category != null)
                {
                    _categoryCache[_defaultBindingCached.Category] = _defaultBindingCached;
                    _defaultBindingCached = _bindings.FirstOrDefault(b => b.Category == null);
                }

                return _defaultBindingCached;
            }
        }
    }
}
