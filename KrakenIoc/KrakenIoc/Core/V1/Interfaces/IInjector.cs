using System;

namespace AOFL.KrakenIoc.Core.V1.Interfaces
{
    public delegate void LogHandler(string format, params object[] args);

    /// <summary>
    /// An injector is responsible for resolving and 
    /// mitigating recursive injection issues.
    /// </summary>
    public interface IInjector
    {
        LogHandler LogHandler { get; set; }

        object Resolve(Type type);

        object Resolve(Type type, IInjectContext injectContext);

        object Resolve(Type type, object target);

        object Resolve(Type type, object target, IInjectContext injectContext);

        /// <summary>
        /// Injects values into the object.
        /// </summary>
        /// <param name="objValue"></param>
        void Inject(object objValue);

        /// <summary>
        /// Injects values into the object
        /// </summary>
        /// <param name="objValue"></param>
        /// <param name="injectContext"></param>
        void Inject(object objValue, IInjectContext injectContext);
    }
}