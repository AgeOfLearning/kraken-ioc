using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AOFL.KrakenIoc.Extensions.V1;

namespace AOFL.KrakenIoc.Testing.V1
{
    [TestClass]
    public class ExtensionTests
    {
        public Action GetMyAnonymous
        {
            get { return () => { }; }
        }

        [TestMethod]
        public void ActionDeterminesWhetherAnonymousOrNot()
        {
            Action callback = () => { };
            Action callback2 = delegate { };
            Action<int> callback3 = i => { };
            Action callback4 = SomeInternalMethod;
            Action callback5 = SomeStaticInternalMethod;

            Assert.IsTrue(callback.IsAnonymousMethod());
            Assert.IsTrue(callback2.IsAnonymousMethod());
            Assert.IsTrue(callback3.IsAnonymousMethod());
            Assert.IsTrue(GetMyAnonymous.IsAnonymousMethod());
            Assert.IsFalse(callback4.IsAnonymousMethod());
            Assert.IsFalse(callback5.IsAnonymousMethod());
        }

        private void SomeInternalMethod()
        {

        }

        private static void SomeStaticInternalMethod()
        {

        }

    }
}