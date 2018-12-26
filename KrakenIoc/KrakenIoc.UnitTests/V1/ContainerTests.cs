using System;
using AOFL.KrakenIoc.Core.V1;
using AOFL.KrakenIoc.Core.V1.Interfaces;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AOFL.KrakenIoc.UnitTests.V1
{
    public class ContainerTests
    {
        #region Test 7 - DoesResolveStaticMethodMonoBehaviour

        public interface ISomeComponent
        {
            GameObject gameObject { get; }
        }

        public class SomeComponent : MonoBehaviour, ISomeComponent
        {
            [Constructor]
            private static SomeComponent Create(GameObject gameObject)
            {
                return gameObject.AddComponent<SomeComponent>();
            }
        }

        [Test]
        public void DoesResolveStaticMethodMonoBehaviour()
        {
            Container container = new Container();
            container.Bind<ISomeComponent>().To<SomeComponent>();

            ISomeComponent seven = container.Resolve<ISomeComponent>();

            Assert.NotNull(seven);

            Object.DestroyImmediate(seven.gameObject);

            container = null;
        }

        #endregion

        #region Test 8 - DoesResolveMonoBehaviour

        public class SomeComponent2 : MonoBehaviour, ISomeComponent
        {

        }

        [Test]
        public void DoesResolveMonoBehaviour()
        {
            Container container = new Container();
            container.Bind<ISomeComponent>().To<SomeComponent2>();

            ISomeComponent eight = container.Resolve<ISomeComponent>();

            Assert.NotNull(eight);

            Object.DestroyImmediate(eight.gameObject);

            container = null;
        }

        #endregion
    }
}
