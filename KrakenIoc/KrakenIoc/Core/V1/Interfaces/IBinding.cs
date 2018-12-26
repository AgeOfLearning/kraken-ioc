using System;
using System.Collections.Generic;

namespace AOFL.KrakenIoc.Core.V1.Interfaces
{
    public delegate T FactoryMethod<T>(IInjectContext injectionContext); 

    /// <summary>
    /// Represents a binding between an interface
    /// and implementation type.
    /// </summary>
    public partial interface IBinding
    {
        event Action<bool, IBinding, object> Resolved;

        IContainer Container { get; set; }

        IBinding To<T>();

        object Resolve(object target = null);

        T Resolve<T>(object target = null);

        object Resolve(IInjectContext parentContext, object target = null);

        /// <summary>
        /// The lifetime-scope of the binding.
        /// </summary>
        BindingType BindingType { get; set; }
        /// <summary>
        /// The concrete type that is bound.
        /// </summary>
        Type BoundType { get; set; }
        /// <summary>
        /// The interface that is binding an implementation.
        /// </summary>
        Type BinderType { get; set; }

        /// <summary>
        /// Category of the binding
        /// </summary>
        object Category { get; set; }

        /// <summary>
        /// List of bound objects
        /// </summary>
        List<object> BoundObjects { get; set; }

        /// <summary>
        /// Factory type - set when FromFactory is used
        /// </summary>
        Type FactoryType { get; set; }

        /// <summary>
        /// Factory method - set when FromFactoryMethod is used
        /// </summary>
        FactoryMethod<object> FactoryMethod { get; set; }


        void Dissolve();

        /// <summary>
        /// Binds type on a category
        /// </summary>
        /// <param name="category"></param>
        IBinding WithCategory(object category);

        /// <summary>
        /// T will be resolved using Factory class
        /// </summary>
        /// <typeparam name="TFactory">Factory</typeparam>
        /// <typeparam name="T">Type being resolved</typeparam>
        /// <returns></returns>
        IBinding FromFactory<TFactory, T>() where TFactory : IFactory<T>;

        /// <summary>
        /// T will be resolved using Factory Method
        /// </summary>
        /// <typeparam name="T">Type being resolved</typeparam>
        /// <param name="factoryMethod">Factory method</param>
        /// <returns></returns>
        IBinding FromFactoryMethod<T>(FactoryMethod<T> factoryMethod);

        /// <summary>
        /// Maps a single-instance binding.
        /// </summary>
        void AsSingleton();
        /// <summary>
        /// Maps a multiple-instance binding.
        /// </summary>
        void AsTransient();

        /// <summary>
        /// Treats this binding as a proxy to the inherited one.
        /// </summary>
        /// <param name="binding"></param>
        void Inherit(IBinding binding);

        /// <summary>
        /// Triggers the event for a resolve event.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="value"></param>
        void NotifyResolved(bool success, object value);
    }
}
