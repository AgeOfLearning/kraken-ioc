using System;
using AOFL.KrakenIoc.Core.V1.Interfaces;

namespace AOFL.KrakenIoc.Extensions.V1
{
    public static class ContainerExtensions
    {
        public static IBinding BindSingleton<T>(this IContainer container)
        {
            IBinding binding = container.Bind<T>();
            binding.AsSingleton();
            return binding;
        }

        public static IBinding BindSingleton<TInterface, TImplementation>(this IContainer container) where TImplementation : TInterface
        {
            IBinding binding = container.Bind<TInterface, TImplementation>();
            binding.AsSingleton();
            return binding;
        }

        public static IBinding BindSingleton<T>(this IContainer container, T value)
        {
            IBinding binding = container.Bind<T>(value);
            binding.AsSingleton();
            return binding;
        }

        public static IBinding BindSingleton<T>(this IContainer container, Type type)
        {
            IBinding binding = container.Bind<T>(type);
            binding.AsSingleton();
            return binding;
        }

        public static IBinding BindTransient<T>(this IContainer container)
        {
            IBinding binding = container.Bind<T>();
            binding.AsTransient();
            return binding;
        }

        public static IBinding BindTransient<TInterface, TImplementation>(this IContainer container) where TImplementation : TInterface
        {
            IBinding binding = container.Bind<TInterface, TImplementation>();
            binding.AsTransient();
            return binding;
        }

        public static IBinding BindTransient<T>(this IContainer container, T value)
        {
            IBinding binding = container.Bind<T>(value);
            binding.AsTransient();
            return binding;
        }

        public static IBinding BindTransient<T>(this IContainer container, Type type)
        {
            IBinding binding = container.Bind<T>(type);
            binding.AsTransient();
            return binding;
        }
    }
}
