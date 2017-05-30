using System;
using NUnit.Framework;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class CurrentAppTest
    {
        [Test]
        public void IsSingleInstanceTest()
        {
            Assert.IsTrue(Runtime.IsTheOnlyInstance);
        }

        [Test]
        public void IsOsDifferentUnknownTest()
        {
            Assert.AreNotEqual(Runtime.OS, OperatingSystem.Unknown, $"Retrieving a OS: {Runtime.OS}");
        }

        [Test]
        public void IsUsingMonoRuntimeTest()
        {
            Assert.AreEqual(Type.GetType("Mono.Runtime") != null, Runtime.IsUsingMonoRuntime);
        }
        
        [Test]
        public void GetAssemblyAttributesTest()
        {
            if (Runtime.Process.ProcessName.StartsWith("JetBrains.ReSharper")) Assert.Ignore("Ignore Resharper runner");
            Assert.AreEqual("NUnit Software", Runtime.CompanyName);
            Assert.AreEqual("dotnet_test_nunit", Runtime.ProductName);
            Assert.AreEqual("NUnit is a trademark of NUnit Software", Runtime.ProductTrademark);
        }
        
        [Test]
        public void GetLocalStorageTest()
        {
            Assert.IsNotEmpty(Runtime.LocalStoragePath, $"Retrieving a local storage path: {Runtime.LocalStoragePath}");
        }

        [Test]
        public void GetProcessTest()
        {
            if (Runtime.Process.ProcessName.StartsWith("JetBrains.ReSharper")) Assert.Ignore("Ignore Resharper runner");
            Assert.IsNotNull(Runtime.Process);
            Assert.AreEqual(Runtime.Process.ProcessName,
#if NET452
                "dotnet-test-nunit"
#else
                "dotnet"
#endif
                );
        }
        
        [Test]
        public void GetEntryAssemblyTest()
        {
            if (Runtime.Process.ProcessName.StartsWith("JetBrains.ReSharper")) Assert.Ignore("Ignore Resharper runner");
            var entryAssembly = Runtime.EntryAssembly;
            Assert.IsNotNull(entryAssembly);
            Assert.IsTrue(entryAssembly.FullName.StartsWith("dotnet-test-nunit"));
        }

        [Test]
        public void GetEntryAssemblyDirectoryTest()
        {
            Assert.IsNotNull(Runtime.EntryAssemblyDirectory);
            // TODO: What else?
        }
    }
}
