using System;
using AOFL.KrakenIoc.Core.V1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AOFL.KrakenIoc.Testing.V1
{
    #region Data
    public class NoConstructorTestClass { }
    public class PrivateConstructorTestClass
    {
        private PrivateConstructorTestClass() { }
    }
    public class MultipleConstructorTestClass
    {
         public MultipleConstructorTestClass() { }
         public MultipleConstructorTestClass(string arg) { }
    }
    public class ClassUsingNoConstructorTestClass
    {
        public ClassUsingNoConstructorTestClass(NoConstructorTestClass noConstructorTestClass)
        {

        }
    }
    #endregion

    [TestClass]
    public class InjectorTests
    {
        [TestMethod]
        public void Injector_DoesNotThrowException_WhenNoConstructor()
        {
            var container = new Container();
            container.Bind<NoConstructorTestClass>().To<NoConstructorTestClass>();
            container.Injector = new Injector(container);

            var result = container.Resolve<NoConstructorTestClass>();
            Assert.IsNotNull(result);

            container.Injector.Inject(result);
        }

        [TestMethod]
        public void Injector_DoesNotThrowException_WhenPrivateConstructor()
        {
            var container = new Container();
            container.Bind<PrivateConstructorTestClass>().To<PrivateConstructorTestClass>();
            container.Injector = new Injector(container);

            var result = container.Resolve<PrivateConstructorTestClass>();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Injector_DoesNotThrowException_WhenMultipleConstructors()
        {
            var container = new Container();
            container.Bind<MultipleConstructorTestClass>().To<MultipleConstructorTestClass>();
            container.Injector = new Injector(container);

            var result = container.Resolve<MultipleConstructorTestClass>();
            Assert.IsNotNull(result);
        }
    }
}
