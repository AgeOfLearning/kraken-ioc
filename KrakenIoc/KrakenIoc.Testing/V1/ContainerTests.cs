using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AOFL.KrakenIoc.Core.V1.Interfaces;
using AOFL.KrakenIoc.Core.V1;
using UnityEngine;
using System.Threading.Tasks;
using AOFL.KrakenIoc.Core.V1.Exceptions;
using System.Linq;

namespace AOFL.KrakenIoc.Testing
{
    [TestClass]
    public class ContainerTests
    {
        #region Test 1 - DoesResolveType

        #region Interface & Implementation
        internal interface ISomeTypeOne
        {
            ISomeTypeTwo Two { get; set; }
        }

        internal interface ISomeTypeTwo
        {
            IContainer Container { get; }
        }

        internal class SomeTypeOne : ISomeTypeOne
        {
            public ISomeTypeTwo Two { get; set; }

            SomeTypeOne(ISomeTypeTwo two)
            {
                Two = two;
            }
        }

        internal class SomeTypeTwo : ISomeTypeTwo
        {
            public IContainer Container { get; set; }

            SomeTypeTwo(IContainer container)
            {
                Container = container;
            }
        }

        internal class AnotherTypeTwo : ISomeTypeTwo
        {
            public IContainer Container { get; set; }

            AnotherTypeTwo(IContainer container)
            {
                Container = container;
            }
        }

        internal class SomeTypeOneCat : ISomeTypeOne
        {
            [Inject("my-category")] public ISomeTypeTwo Two { get; set; }
        }

        internal class SomeTypeOneCatObject : ISomeTypeOne
        {
            public enum Category { CategoryA, CategoryB };

            [Inject(Category.CategoryA)] public ISomeTypeTwo Two { get; set; }
        }

        internal class SomeTypeOneCatPrimitive : ISomeTypeOne
        {
            public const int Category = 123;

            [Inject(Category)] public ISomeTypeTwo Two { get; set; }
        }

        internal interface ISomeTypeSeven { }

        internal class SomeTypeSevenA : ISomeTypeSeven { }

        internal class SomeTypeSevenB : ISomeTypeSeven { }

        #endregion

        [TestMethod]
        public void DoesResolveType()
        {
            Container container = new Container();

            // Transient bindings...
            container.Bind<ISomeTypeOne>().To<SomeTypeOne>();
            container.Bind<ISomeTypeTwo>().To<SomeTypeTwo>();

            // Resolve one... should be injected
            ISomeTypeOne one = container.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one);
            Assert.IsNotNull(one.Two);

            // Ensure we can inject additional "Extra" dependencies...
            Assert.IsNotNull(one.Two.Container);

            container = null;
        }

        [TestMethod]
        public void DoesResolveTypeWhenGenericBindUsed()
        {
            Container container = new Container();

            // Transient bindings...
            container.Bind<ISomeTypeOne, SomeTypeOne>();
            container.Bind<ISomeTypeTwo, SomeTypeTwo>();

            // Resolve one... should be injected
            ISomeTypeOne one = container.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one);
            Assert.IsNotNull(one.Two);

            // Ensure we can inject additional "Extra" dependencies...
            Assert.IsNotNull(one.Two.Container);

