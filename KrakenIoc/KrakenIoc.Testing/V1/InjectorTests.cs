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
    
    internal interface ISomeInjectedType { }

    internal class SomeInjectedType : ISomeInjectedType { }

    internal interface ISomeClientType
    {
        ISomeInjectedType InjectedInstance { get; }
    }

    internal class SomeClientTypeBase : ISomeClientType
    {
        [Inject] public ISomeInjectedType InjectedInstance { get; private set; }
    }

    internal class SomeClientTypeDerived : SomeClientTypeBase { }

    internal class SomeOtherClientTypeBase : ISomeClientType
    {
        [Inject] private ISomeInjectedType _injectedInstance { get; set; }

        public ISomeInjectedType InjectedInstance
        {
            get { return _injectedInstance; }
        }
    }

    internal class SomeOtherClientTypeDerived : SomeOtherClientTypeBase { }

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

        [TestMethod]
        public void Injector_DoesNotThrowException_WhenPublicPropertyInjectedOnBaseType()
        {
            var container = new Container();
            container.Bind<ISomeInjectedType>().To<SomeInjectedType>().AsSingleton();
            ISomeClientType client = new SomeClientTypeBase();

            var injector = new Injector(container);
            injector.Inject(client);

            Assert.IsNotNull(client.InjectedInstance, "Injected public property (from base type) is null on base type instance.");
        }
        
        [TestMethod]
        public void Injector_DoesNotThrowException_WhenPublicPropertyInjectedOnDerivedType()
        {
            var container = new Container();
            container.Bind<ISomeInjectedType>().To<SomeInjectedType>().AsSingleton();
            ISomeClientType client = new SomeClientTypeDerived();

            var injector = new Injector(container);
            injector.Inject(client);
            
            Assert.IsNotNull(client.InjectedInstance, "Injected public property (from base type) is null on derived type instance.");
        }
        
        [TestMethod]
        public void Injector_DoesNotThrowException_WhenPrivatePropertyInjectedOnBaseType()
        {
            var container = new Container();
            container.Bind<ISomeInjectedType>().To<SomeInjectedType>().AsSingleton();
            ISomeClientType client = new SomeOtherClientTypeBase();

            var injector = new Injector(container);
            injector.Inject(client);

            Assert.IsNotNull(client.InjectedInstance, "Injected private property (from base type) is null on base type instance.");
        }

        [TestMethod]
        public void Injector_DoesNotThrowException_WhenPrivatePropertyInjectedOnDerivedType()
        {
            var container = new Container();
            container.Bind<ISomeInjectedType>().To<SomeInjectedType>().AsSingleton();
            ISomeClientType client = new SomeOtherClientTypeDerived();

            var injector = new Injector(container);
            injector.Inject(client);

            Assert.IsNotNull(client.InjectedInstance, "Injected private property (from base type) is null on derived type instance.");
        }
    }
}
