using System;

namespace AOFL.KrakenIoc.Core.V1.Interfaces
{
    public interface IInjectContext
    {
        /// <summary>
        /// Container being used for an injection
        /// </summary>
        IContainer Container { get; }

        /// <summary>
        /// Type that 
        /// </summary>
        Type DeclaringType { get; }

        /// <summary>
        /// Parent context
        /// </summary>
        IInjectContext ParentContext { get; }
    }
}