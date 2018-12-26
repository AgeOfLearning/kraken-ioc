using System;
using System.Reflection;

namespace AOFL.KrakenIoc.Core.V1
{
    /// <summary>
    /// Internally cached representation of the constructor, it's inject attribute and constructor parameters
    /// </summary>
    public class MethodInfoCache
    {
        public MethodInfo MethodInfo { get; set; }
        public InjectAttribute InjectAttribute { get; set; }
        public ParameterInfoCache[] Parameters { get; set; }
        public Type DeclaringType { get; set; }

        public MethodInfoCache(MethodInfo methodInfo, InjectAttribute injectAttribute, ParameterInfoCache[] parameters)
        {
            MethodInfo = methodInfo;
            InjectAttribute = injectAttribute;
            Parameters = parameters;
            DeclaringType = methodInfo.DeclaringType;
        }
    }
}
