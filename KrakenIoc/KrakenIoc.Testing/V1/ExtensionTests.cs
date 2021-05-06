using System;
using NUnit.Framework;
using AOFL.KrakenIoc.Extensions.V1;

namespace AOFL.KrakenIoc.Testing.V1
{
    public class ExtensionTests
    {
        public Action GetMyAnonymous
        {
            get { return () => { }; }
        }

        [Test]
        public void ActionDeterminesWhetherAnonymousOrNot()
        {
            Action callback = () => { };
            Action callback2 = delegate { };
            Action<int> callback3 = i => { };
            Action callback4 = SomeInternalMethod;
            Action callback5 = SomeStaticInternalMethod;

            //Sanity checks that anonymous actions' names start with '<', while actual methods do not.
            Assert.IsTrue(callback.Method.Name[0] == '<', "It is assumed in the " + nameof(ActionExtensions.IsAnonymousMethod) + "(...) method that action/method names starting with '<' are anonymous!");
            Assert.IsFalse(callback4.Method.Name[0] == '<', "It is assumed in the " + nameof(ActionExtensions.IsAnonymousMethod) + "(...) method that actual/methods names that DO NOT start with '<' are actual methods (NOT anonymous)!");

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