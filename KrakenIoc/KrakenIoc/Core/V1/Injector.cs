using AOFL.KrakenIoc.Core.V1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AOFL.KrakenIoc.Core.V1
{
    /// <inheritdoc/>
    public class Injector : IInjector
    {
        protected readonly Dictionary<Type, ConstructorInfoCache> _cachedConstructor = new Dictionary<Type, ConstructorInfoCache>();
        protected readonly Dictionary<Type, ConstructorInfoCache> _cachedDefaultConstructor = new Dictionary<Type, ConstructorInfoCache>();
        protected readonly Dictionary<Type, MethodInfoCache> _cachedMethodConstructor = new Dictionary<Type, MethodInfoCache>();
        protected readonly Dictionary<Type, IEnumerable<MethodInfoCache>> _cachedMethods = new Dictionary<Type, IEnumerable<MethodInfoCache>>();
        protected readonly Dictionary<Type, IEnumerable<MemberInfoCache>> _cachedFields = new Dictionary<Type, IEnumerable<MemberInfoCache>>();
        protected readonly Dictionary<Type, IEnumerable<MemberInfoCache>> _cachedProperties = new Dictionary<Type, IEnumerable<MemberInfoCache>>();
        protected readonly List<Type> _currentlyResolvingTypes = new List<Type>();
        protected readonly IContainer _container;
        protected readonly bool _shouldLog;

        public LogHandler LogHandler { get; set; }

        public Injector(IContainer container)
        {
            _container = container;
            _shouldLog = container.ShouldLog;
        }

        /// <summary>
        /// Ensures the dictionaries have cached information
        /// for reflection purposes.
        /// </summary>
        /// <param name="type">The type to reflect.</param>
        protected void PreInjectReflection(Type type)
        {
            if (!_cachedConstructor.ContainsKey(type))
            {
                var filtered = FindConstructorRecursive(type);

                if (filtered != null)
                {
                    _cachedConstructor.Add(type, filtered);
                }
            }

            if (!_cachedMethodConstructor.ContainsKey(type))
            {
                var filtered = FindConstructorMethodRecursive(type);

                if(filtered != null)
                {
                    _cachedMethodConstructor.Add(type, filtered);
                }
            }

            if (!_cachedMethods.ContainsKey(type))
            {
                var filtered = FindMethodsRecursive(type);

                if(filtered != null)
                {
                    _cachedMethods.Add(type, filtered);
                }
            }

            if (!_cachedFields.ContainsKey(type))
            {
                var filtered = FindFieldsRecursive(type);

                _cachedFields.Add(type, filtered);
            }

            if (!_cachedProperties.ContainsKey(type))
            {
                var filtered = FindPropertiesRecursive(type);

                _cachedProperties.Add(type, filtered);
            }
        }

        public virtual object Resolve(Type type, object target)
        {
            throw new NotImplementedException();
        }

        public virtual object Resolve(Type type, object target, IInjectContext injectContext)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type type)
        {
            return Resolve(type, null);
        }

        public object Resolve(Type type, IInjectContext injectContext)
        {
            if (type.IsInterface)
            {
                LogHandler.Invoke("Injector: {0}", "Trying to resolve an interface instead of a concrete type! : " + type);

                return null;
            }

            _currentlyResolvingTypes.Add(type);

            // Ensure cached reflection...
            PreInjectReflection(type);
            
            // Execute static constructor method
            if (_cachedMethodConstructor.ContainsKey(type) && _cachedMethodConstructor[type] != null)
            { 
                // Include additional dependency of the container...
                object instance = ExecuteStaticMethodConstructor(_container, injectContext, _cachedMethodConstructor[type], type, _container);

                _currentlyResolvingTypes.Remove(type);

                return instance;
            }

            // Execute constructor
            if(_cachedConstructor.ContainsKey(type) && _cachedConstructor[type] != null)
            {
                // Include additional dependency of the container...
                object instance = ExecuteConstructor(_container, injectContext, _cachedConstructor[type], type, _container);

                _currentlyResolvingTypes.Remove(type);

                return instance;
            }
            else
            {
                _currentlyResolvingTypes.Remove(type);

                // Invoke defaut constructor...
                object instance = CreateInstance(type);

                return instance;
            }
        }

        private object CreateInstance(Type type)
        {
            if (!_cachedDefaultConstructor.ContainsKey(type))
            {
                // Find first constructor without parameters...
                ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                ConstructorInfo constructor = null;

                foreach (ConstructorInfo info in constructors)
                {
                    if(info.GetParameters().Length == 0)
                    {
                        constructor = info;
                        break;
                    }
                }

                if (constructor != null)
                {
                    ConstructorInfoCache constructorInfoCache = new ConstructorInfoCache(constructor, GetParameterInfoCache(constructor.GetParameters()));

                    _cachedDefaultConstructor.Add(type, constructorInfoCache);
                }
                else
                {
                    _cachedDefaultConstructor.Add(type, null);
                }
            }

            if (_cachedDefaultConstructor[type] != null)
            {
                return _cachedDefaultConstructor[type].ConstructorInfo.Invoke(null);
            }

            return Activator.CreateInstance(type);
        }

        protected object ExecuteStaticMethodConstructor(IContainer container, IInjectContext parentContext, MethodInfoCache method, Type type, params object[] addDependencies)
        {
            ParameterInfoCache[] parameters = method.Parameters;

            if(parameters == null || parameters.Length == 0)
            {
                return method.MethodInfo.Invoke(null, null);
            }

            object[] paramObjects = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfoCache parameter = parameters[i];
                ParameterInfo parameterInfo = parameter.ParameterInfo;

                // Can we resolve this?
                if (container.HasBindingFor(parameterInfo.ParameterType))
                {
                    bool isCircularDependency = false;

                    foreach (var resolvingType in _currentlyResolvingTypes)
                    {
                        if (parameterInfo.ParameterType.IsAssignableFrom(resolvingType))
                        {
                            isCircularDependency = true;
                        }
                    }

                    // Prevent recursive / circular dependency...
                    if (isCircularDependency)
                    {
                        LogHandler?.Invoke("Injector: Circular dependency detected in Method {0}.{1}, parameter index: {2}", type, method.MethodInfo.Name, i);
                        paramObjects[i] = null;
                    }
                    else
                    {
                        InjectContext injectContext = new InjectContext(container, method.DeclaringType, parentContext);

                        Type parameterType = parameterInfo.ParameterType;
                        object paramInstance = container.ResolveWithCategory(parameterType, parameter.InjectAttribute?.Category, injectContext);
                        paramObjects[i] = paramInstance;
                    }
                }
                else
                {
                    bool hasAdditionalDependency = false;

                    // Check additional dependencies...
                    foreach (object dependency in addDependencies)
                    {
                        if (parameterInfo.ParameterType.IsInstanceOfType(dependency))
                        {
                            hasAdditionalDependency = true;
                            paramObjects[i] = dependency;

                            break;
                        }
                    }

                    if (!hasAdditionalDependency)
                    {
                        LogHandler?.Invoke("Injector: {0} - Type is not bound in the container, assigning as null {1}, method {2}, parameter index: {2}", type, parameterInfo.ParameterType, method.MethodInfo.Name, i);
                        paramObjects[i] = null;
                    }
                }
            }

            // Invoke constructor...
            object instance = method.MethodInfo.Invoke(null, paramObjects);

            return instance;
        }

        private object ExecuteConstructor(IContainer container, IInjectContext parentContext, ConstructorInfoCache constructor, Type type, params object[] addDependencies)
        {
            ParameterInfoCache[] parameters = constructor.Parameters;
            
            if(parameters == null || parameters.Length == 0)
            {
                return constructor.ConstructorInfo.Invoke(null);
            }

            // Resolve parameters

            object[] paramObjects = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfoCache parameter = parameters[i];
                ParameterInfo parameterInfo = parameter.ParameterInfo;

                // Can we resolve this?
                if (container.HasBindingFor(parameterInfo.ParameterType))
                {
                    bool isCircularDependency = false;

                    foreach (Type resolvingType in _currentlyResolvingTypes)
                    {
                        if (parameterInfo.ParameterType.IsAssignableFrom(resolvingType))
                        {
                            isCircularDependency = true;
                        }
                    }

                    // Prevent recursive / circular dependency...
                    if (isCircularDependency)
                    {
                        LogHandler?.Invoke("Injector: Circular dependency detected in Type {0} parameter index: {1}", type, i);
                        paramObjects[i] = null;
                    }
                    else
                    {

                        InjectContext injectContext = new InjectContext(container, constructor.DeclaringType, parentContext);

                        object paramInstance = container.ResolveWithCategory(parameterInfo.ParameterType, parameters[i].InjectAttribute?.Category, injectContext);
                        paramObjects[i] = paramInstance;
                    }
                }
                else
                {
                    bool hasAdditionalDependency = false;

                    // Check additional dependencies...
                    foreach (object dependency in addDependencies)
                    {
                        if (parameterInfo.ParameterType.IsInstanceOfType(dependency))
                        {
                            hasAdditionalDependency = true;
                            paramObjects[i] = dependency;

                            break;
                        }
                    }

                    if (!hasAdditionalDependency)
                    {
                        LogHandler?.Invoke("Injector: {0} - Type is not bound in the container, assigning as null {1}, parameter index: {2}", type, parameterInfo.ParameterType, i);
                        paramObjects[i] = null;
                    }
                }
            }


            // Invoke constructor...
            object instance = constructor.ConstructorInfo.Invoke(paramObjects);

            return instance;
        }

        private void InjectMethod(IContainer container, IInjectContext parentContext, Type type, object obj, MethodInfoCache method)
        {
            ParameterInfoCache[] parameters = method.Parameters;
            if(parameters == null || parameters.Length == 0)
            {
                method.MethodInfo.Invoke(obj, null);
                return;
            }

            object[] paramObjects = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfoCache parameter = parameters[i];
                ParameterInfo parameterInfo = parameter.ParameterInfo;

                // Can we resolve this?
                if (container.HasBindingFor(parameterInfo.ParameterType))
                {
                    // Prevent recursive / circular dependency...
                    if (parameterInfo.ParameterType.IsAssignableFrom(type) || _currentlyResolvingTypes.Contains(parameterInfo.ParameterType))
                    {
                        LogHandler?.Invoke("Injector: Circular dependency detected in Method {0}.{1}, parameter index: {2}", type, method.MethodInfo.Name, i);
                        paramObjects[i] = null;
                    }
                    else
                    {
                        InjectContext injectContext = new InjectContext(container, method.DeclaringType, parentContext);

                        Type parameterType = parameterInfo.ParameterType;
                        
                        object paramInstance = container.ResolveWithCategory(parameterType, parameter.InjectAttribute?.Category, injectContext);
                        paramObjects[i] = paramInstance;
                    }
                }
                else
                {
                    LogHandler?.Invoke("Injector: {0} - Type is not bound in the container, assigning as null {1}, method {2}, parameter index: {2}", type, parameterInfo.ParameterType, method.MethodInfo.Name, i);
                    paramObjects[i] = null;
                }
            }

            method.MethodInfo.Invoke(obj, paramObjects.ToArray());
        }
        
        public void Inject(object objValue)
        {
            Inject(objValue, null);
        }

        public void Inject(object objValue, IInjectContext parentContext)
        {
            Type tType = objValue.GetType();

            // Ensure cached reflection...
            PreInjectReflection(tType);

            _currentlyResolvingTypes.Add(tType);

            // Inject methods...
            for (int i = 0; i < _cachedMethods[tType].Count(); i++)
            {
                MethodInfoCache method = _cachedMethods[tType].ElementAt(i);

                InjectMethod(_container, parentContext, tType, objValue, method);
            }

            // Inject into Fields...
            for (int i = 0; i < _cachedFields[tType].Count(); i++)
            {
                MemberInfoCache memberCache = _cachedFields[tType].ElementAt(i);
                FieldInfo field = (FieldInfo) memberCache.MemberInfo;

                bool isCircularReference = false;

                if(field.FieldType.IsAssignableFrom(typeof(IContainer)))
                {
                    field.SetValue(objValue, _container);
                    continue;
                }

                foreach (Type type in _currentlyResolvingTypes)
                {
                    if (field.FieldType.IsAssignableFrom(type))
                    {
                        var binding = _container.GetBinding(field.FieldType, memberCache.InjectAttribute.Category);

                        if (binding != null && binding.BindingType == BindingType.Transient)
                        {
                            isCircularReference = true;
                        }
                    }
                }

                if (isCircularReference)
                {
                    LogHandler?.Invoke("Injector: Circular dependency detected in Field {0}.{1}", tType, field.Name);
                }
                else
                {
                    InjectContext injectContext = new InjectContext(_container, memberCache.DeclaringType, parentContext);

                    object fieldValue = _container.ResolveWithCategory(field.FieldType, memberCache.InjectAttribute.Category, injectContext);

                    field.SetValue(objValue, fieldValue);
                }
            }

            // Inject into Properties...
            for (int i = 0; i < _cachedProperties[tType].Count(); i++)
            {
                MemberInfoCache memberCache = _cachedProperties[tType].ElementAt(i);
                PropertyInfo property = (PropertyInfo) memberCache.MemberInfo;

                bool isCircularReference = false;

                if (property.PropertyType.IsAssignableFrom(typeof(IContainer)))
                {
                    property.SetValue(objValue, _container, null);
                    continue;
                }

                foreach (Type type in _currentlyResolvingTypes)
                {
                    if (property.PropertyType.IsAssignableFrom(type))
                    {
                        var binding = _container.GetBinding(property.PropertyType, memberCache.InjectAttribute.Category);

                        if (binding != null && binding.BindingType == BindingType.Transient)
                        {
                            isCircularReference = true;
                        }
                    }
                }

                if (isCircularReference)
                {
                    LogHandler?.Invoke("Injector: Circular dependency detected in Property {0}.{1}", tType, property.Name);
                }
                else
                {
                    InjectContext injectContext = new InjectContext(_container, memberCache.DeclaringType, parentContext);

                    object propertyValue = _container.ResolveWithCategory(property.PropertyType, memberCache.InjectAttribute.Category, injectContext);
                    
                    property.SetValue(objValue, propertyValue, null);
                }
            }

            _currentlyResolvingTypes.Remove(tType);
        }

        private ConstructorInfoCache FindConstructorRecursive(Type type)
        {
            ConstructorInfo constructorInfo = null;

            ConstructorInfo[] allConstructors = type.GetConstructors(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if(allConstructors.Length > 0)
            {
                // Choose the first...
                constructorInfo = allConstructors[0];

                // Iterate on the others for [Constructor]...
                foreach (ConstructorInfo info in allConstructors)
                {
                    if (info.GetCustomAttributes(typeof(ConstructorAttribute), true).Length <= 0)
                    {
                        continue;
                    }

                    constructorInfo = info;
                    break;
                }
            }
            
            if(constructorInfo == null)
            {
                return null;
            }
            
            var constructorInfoCache = new ConstructorInfoCache(constructorInfo, GetParameterInfoCache(constructorInfo.GetParameters()));

            return constructorInfoCache;
        }
        
        private ParameterInfoCache[] GetParameterInfoCache(ParameterInfo[] parameters)
        {
            if(parameters == null || parameters.Length == 0)
            {
                return null;
            }

            ParameterInfoCache[] parametersCache = new ParameterInfoCache[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameterInfo = parameters[i];

                object[] injectAttributes = parameterInfo.GetCustomAttributes(typeof(InjectAttribute), true);

                InjectAttribute injectAttribute = null;

                if (injectAttributes.Length > 0)
                {
                    injectAttribute = (InjectAttribute)injectAttributes[0];
                }

                parametersCache[i] = new ParameterInfoCache(parameters[i], injectAttribute);
            }

            return parametersCache;
        }

        private MethodInfoCache FindConstructorMethodRecursive(Type type)
        {
            MethodInfo constructorMethodInfo = null;

            MethodInfo[] allStaticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var info in allStaticMethods)
            {
                if (info.GetCustomAttributes(typeof(ConstructorAttribute), true).Length <= 0)
                {
                    continue;
                }

                // Does it return an instance of itself?
                if (info.ReturnType == type)
                {
                    constructorMethodInfo = info;
                    break;
                }
            }

            if(constructorMethodInfo == null)
            {
                return null;
            }

            ParameterInfo[] methodParameters = constructorMethodInfo.GetParameters();

            object[] injectAttributes = constructorMethodInfo.GetCustomAttributes(typeof(InjectAttribute), true);
            InjectAttribute injectAttribute = null;
            if (injectAttributes.Length > 0)
            {
                injectAttribute = (InjectAttribute)injectAttributes[0];
            }

            ParameterInfoCache[] parameters = GetParameterInfoCache(constructorMethodInfo.GetParameters());

            var constructorMethodInfoCache = new MethodInfoCache(constructorMethodInfo, injectAttribute, parameters);

            return constructorMethodInfoCache;
        }

        protected virtual bool IsRootType(Type type)
        {
            return type == typeof(object);
        }

        protected virtual List<MethodInfoCache> FindMethodsRecursive(Type type, List<MethodInfoCache> methods=null)
        {
            if (methods == null)
            {
                methods = new List<MethodInfoCache>();
            }

            var subMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var info in subMethods)
            {
                if(info.IsSpecialName || !MightHaveInjectAttribute(info))
                {
                    continue;
                }

                object[] injectAttributes = info.GetCustomAttributes(typeof(InjectAttribute), true);
                if(injectAttributes.Length <= 0)
                {
                    continue;
                }

                InjectAttribute injectAttribute = (InjectAttribute)injectAttributes[0];

                MethodInfoCache methodInfoCache = new MethodInfoCache(info, injectAttribute, GetParameterInfoCache(info.GetParameters()));

                methods.Add(methodInfoCache);
            }
            
            if (IsRootType(type.BaseType))
            {
                return methods;
            }

            FindMethodsRecursive(type.BaseType, methods);
            
            return methods;
        }

        protected virtual List<MemberInfoCache> FindFieldsRecursive(Type type)
        {
            List<MemberInfoCache> fields = new List<MemberInfoCache>();

            var subFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var info in subFields)
            {
                var attrs = (InjectAttribute[])info.GetCustomAttributes(typeof(InjectAttribute), true);

                if (attrs.Length <= 0)
                {
                    continue;
                }

                fields.Add(new MemberInfoCache(info, attrs[0]));
            }

            if (IsRootType(type.BaseType))
            {
                return fields;
            }

            // Continue recursively...
            var childFields = FindFieldsRecursive(type.BaseType);

            foreach (var childInfo in childFields)
            {
                if (!fields.Contains(childInfo))
                {
                    fields.Add(childInfo);
                }
            }

            return fields;
        }

        protected virtual List<MemberInfoCache> FindPropertiesRecursive(Type type)
        {
            var properties = new List<MemberInfoCache>();

            const BindingFlags propertyFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var subProperties = type.GetProperties(propertyFlags);

            foreach (var info in subProperties)
            {
                if (!MightHaveInjectAttribute(info))
                {
                    continue;
                }

                var attrs = (InjectAttribute[])info.GetCustomAttributes(typeof(InjectAttribute), true);

                if (attrs.Length <= 0)
                {
                    continue;
                }
                
                var strongInfo = info.DeclaringType.GetProperty(info.Name, propertyFlags);
                properties.Add(new MemberInfoCache(strongInfo, attrs[0]));
            }

            if(IsRootType(type.BaseType))
            {
                return properties;
            }

            // Continue recursively...
            var childProperties = FindPropertiesRecursive(type.BaseType);

            foreach (var childInfo in childProperties)
            {
                if (!properties.Contains(childInfo))
                {
                    properties.Add(childInfo);
                }
            }

            return properties;
        }
        
        /// <summary>
        /// Returns true if MemberInfo might have CustomAttribute applied to it
        /// </summary>
        /// <param name="memberInfo">MemberInfo</param>
        /// <returns>True if MemberInfo can potentially have InjectAttribute applied to it</returns>
        protected virtual bool MightHaveInjectAttribute(MemberInfo memberInfo)
        {
            var declaringType = memberInfo.DeclaringType;

            return declaringType != typeof(object);
        }
    }
}