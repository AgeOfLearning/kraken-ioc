using System;
using System.CodeDom.Compiler;
using System.Reflection;

namespace AOFL.KrakenIoc.Extensions.V1
{
    public static class ActionExtensions
    {
        /// <summary>
        /// Determines whether this Action is an anonymous method
        /// or not.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool IsAnonymousMethod(this Action action)
        {
            return IsAnonymousMethod(action.Method);
        }

        /// <summary>
        /// Determines whether this Action is an anonymous method
        /// or not.
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="T1"></typeparam>
        /// <returns></returns>
        public static bool IsAnonymousMethod<T1>(this Action<T1> action)
        {
            return IsAnonymousMethod(action.Method);
        }

        /// <summary>
        /// Determines whether this Action is an anonymous method
        /// or not.
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static bool IsAnonymousMethod<T1, T2>(this Action<T1, T2> action)
        {
            return IsAnonymousMethod(action.Method);
        }

        /// <summary>
        /// Determines whether this Action is an anonymous method
        /// or not.
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public static bool IsAnonymousMethod<T1, T2, T3>(this Action<T1, T2, T3> action)
        {
            return IsAnonymousMethod(action.Method);
        }

        private static bool IsAnonymousMethod(MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == null || !CodeGenerator.IsValidLanguageIndependentIdentifier(methodInfo.N‌​ame);
        }
    }
}