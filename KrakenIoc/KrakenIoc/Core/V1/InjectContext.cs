using System;
using System.Collections.Generic;
using AOFL.KrakenIoc.Core.V1.Interfaces;

namespace AOFL.KrakenIoc.Core
{
    /// <summary>
    /// Represents additional data passed when injecting into property, field, method or constructor
    /// </summary>
    public class InjectContext : IInjectContext
    {
        /// <summary>
        /// Container being used for an injection
        /// </summary>
        public IContainer Container { get; set; }
        
        /// <summary>
        /// Type that is declaring injected field, property or method
        /// </summary>
        public Type DeclaringType { get; set; }

        /// <summary>
        /// Parent context
        /// </summary>
        public IInjectContext ParentContext { get; set; }

        public InjectContext(IContainer container, Type declaringType, IInjectContext parentContext)
        {
            Container = container;
            DeclaringType = declaringType;
            ParentContext = parentContext;
        }       

        public InjectContext(IContainer container, Type declaringType)
            : this(container, declaringType, null)
        {
        }
    }
}
