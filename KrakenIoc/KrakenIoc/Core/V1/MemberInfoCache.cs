using System;
using System.Reflection;

namespace AOFL.KrakenIoc.Core.V1
{
    /// <summary>
    /// Internally cached representation of the member and inject attribute.
    /// </summary>
    public struct MemberInfoCache
    {
        public MemberInfo MemberInfo { get; set; }
        public InjectAttribute InjectAttribute { get; set; }
        public Type DeclaringType { get; set; }

        public MemberInfoCache(MemberInfo memberInfo, InjectAttribute injectAttribute)
        {
            MemberInfo = memberInfo;
            InjectAttribute = injectAttribute;
            DeclaringType = memberInfo.DeclaringType;
        }
    }
}