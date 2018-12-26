using AOFL.KrakenIoc.Core.V1.Interfaces;
using UnityEngine;

namespace AOFL.KrakenIoc.Extensions.V1
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Adds a component and injects via the container.
        /// </summary>
        /// <returns>The component.</returns>
        /// <param name="gameObject">Game object.</param>
        /// <param name="container">Container.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T AddComponent<T>(this GameObject gameObject, IContainer container)
        {
            return container.Resolve<T>(gameObject);
        }
    }
}