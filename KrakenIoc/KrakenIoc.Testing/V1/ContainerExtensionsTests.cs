using System;
using NUnit.Framework;
using AOFL.KrakenIoc.Core.V1.Interfaces;
using AOFL.KrakenIoc.Core.V1;
using AOFL.KrakenIoc.Core.V1.Exceptions;
using AOFL.KrakenIoc.Extensions.V1;

namespace AOFL.KrakenIoc.Testing
{
    [Parallelizable]
    public class ContainerExtensionsTests
    {
        private interface ISomeInterface
        {
            public int Field { get; }
        }
        private class SomeClass : ISomeInterface
        {
            public int Field { get; private set; }

            public SomeClass(int field)
            {
                Field = field;
            }
        }

        [TestCase(73)]
        [TestCase(-204)]
        [TestCase(0)]
        public void BindSingletonCompilesWithFactoryMethodAfterwards(int fieldValue)
        {
            Container container = new Container();
            IBinding binding = container.BindSingleton<ISomeInterface, SomeClass>().FromFactoryMethod(
                (IInjectContext ctx) => new SomeClass(fieldValue)
            );

            Assert.AreEqual(BindingType.Singleton, binding.BindingType);

            ISomeInterface service = container.Resolve<ISomeInterface>();
            Assert.AreEqual(fieldValue, service.Field);
        }

        [TestCase(10, 20, 30, 40, 50, 60)]
        [TestCase(-105, 4958, 0, 322245, -49385)]
        public void BindTransientCompilesWithFactoryMethodAfterwards(params int[] fields)
        {
            int count = fields.Length;
            int index = 0;

            Container container = new Container();
            IBinding binding = container.BindTransient<ISomeInterface, SomeClass>().FromFactoryMethod(
                (IInjectContext ctx) => new SomeClass(fields[index++])
            );

            Assert.AreEqual(BindingType.Transient, binding.BindingType);

            for (int i = 0; i < count; i++)
            {
                ISomeInterface service = container.Resolve<ISomeInterface>();
                Console.WriteLine("service (" + i + "/" + count + ") has field = " + service.Field);
                Assert.AreEqual(fields[i], service.Field);
            }
        }
    }
}