            container = null;
        }

        [TestMethod]
        public void DoesResolveTypeWithCategory()
        {
            Container container = new Container();

            // Transient bindings...
            container.Bind<ISomeTypeOne>().To<SomeTypeOneCat>();
            container.Bind<ISomeTypeTwo>().WithCategory("my-category").To<SomeTypeTwo>();

            // Resolve one... should be injected
            ISomeTypeOne one = container.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one);
            Assert.IsNotNull(one.Two);

            // Ensure we can inject additional "Extra" dependencies...
            Assert.IsNotNull(one.Two.Container);

            container = null;
        }

        [TestMethod]
        public void DoesResolveTypeWithCategoryUsingWithCategory()
        {
            Container container = new Container();

            container.Bind<ISomeTypeOne>().To<SomeTypeOneCat>();
            container.Bind<ISomeTypeTwo>().WithCategory("my-category").To<SomeTypeTwo>();

            // Resolve one... should be injected
            ISomeTypeOne one = container.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one);
            Assert.IsNotNull(one.Two);

            // Ensure we can inject additional "Extra" dependencies...
            Assert.IsNotNull(one.Two.Container);

            container = null;
        }

        [TestMethod]
        public void DoesResolveTypeWithObjectCategory()
        {
            Container container = new Container();
            
            container.Bind<ISomeTypeOne>().To<SomeTypeOneCatObject>();
            container.Bind<ISomeTypeTwo>().WithCategory(SomeTypeOneCatObject.Category.CategoryA).To<SomeTypeTwo>();

            // Resolve one... should be injected
            ISomeTypeOne one = container.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one);
            Assert.IsNotNull(one.Two);

            // Ensure we can inject additional "Extra" dependencies...
            Assert.IsNotNull(one.Two.Container);

            container = null;
        }

        [TestMethod]
        public void DoesResolveTypeWithObjectCategoryUsingPrimitiveInjectAttribute()
        {
            Container container = new Container();

            container.Bind<ISomeTypeOne>().To<SomeTypeOneCatPrimitive>();
            container.Bind<ISomeTypeTwo>().WithCategory(SomeTypeOneCatPrimitive.Category).To<SomeTypeTwo>();

            // Resolve one... should be injected
            ISomeTypeOne one = container.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one);
            Assert.IsNotNull(one.Two);

            // Ensure we can inject additional "Extra" dependencies...
            Assert.IsNotNull(one.Two.Container);

            container = null;
        }

        [TestMethod]
        public void DoesResolveTypeWithObjectCategoryUsingResolveWithCategory()
        {
            Container container = new Container();

            byte category = 123;

            container.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().WithCategory(category);
            
            ISomeTypeTwo two = container.ResolveWithCategory<ISomeTypeTwo>(category);

            Assert.IsNotNull(two);
            
            container = null;
        }

        [TestMethod]
        public void DoesResolveTypeWhenCategoryIsSetAfterwards()
        {
            Container container = new Container();
            
            object category = new object();

            IBinding binding = container.Bind<ISomeTypeSeven>().To<SomeTypeSevenA>();

            ISomeTypeSeven sevenA = container.Resolve<ISomeTypeSeven>();
            Assert.IsInstanceOfType(sevenA, typeof(SomeTypeSevenA));

            binding.WithCategory(category);
            
            ISomeTypeSeven sevenB = container.ResolveWithCategory<ISomeTypeSeven>(category);
            Assert.IsInstanceOfType(sevenB, typeof(SomeTypeSevenA));
        }

        [TestMethod]
        [ExpectedException(typeof(MissingBindingException))]
        public void DoesNotResolveTypeWithWhenCategoryIsSetAfterwards()
        {
            Container container = new Container();

            object category = new object();

            IBinding binding = container.Bind<ISomeTypeSeven>().To<SomeTypeSevenA>();

            ISomeTypeSeven sevenA = container.Resolve<ISomeTypeSeven>();
            Assert.IsInstanceOfType(sevenA, typeof(SomeTypeSevenA));

            binding.WithCategory(category);

            ISomeTypeSeven sevenB = container.Resolve<ISomeTypeSeven>();
        }

        [TestMethod]
        [ExpectedException(typeof(TypeCategoryAlreadyBoundException))]
        public void ThrowsExceptionWhenCategoryIsAlreadySet()
        {
            Container container = new Container();

            object categoryA = new object();
            object categoryB = new object();
 
            IBinding binding = container.Bind<ISomeTypeSeven>().To<SomeTypeSevenA>().WithCategory(categoryA);

            ISomeTypeSeven sevenA = container.ResolveWithCategory<ISomeTypeSeven>(categoryA);
            Assert.IsInstanceOfType(sevenA, typeof(SomeTypeSevenA));

            binding.WithCategory(categoryB);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeCategoryAlreadyBoundException))]
        public void ThrowsExceptionWhenCategoryIsAlreadySet2()
        {
            Container container = new Container();

            object categoryA = new object();
            object categoryB = new object();

            IBinding binding = container.Bind<ISomeTypeSeven>().To<SomeTypeSevenA>().WithCategory(categoryA);

            ISomeTypeSeven sevenA = container.ResolveWithCategory<ISomeTypeSeven>(categoryA);
            Assert.IsInstanceOfType(sevenA, typeof(SomeTypeSevenA));

            binding.Category = categoryB;
        }

        [TestMethod]
        [ExpectedException(typeof(TypeCategoryAlreadyBoundException))]
        public void ThrowsExceptionWhenCategoryIsRemoved()
        {
            Container container = new Container();

            object categoryA = new object();

            IBinding binding = container.Bind<ISomeTypeSeven>().To<SomeTypeSevenA>().WithCategory(categoryA);

            ISomeTypeSeven sevenA = container.ResolveWithCategory<ISomeTypeSeven>(categoryA);
            Assert.IsInstanceOfType(sevenA, typeof(SomeTypeSevenA));

            binding.Category = null;
        }

        #endregion

        #region Test 2 - DoesResolveSingleton

        [TestMethod]
        public void DoesResolveSingleton()
        {
            Container container = new Container();

            // Transient & Singleton binding...
            container.Bind<ISomeTypeOne>().To<SomeTypeOne>();
            container.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();

            // Resolve one... should be injected
            ISomeTypeOne one = container.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one);
            Assert.IsNotNull(one.Two);

            // Resolve singleton
            ISomeTypeTwo twoTest2 = container.Resolve<ISomeTypeTwo>();

            Assert.AreEqual(one.Two, twoTest2);

            container = null;
        }
        #endregion

        #region Test 3 - DoesNotAllowConstructorCircularDependency

        #region Interface & Implementations
        internal interface ISomeTypeThree { }
        internal interface ISomeTypeFour { }

        internal class SomeTypeThree : ISomeTypeThree
        {
            SomeTypeThree(ISomeTypeFour four)
            {

            }
        }

        internal class SomeTypeFour : ISomeTypeFour
        {
            SomeTypeFour(ISomeTypeThree three)
            {

            }
        }
        #endregion

        [TestMethod]
        public void DoesNotAllowConstructorCircularDependency()
        {
            Container container = new Container();

            // Transient & Singleton binding...
            container.Bind<ISomeTypeThree>().To<SomeTypeThree>();
            container.Bind<ISomeTypeFour>().To<SomeTypeFour>();

            // Resolve three... should be injected, but not crash due to circular dependency
            ISomeTypeThree three = container.Resolve<ISomeTypeThree>();

            Assert.IsNotNull(three);

            container = null;
        }
        #endregion

        #region Test 4 - DoesNotAllowPropertyInjectedCircularDependency

        #region Interfaces & Implementation

        interface ISomeTypeFive
        {
            ISomeTypeSix Six { get; set; }
        }

        interface ISomeTypeSix
        {
            ISomeTypeFive Five { get; set; }
        }

        class SomeTypeFive : ISomeTypeFive
        {
            [Inject] public ISomeTypeSix Six { get; set; }
        }

        class SomeTypeSix : ISomeTypeSix
        {
            [Inject] public ISomeTypeFive Five { get; set; }
        }

        #endregion

        [TestMethod]
        public void DoesNotAllowPropertyInjectedCircularDependency()
        {
            Container container = new Container();

            // Transient binding...
            container.Bind<ISomeTypeFive>().To<SomeTypeFive>();
            container.Bind<ISomeTypeSix>().To<SomeTypeSix>();

            // Resolve three... should be injected, but not crash due to circular dependency
            ISomeTypeFive five = container.Resolve<ISomeTypeFive>();

            Assert.IsNotNull(five);
            Assert.IsNotNull(five.Six);
            Assert.IsNull(five.Six.Five);

            container = null;
        }
        #endregion

        #region Test 5 - DoesNotLeakMemory

        //[TestMethod]
        public void DoesNotLeakMemory()
        {
            GC.Collect();

            long newMemory = 0;
            long originalMemory = System.GC.GetTotalMemory(false);

            // Check for binding memory leaks...
            Container container = new Container();
            container.Bind<ISomeTypeFive>().To<SomeTypeFive>();
            container.Bind<ISomeTypeSix>().To<SomeTypeSix>();
            container.Dispose();
            container = null;

            newMemory = System.GC.GetTotalMemory(true);
            Assert.IsTrue(newMemory <= originalMemory);

            // Check for binding & resolving memory leaks...
            container = new Container();
            container.ShouldLog = false;
            container.Bind<ISomeTypeFive>().To<SomeTypeFive>();
            container.Bind<ISomeTypeSix>().To<SomeTypeSix>();

            // Resolve once since we cache reflection info...
            container.Resolve<ISomeTypeFive>();

            newMemory = 0;
            originalMemory = System.GC.GetTotalMemory(true);

            for (int i = 0; i < 10000; i++)
            {
                container.Resolve<ISomeTypeFive>();
            }

            newMemory = System.GC.GetTotalMemory(true);

            long memDifference = newMemory - originalMemory;

            Assert.IsTrue(memDifference <= 0);
        }
        #endregion

        #region Test 6 - DoesAllowCircularDependencyForSingletonPropertyInjection

        [TestMethod]
        public void DoesAllowCircularDependencyForSingletonPropertyInjection()
        {
            Container container = new Container();

            // Singleton binding...
            container.Bind<ISomeTypeFive>().To<SomeTypeFive>().AsSingleton();
            container.Bind<ISomeTypeSix>().To<SomeTypeSix>().AsSingleton();

            // Resolve five...
            ISomeTypeFive five = container.Resolve<ISomeTypeFive>();

            Assert.IsNotNull(five);
            Assert.IsNotNull(five.Six);
            Assert.AreEqual(five, five.Six.Five);

            container = null;
        }

        #endregion

        #region Test 7 - DoesResolveInheritedBindings

        #region Interface & Implementation

        public interface ISomeTypeEight { }

        public interface ISomeTypeEightExtended : ISomeTypeEight { }

        public class SomeTypeEightExtended: ISomeTypeEightExtended { }


        #endregion

        [TestMethod]
        public void DoesResolveFromInheritedContainer()
        {
            Container containerA = new Container();
            Container containerB = new Container();

            containerA.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();
            containerB.Inherit(containerA);
            containerB.Bind<ISomeTypeOne>().To<SomeTypeOne>().AsSingleton();

            ISomeTypeOne one = containerB.Resolve<ISomeTypeOne>();
            ISomeTypeTwo two = containerA.Resolve<ISomeTypeTwo>();

            //Assert.IsNotNull(one);
            //Assert.IsNotNull(one.Two);
            Assert.AreEqual(one.Two, two);
        }

        [TestMethod]
        public void DoesResolveFromInheritedContainerWithMultipleLevelsOfInheritance()
        {
            Container containerA = new Container();
            Container containerB = new Container();
            Container containerC = new Container();

            containerA.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();
            containerB.Inherit(containerA);
            containerC.Inherit(containerB);
            containerC.Bind<ISomeTypeOne>().To<SomeTypeOne>().AsSingleton();

            ISomeTypeOne one = containerC.Resolve<ISomeTypeOne>();
            ISomeTypeTwo two = containerA.Resolve<ISomeTypeTwo>();

            //Assert.IsNotNull(one);
            //Assert.IsNotNull(one.Two);
            Assert.AreEqual(one.Two, two);
        }

        [TestMethod]
        public void DoesInjectTransientFromInheritedContainer()
        {
            Container containerA = new Container();
            Container containerB = new Container();

            containerA.Bind<ISomeTypeOne>().To<SomeTypeOne>();
            containerB.Inherit(containerA);
            containerB.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();

            ISomeTypeOne one = containerB.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one.Two, "two did not resolve");
        }

        [TestMethod]
        public void DoesInjectTransientFromInheritedContainerWithMultipleLevelsOfInheritance()
        {
            Container containerA = new Container();
            Container containerB = new Container();
            Container containerC = new Container();

            containerA.Bind<ISomeTypeOne>().To<SomeTypeOne>();
            containerB.Inherit(containerA);
            containerC.Inherit(containerB);
            containerC.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();

            ISomeTypeOne one = containerC.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one.Two, "two did not resolve");
        }

        [TestMethod]
        public void DoesNotInjectSingletonFromInheritedContainer()
        {
            Container containerA = new Container();
            Container containerB = new Container();

            containerA.Bind<ISomeTypeOne>().To<SomeTypeOne>().AsSingleton(); // Singleton should always resolve from parent container
            containerB.Inherit(containerA);
            containerB.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();

            ISomeTypeOne one = containerB.Resolve<ISomeTypeOne>();

            Assert.IsNull(one.Two, "two did resolve");
        }

        [TestMethod]
        public void DoesNotInjectSingletonFromInheritedContainerWithMultipleLevelsOfInheritance()
        {
            Container containerA = new Container();
            Container containerB = new Container();
            Container containerC = new Container();

            containerA.Bind<ISomeTypeOne>().To<SomeTypeOne>().AsSingleton(); // Singleton should always resolve from parent container
            containerB.Inherit(containerA);
            containerC.Inherit(containerB);
            containerC.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();

            ISomeTypeOne one = containerC.Resolve<ISomeTypeOne>();

            Assert.IsNull(one.Two, "two did not resolve");
        }
        
        [TestMethod]
        public void DoesResolveInheritedBinding()
        {
            Container container = new Container();

            container.Bind<ISomeTypeEightExtended>().To<SomeTypeEightExtended>().AsSingleton();
            container.Bind<ISomeTypeEight>().Inherit(container.GetBinding<ISomeTypeEightExtended>());

            ISomeTypeEight someTypeEight = container.Resolve<ISomeTypeEight>();
            ISomeTypeEightExtended someTypeEightExtended = container.Resolve<ISomeTypeEightExtended>();

            Assert.IsNotNull(someTypeEight);
            Assert.IsNotNull(someTypeEightExtended);
        }

        [TestMethod]
        public void DoesDisposeWithInheritedBinding()
        {
            Container container = new Container();

            container.Bind<ISomeTypeEightExtended>().To<SomeTypeEightExtended>().AsSingleton();
            container.Bind<ISomeTypeEight>().Inherit(container.GetBinding<ISomeTypeEightExtended>()); 

            container.Dispose();
        }

        [TestMethod]
        public void DoesOverrideInheritedBindingWhenBoundAsTransient()
        {
            Container containerA = new Container();
            Container containerB = new Container();

            containerA.Bind<ISomeTypeOne>().To<SomeTypeOne>().AsTransient();
            containerA.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsTransient();
            containerB.Inherit(containerA);
            containerB.Bind<ISomeTypeTwo>().To<AnotherTypeTwo>().AsTransient();

            ISomeTypeOne one = containerB.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one.Two, "two did not resolve");
            Assert.IsInstanceOfType(one.Two, typeof(AnotherTypeTwo), "ContainerB did not override inhjected type");
        }

        [TestMethod]
        public void DoesOverrideInheritedBindingWhenBoundAsSingleton()
        {
            Container containerA = new Container();
            Container containerB = new Container();

            containerA.Bind<ISomeTypeOne>().To<SomeTypeOne>().AsTransient();
            containerA.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();
            containerB.Inherit(containerA);
            containerB.Bind<ISomeTypeTwo>().To<AnotherTypeTwo>().AsSingleton();

            ISomeTypeOne one = containerB.Resolve<ISomeTypeOne>();

            Assert.IsNotNull(one.Two, "two did not resolve");
            Assert.IsInstanceOfType(one.Two, typeof(AnotherTypeTwo), "ContainerB did not override inhjected type");

            ISomeTypeTwo two = containerB.Resolve<ISomeTypeTwo>();

            Assert.AreEqual(two, one.Two, "Did not resolve singleton instance");
            Assert.IsInstanceOfType(two, typeof(AnotherTypeTwo), "ContainerB did not override inhjected type");
        }
        #endregion

        #region Test 8 - DoesBootstrapBindings

        class SomeBootstrap : IBootstrap
        {
            public void SetupBindings(IContainer container)
            {
                container.Bind<ISomeTypeTwo>().To<SomeTypeTwo>().AsSingleton();
            }
        }

        [TestMethod]
        public void DoesBootstrapBindings()
        {
            Container container = new Container();
            container.Bootstrap<SomeBootstrap>();

            ISomeTypeTwo two = container.Resolve<ISomeTypeTwo>();

            Assert.IsNotNull(two);
        }

        #endregion

        #region Test 9 - DoesResolveFromFactory
        #region Interfaces & Implementations
        interface ISomeTypeTest9 { }

        class SomeTypeTest9 : ISomeTypeTest9 { }

        interface ISomeTypeTest9Factory : IFactory<ISomeTypeTest9>
        {
            int NumCreated { get; }
            IInjectContext ItemInjectContext { get; }
        }

        class SomeTypeTest9Factory : Factory<ISomeTypeTest9>, ISomeTypeTest9Factory
        {
            public int NumCreated { get; set; }
            public IInjectContext ItemInjectContext { get; private set; }

            public override ISomeTypeTest9 Create(IInjectContext injectContext)
            {
                NumCreated++;
                ItemInjectContext = injectContext;
                return new SomeTypeTest9();
            }
        }

        interface ISomeTypeTest9_B { }

        class SomeTypeTest9_B : ISomeTypeTest9_B
        {
            public SomeTypeTest9_B(ISomeTypeTest9 someType)
            {
            }
        }

        #endregion

        [TestMethod]
        public void DoesResolveFromFactory()
        {
            Container container = new Container();
            ISomeTypeTest9Factory factory = new SomeTypeTest9Factory();

            container.Bind<ISomeTypeTest9>().To<SomeTypeTest9>().FromFactory<ISomeTypeTest9Factory, ISomeTypeTest9>().AsTransient();
            container.Bind<ISomeTypeTest9Factory>(factory).To<SomeTypeTest9Factory>().AsSingleton();

            ISomeTypeTest9 someTypeTest9 = container.Resolve<ISomeTypeTest9>();

            Assert.AreEqual(1, factory.NumCreated, "did not resolve using factory");
        }


        [TestMethod]
        public void DoesResolveFromFactoryAsSingleton()
        {
            Container container = new Container();
            ISomeTypeTest9Factory factory = new SomeTypeTest9Factory();

            container.Bind<ISomeTypeTest9>().To<SomeTypeTest9>().FromFactory<ISomeTypeTest9Factory, ISomeTypeTest9>().AsSingleton();
            container.Bind<ISomeTypeTest9Factory>(factory).To<SomeTypeTest9Factory>().AsSingleton();

            ISomeTypeTest9 someTypeTest9 = container.Resolve<ISomeTypeTest9>();
            ISomeTypeTest9 someTypeTest9_B = container.Resolve<ISomeTypeTest9>();

            Assert.AreEqual(1, factory.NumCreated, "did not resolve once"); // must not be 2
        }

        [TestMethod]
        public void DoesResolveFromFactoryWithInheritedContainer()
        {
            Container parentContainer = new Container();
            Container childContainer = new Container();

            parentContainer.Bind<ISomeTypeTest9>().To<SomeTypeTest9>().FromFactory<ISomeTypeTest9Factory, ISomeTypeTest9>().AsTransient();

            ISomeTypeTest9Factory factory = new SomeTypeTest9Factory();
            parentContainer.Bind<ISomeTypeTest9Factory>(factory).To<SomeTypeTest9Factory>().AsSingleton();

            childContainer.Inherit(parentContainer);

            childContainer.Bind<ISomeTypeTest9_B>().To<SomeTypeTest9_B>().AsSingleton();

            ISomeTypeTest9_B someTypeTest9 = childContainer.Resolve<ISomeTypeTest9_B>();
            
            Assert.AreEqual(1, factory.NumCreated, "did not resolve using factory");

            Assert.AreEqual(childContainer, factory.ItemInjectContext.Container, "invalid  container");
            Assert.AreEqual(typeof(SomeTypeTest9_B), factory.ItemInjectContext.DeclaringType, "invalid declaring type");
        }


        [TestMethod]
        public void DoesResolveFromFactoryMethod()
        {
            Container container = new Container();

            int numResolved = 0;

            container.Bind<ISomeTypeTest9>().To<SomeTypeTest9>().FromFactoryMethod(delegate(IInjectContext injectContext) 
            {
                numResolved++;
                return new SomeTypeTest9();

            }).AsTransient();

            ISomeTypeTest9 someTypeTest9 = container.Resolve<ISomeTypeTest9>();
            ISomeTypeTest9 someTypeTest9_B = container.Resolve<ISomeTypeTest9>();

            Assert.AreEqual(2, numResolved, "did not resolve twice");
        }

        [TestMethod]
        public void DoesResolveFromFactoryMethodWithInheritedContainer()
        {
            int numResolved = 0;

            Container parentContainer = new Container();

            Container childContainer = new Container();

            parentContainer.Bind<ISomeTypeTest9>().To<SomeTypeTest9>().FromFactoryMethod(delegate (IInjectContext injectContext)
            {                
                Assert.AreEqual(typeof(SomeTypeTest9_B), injectContext.DeclaringType, "invalid declaring type");
                Assert.AreEqual(childContainer, injectContext.Container, "invalid container");

                numResolved++;
                return new SomeTypeTest9();

            }).AsTransient();

            childContainer.Inherit(parentContainer);
            childContainer.Bind<ISomeTypeTest9_B>().To<SomeTypeTest9_B>().AsSingleton();
            
            ISomeTypeTest9_B someTypeTest9 = childContainer.Resolve<ISomeTypeTest9_B>();

            Assert.AreEqual(1, numResolved, "did not resolve");
        }

        [TestMethod]
        public void DoesResolveFromFactoryMethodAsSingleton()
        {
            Container container = new Container();

            int numResolved = 0;

            container.Bind<ISomeTypeTest9>().To<SomeTypeTest9>().FromFactoryMethod(delegate (IInjectContext injectContext)
            {
                numResolved++;
                return new SomeTypeTest9();

            }).AsSingleton();

            ISomeTypeTest9 someTypeTest9 = container.Resolve<ISomeTypeTest9>();
            ISomeTypeTest9 someTypeTest9_B = container.Resolve<ISomeTypeTest9>();

            Assert.AreEqual(1, numResolved, "did not resolve once");
            Assert.AreEqual(someTypeTest9, someTypeTest9_B, "instances are not equal");
        }


        #region Interfaces & Implementation
        interface ISomeProvider
        {
            Type ConsumerType { get; set; }
        }

        class SomeProvider : ISomeProvider
        {
            public Type ConsumerType { get; set; }
        }

        interface ISomeProviderFactory : IFactory<ISomeProvider>
        {
        }

        class SomeProviderFactory : Factory<ISomeProvider>, ISomeProviderFactory
        {
            public override ISomeProvider Create(IInjectContext injectContext)
            {
                return new SomeProvider() { ConsumerType = injectContext.DeclaringType };
            }
        }

        interface ISomeConsumer
        {
            ISomeProvider Provider { get; set; }
        }

        interface ISomeOtherConsumer
        {
            ISomeProvider Provider { get; }
        }
        
        class SomeConsumer : ISomeConsumer
        {
            [Inject] public ISomeProvider Provider { get; set; }
        }

        class SomeOtherConsumer : ISomeOtherConsumer
        {
            public ISomeProvider Provider { get; private set; }

            SomeOtherConsumer(ISomeProvider provider)
            {
                Provider = provider;
            }
        }

        #endregion

        [TestMethod]
        public void DoesResolveFromFactoryWithInjectContext()
        {
            Container container = new Container();
            container.Bind<ISomeConsumer>().To<SomeConsumer>().AsSingleton();
            container.Bind<ISomeOtherConsumer>().To<SomeOtherConsumer>().AsSingleton();
            container.Bind<ISomeProviderFactory>().To<SomeProviderFactory>().AsSingleton();
            container.Bind<ISomeProvider>().FromFactory<ISomeProviderFactory, ISomeProvider>().AsTransient();

            ISomeConsumer consumer = container.Resolve<ISomeConsumer>();
            ISomeOtherConsumer otherConsumer = container.Resolve<ISomeOtherConsumer>();

            Assert.AreNotEqual(consumer.Provider, otherConsumer.Provider, "Provider did not resolve as transient");
            Assert.AreEqual(consumer.Provider.ConsumerType, typeof(SomeConsumer), "Incorrect consumer type");
            Assert.AreEqual(otherConsumer.Provider.ConsumerType, typeof(SomeOtherConsumer), "Incorrect consumer type");
        }

        [TestMethod]
        public void DoesResolveFromFactoryMethodWithInjectContext()
        {
            Container container = new Container();

            bool didCallFactoryMethod = false;

            container.Bind<ISomeConsumer>().To<SomeConsumer>().AsSingleton();
            container.Bind<ISomeProvider>().To<SomeProvider>().FromFactoryMethod(delegate (IInjectContext injectContext)
            {
                Assert.AreEqual(typeof(SomeConsumer), injectContext.DeclaringType, "incorrect context declaring type");
                
                didCallFactoryMethod = true;
                return new SomeProvider();

            }).AsTransient();

            ISomeConsumer consumer = container.Resolve<ISomeConsumer>();

            Assert.IsTrue(didCallFactoryMethod, "did not resolve using factory method");
        }
        #endregion

        #region Test 10 - DoesResolveDelegates

        private class SomeTypeTest10
        {
            public string Property { get; set; }

            public SomeTypeTest10(Func<string> factory)
            {
                Property = factory();
            }
        }

        [TestMethod]
        public void DoesResolveFunc()
        {
            var container = new Container();
            container.Bind<Func<string>>(() => "Hello World").AsSingleton();
            container.Bind<SomeTypeTest10>().To<SomeTypeTest10>();

            var instance = container.Resolve<SomeTypeTest10>();

            Assert.AreEqual("Hello World", instance.Property);
        }

        private delegate string StringDelegateOne(string input);
        private delegate string StringDelegateTwo(string input);

        private class SomeTypeTest11
        {
            public string Property { get; set; }
            public string Property2 { get; set; }

            public SomeTypeTest11(StringDelegateOne stringDelegateOne, StringDelegateTwo stringDelegateTwo)
            {
                Property = stringDelegateOne("Hello");
                Property2 = stringDelegateTwo("Hello");
            }
        }

        [TestMethod]
        public void DoesResolveDelegateTypes()
        {
            var container = new Container();
            container.Bind<StringDelegateOne>((input) => input).AsSingleton();
            container.Bind<StringDelegateTwo>((input) => "World").AsSingleton();
            container.Bind<SomeTypeTest11>().To<SomeTypeTest11>();

            var instance = container.Resolve<SomeTypeTest11>();

            Assert.AreEqual("Hello", instance.Property);
            Assert.AreEqual("World", instance.Property2);
        }

        #endregion

        #region Test 11 - DoesResolveGenericInterface
        interface ISomeTypeTest11<T1, T2>
        {
            T1 SomeValue1 { get; set; }
            T2 SomeValue2 { get; set; }
        }

        class SomeTypeTest11<T1, T2> : ISomeTypeTest11<T1, T2>
        {
            public T1 SomeValue1 { get; set; }
            public T2 SomeValue2 { get; set; }
        }

        [TestMethod]
        public void DoesResolveGenericInterface()
        {
            Container container = new Container();
            container.Bind<ISomeTypeTest11<string, int>>().To<SomeTypeTest11<string, int>>().AsSingleton();

            ISomeTypeTest11<string, int> someTypeTest11 = container.Resolve<ISomeTypeTest11<string, int>>();

            Assert.IsNotNull(someTypeTest11, "did not resolve generic interface");
        }
        #endregion

        #region Test 12 - HasBinding
        [TestMethod]
        public void HasBinding()
        {
            Container container = new Container();
            container.Bind<ISomeTypeOne>().To<SomeTypeOne>();

            Assert.IsTrue(container.HasBindingFor<ISomeTypeOne>(), "does not have binding");
            Assert.IsTrue(container.HasBindingFor(typeof(ISomeTypeOne)), "does not have binding");
        }

        [TestMethod]
        public void HasBindingForCategory()
        {
            object category = "category";

            Container container = new Container();
            container.Bind<ISomeTypeOne>().To<SomeTypeOne>().WithCategory(category);

            Assert.IsTrue(container.HasBindingForCategory<ISomeTypeOne>(category), "does not have binding");
            Assert.IsTrue(container.HasBindingForCategory(typeof(ISomeTypeOne), category), "does not have binding");
        }
        #endregion

        #region Test 13 - DoesInjectConstructorWithCategory
        #region Interface & Implementation
        interface ITest13_Service
        { 
        }
        
        class Test13_ServiceImplementation1 : ITest13_Service
        {
        }

        class Test13_ServiceImplementation2 : ITest13_Service
        {
        }

        interface ITest13_Consumer
        {
            ITest13_Service Service { get; }
        }

        class Test13_Consumer : ITest13_Consumer
        {
            public ITest13_Service Service { get; private set; }

            public Test13_Consumer([Inject("category")] ITest13_Service service)
            {
                Service = service;
            }
        }

        class Test13_Consumer2 : ITest13_Consumer
        {
            public ITest13_Service Service { get; set; }

            [Constructor]
            public static Test13_Consumer2 Create([Inject("category")] ITest13_Service service)
            {
                Test13_Consumer2 consumer = new Test13_Consumer2() { Service = service };
                return consumer;
            }
        }

        class Test13_Consumer3 : ITest13_Consumer
        {
            public ITest13_Service Service { get; private set; }

            public Test13_Consumer3() { }

            [Inject]
            public void OnCreate([Inject("category")] ITest13_Service service)
            {
                Service = service;
            }
        }
        #endregion

        [TestMethod]
        public void DoesInjectIntoConstructorWithCategory()
        {
            Container container = new Container();
            container.Bind<ITest13_Service>().To<Test13_ServiceImplementation1>().AsSingleton();
            container.Bind<ITest13_Service>().To<Test13_ServiceImplementation2>().WithCategory("category").AsSingleton();
            container.Bind<ITest13_Consumer>().To<Test13_Consumer>().AsSingleton();

            ITest13_Consumer consumer = container.Resolve<ITest13_Consumer>();

            Assert.AreEqual(typeof(Test13_ServiceImplementation2), consumer.Service.GetType(), "resolved with incorrect type");
        }

        [TestMethod]
        public void DoesInjectIntoConstructorMethodWithCategory()
        {
            Container container = new Container();
            container.Bind<ITest13_Service>().To<Test13_ServiceImplementation1>().AsSingleton();
            container.Bind<ITest13_Service>().To<Test13_ServiceImplementation2>().WithCategory("category").AsSingleton();
            container.Bind<ITest13_Consumer>().To<Test13_Consumer2>().AsSingleton();

            ITest13_Consumer consumer = container.Resolve<ITest13_Consumer>();

            Assert.AreEqual(typeof(Test13_ServiceImplementation2), consumer.Service.GetType(), "resolved with incorrect type");
        }

        [TestMethod]
        public void DoesInjectIntoMethodWithCategory()
        {
            Container container = new Container();
            container.Bind<ITest13_Service>().To<Test13_ServiceImplementation1>().AsSingleton();
            container.Bind<ITest13_Service>().To<Test13_ServiceImplementation2>().WithCategory("category").AsSingleton();
            container.Bind<ITest13_Consumer>().To<Test13_Consumer3>().AsSingleton();

            ITest13_Consumer consumer = container.Resolve<ITest13_Consumer>();

            Assert.AreEqual(typeof(Test13_ServiceImplementation2), consumer.Service.GetType(), "resolved with incorrect type");
        }
        #endregion
    }
}
