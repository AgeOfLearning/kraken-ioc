using System;
using System.Reflection;

namespace AOFL.KrakenIoc.Core.V1
{
    /// <summary>
    /// Internally cached representation of the constructor, it's inject attribute and constructor parameters
    /// </summary>
    public class ConstructorInfoCache
    {
        public ConstructorInfo ConstructorInfo { get; set; }
        public ParameterInfoCache[] Parameters { get; set; }
        public Type DeclaringType { get; set; }

        public ConstructorInfoCache(ConstructorInfo constructorInfo, ParameterInfoCache[] parameters)
        {
            ConstructorInfo = constructorInfo;
            Parameters = parameters;
            DeclaringType = constructorInfo.DeclaringType;
        }
    }
}
