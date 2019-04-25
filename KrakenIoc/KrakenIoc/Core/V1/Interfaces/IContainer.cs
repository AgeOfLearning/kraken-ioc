using System;
using System.Collections.Generic;

namespace AOFL.KrakenIoc.Core.V1.Interfaces
{
    /// <summary>
    /// Contains the IOC bindings that can be
    /// resolved and injected.
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// The injector that this container uses.
        /// </summary>
        IInjector Injector { get; set; }
        bool ShouldLog { get; set; }
        LogHandler LogHandler { get; set; }

        /// <summary>
        /// Checks if binding exists for a type
        /// </summary>
        /// <typeparam name="T">Binder type</typeparam>
        /// <returns>True if binding exists, otherwise false</returns>
        bool HasBindingFor<T>();

        /// <summary>
        /// Checks if binding exists for a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True if binding exists, otherwise false</returns>
        bool HasBindingFor(Type type);

        /// <summary>
        /// Checks if binding exists for a type and a category
        /// </summary>
        /// <typeparam name="T">Type</typeparam>>
        /// <param name="category">Category</param>
        bool HasBindingForCategory<T>(object category);

        /// <summary>
        /// Checks if binding exists for a type and a category
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="category">Category</param>
        /// <returns></returns>
        bool HasBindingForCategory(Type type, object category);

        /// <summary>
        /// Executes the specfied boostrap, thus allowing
        /// container bindings externally.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Bootstrap<T>() where T : IBootstrap;
        void Bootstrap(Type type);

        /// <summary>
        /// Creates a binding for the interface type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category">Category to bind against.</param>
        /// <returns>IBinding</returns>
        IBinding Bind<T>();

        /// <summary>
        /// Creates a binding for the interface type
        /// </summary>
        /// <typeparam name="TInterface">Interface type</typeparam>
        /// <typeparam name="TImplementation">Implementation type</typeparam>
        /// <returns>Binding</returns>
        IBinding Bind<TInterface, TImplementation>() where TImplementation : TInterface;

        /// <summary>
        /// Creates a binding for the interface type
        /// with the category.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <returns></returns>
        [Obsolete("Do not use Bind<T>(category), instead do Bind<T>().WithCategory(category).")]
        IBinding Bind<T>(object category);

        /// <summary>
        /// Creates a binding for the interface type with a specific value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        IBinding Bind<T>(T value);
        
        /// <summary>
        /// Binds the interface type T against the implementation
        /// Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        IBinding Bind<T>(Type type);

        /// <summary>
        /// Binds multiple interface types against the implementation
        /// Type.
        /// </summary>
        /// <param name="interfaceTypes">Interface types</param>
        /// <returns></returns>
        IBinding Bind(params Type[] interfaceTypes);

        /// <summary>
        /// Returns an instance of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(Type type);

        /// <summary>
        /// Returns an instance of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parentContext"></param>
        /// <returns></returns>
        object Resolve(Type type, IInjectContext parentContext);

        /// <summary>
        /// Adds a component of specificed type and optional category.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="gameObject"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        object Resolve(Type type, object gameObject);

        /// <summary>
        /// Returns an instance of the specified type and optional category.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <returns></returns>
        T Resolve<T>();

        /// <summary>
        /// Adds a component of specified type and optional category.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        T Resolve<T>(object target);

        /// <summary>
        /// Adds a component of specified type and optional category.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        [Obsolete("Do not use Resolve(category), instead use ResolveWithCategory(category)")]
        T Resolve<T>(string category);

        /// <summary>
        /// Returns an instance of the specified type and optional category.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        object ResolveWithCategory(Type type, object category);

        /// <summary>
        /// Returns an instance of the specified type and optional category.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="parentContext"></param>
        /// <returns></returns>
        object ResolveWithCategory(Type type, object category, IInjectContext parentContext);

        /// <summary>
        /// Returns an instance of the specified type and optional category.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <returns></returns>
        T ResolveWithCategory<T>(object category);

        /// <summary>
        /// Adds a component of specificed type and optional category.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        object ResolveWithCategory(Type type, object target, object category);

        /// <summary>
        /// Adds a component of specificed type and optional category.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <param name="category"></param>
        /// <param name="parentContext"></param>
        /// <returns></returns>
        object ResolveWithCategory(Type type, object target, object category, IInjectContext parentContext);

        /// <summary>
        /// Returns an instance of the specified type and optional category.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        T ResolveWithCategory<T>(object target, object category);

        /// <summary>
        /// Inherits the bindings from an existing container.
        /// </summary>
        /// <param name="container"></param>
        void Inherit(IContainer container);

        IBinding GetBinding<T>();
        IBinding GetBinding<T>(object category);
        IBinding GetBinding(Type type);
        IBinding GetBinding(Type type, object category);

        List<IBinding> GetBindings();
        List<Type> GetBindedTypes();

        /// <summary>
        /// Unbinds and dissolves the bindings.
        /// </summary>
        /// <param name="type">Type.</param>
        void Dissolve(Type type);
        void Dissolve<T>();
    }
}
