using System;
using Kernel.Module.Test.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kernel.Module.Test
{
    [TestClass]
    public class UnitTest2
    {
        string file1 = "Kernel.Module.TestModule1.dll";
        [TestMethod]
        public void OneLiner()
        {

            IMyModuleInterface instance =
                Kernel.Module.ModuleLoader.Load(file1)
                      .GetFirstClass<IMyModuleInterface>()
                      .CreateInstance<IMyModuleInterface>();
            Assert.IsTrue(instance.ToLower("AAA") == "AAA".ToLower());
        }
    }
}
