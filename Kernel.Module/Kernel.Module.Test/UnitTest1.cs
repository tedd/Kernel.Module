using System;
using System.Collections.Generic;
using System.Linq;
using Kernel.Module.Test.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kernel.Module.Test
{
    [TestClass]
    public class UnitTest1
    {
        string file1 = "Kernel.Module.TestModule1.dll";
        string namespace1 = "Kernel.Module.TestModule1";
        string file2 = "Kernel.Module.TestModule2.dll";
        string namespace2 = "Kernel.Module.TestModule2";
        string _strMyAnonymousClass = "MyAnonymousClass";
        string _strMyAnonymousClassWithConstructor = "MyAnonymousClassWithConstructor";
        string _strMyComplexClass = "MyComplexClass";
        string _strMyInterfacedClass = "MyInterfacedClass";
        string _strMyInterfacedClassByInterfaceInheritence = "MyInterfacedClassByInterfaceInheritence";
        string _strIMyModuleInterface = "IMyModuleInterface";
        string _strIMyModuleInterfaceByInheritence = "IMyModuleInterfaceByInheritence";
        string _strIOtherInterface = "IOtherInterface";
        List<string> AllClasses;
        List<string> AllInterfacedClasses;
        List<string> Interfaces;
        List<string> AllInterfacedClassesByInheritance;
        List<string> AllInterfacedClassesComplex;

        public UnitTest1()
        {
            AllClasses = new List<string>()
                             {
                                 _strMyAnonymousClass,
                                 _strMyAnonymousClassWithConstructor,
                                 _strMyComplexClass,
                                 _strMyInterfacedClass,
                                 _strMyInterfacedClassByInterfaceInheritence
                             };
            AllInterfacedClasses = new List<string>() { _strMyComplexClass, _strMyInterfacedClass, _strMyInterfacedClassByInterfaceInheritence };
            Interfaces = new List<string>() { _strIMyModuleInterface, _strIMyModuleInterfaceByInheritence, _strIOtherInterface };
            AllInterfacedClassesByInheritance = new List<string>() { _strMyInterfacedClassByInterfaceInheritence };
            AllInterfacedClassesComplex = new List<string>() { _strMyComplexClass };
        }

        public void HardRef()
        {
            var m1 = new Kernel.Module.TestModule1.MyAnonymousClass();
            var m2 = new Kernel.Module.TestModule2.MyAnonymousClass();
        }
        #region Test_Module_GetByInterface(): Test module.GetClassesWithInterface<T>() and module.GetClassesWithInterface(typeof(T));
        [TestMethod]
        public void Test_Module_GetByInterface()
        {
            var module1 = Kernel.Module.ModuleLoader.Load(file1);

            var module2 = Kernel.Module.ModuleLoader.Load(file2);

            // Interfaces
            Test_Module_GetByInterface_Particular(module1.GetClasses<IMyModuleInterface>(), AllInterfacedClasses, module1, namespace1);
            Test_Module_GetByInterface_Particular(module1.GetClasses(typeof(IMyModuleInterface)), AllInterfacedClasses, module1, namespace1);

            Test_Module_GetByInterface_Particular(module2.GetClasses<IMyModuleInterface>(), AllInterfacedClasses, module2, namespace2);
            Test_Module_GetByInterface_Particular(module2.GetClasses(typeof(IMyModuleInterface)), AllInterfacedClasses, module2, namespace2);

            // Inherited interfaces
            Test_Module_GetByInterface_Particular(module1.GetClasses<IMyModuleInterfaceByInheritence>(), AllInterfacedClassesByInheritance, module1, namespace1);
            Test_Module_GetByInterface_Particular(module1.GetClasses(typeof(IMyModuleInterfaceByInheritence)), AllInterfacedClassesByInheritance, module1, namespace1);
            
            Test_Module_GetByInterface_Particular(module2.GetClasses<IMyModuleInterfaceByInheritence>(), AllInterfacedClassesByInheritance, module2, namespace2);
            Test_Module_GetByInterface_Particular(module2.GetClasses(typeof(IMyModuleInterfaceByInheritence)), AllInterfacedClassesByInheritance, module2, namespace2);

            // Baseclasses
            Test_Module_GetByInterface_Particular(module1.GetClasses<ApplicationException>(), AllInterfacedClassesComplex, module1, namespace1);
            Test_Module_GetByInterface_Particular(module1.GetClasses<Exception>(), AllInterfacedClassesComplex, module1, namespace1);

            Test_Module_GetByInterface_Particular(module2.GetClasses<ApplicationException>(), AllInterfacedClassesComplex, module2, namespace2);
            Test_Module_GetByInterface_Particular(module2.GetClasses<Exception>(), AllInterfacedClassesComplex, module2, namespace2);

        }


        private void Test_Module_GetByInterface_Particular(IEnumerable<ModuleClass> classesWithInterface, List<string> expectedInterfaces, Module module, string ns)
        {
            Assert.IsNotNull(classesWithInterface);
            var classesWithInterfaceList = new List<ModuleClass>(classesWithInterface);
            var classesWithInterfaceListText = (from c in classesWithInterfaceList select c.FullName).ToList();
            // Test if count is correct
            Assert.IsTrue(classesWithInterfaceList.Count == expectedInterfaces.Count, "Interfaced class count in loaded module \""
                + module.FileName + "\" is not correct: Expecting: " + expectedInterfaces.Count + ", actual: " + classesWithInterfaceList.Count);
            // Test if all classes we expect is there
            foreach (var className in expectedInterfaces)
            {
                var fullClassName = ns + "." + className;
                Assert.IsTrue(classesWithInterfaceListText.Contains(fullClassName), "Interfaced class named \"" + className + "\" not found in module.");
            }
        }
        #endregion

        #region Test_Module_First(): Test module.GetFirstClass<IMyModuleInterface>();
        [TestMethod]

        public void Test_Module_First()
        {
            var module1 = Kernel.Module.ModuleLoader.Load(file1);
            var module2 = Kernel.Module.ModuleLoader.Load(file2);

            Test_Module_First_Particular(module1, namespace1);
            Test_Module_First_Particular(module2, namespace2);
        }

        private void Test_Module_First_Particular(Module module, string ns)
        {
            ModuleClass first;

            // generic by interface
            first = module.GetFirstClass<IMyModuleInterface>();
            Assert.IsNotNull(first);
            Assert.IsTrue(AllInterfacedClasses.Contains(first.Name));

            // param by interface
            first = module.GetFirstClass(typeof(IMyModuleInterface));
            Assert.IsNotNull(first);
            Assert.IsTrue(AllInterfacedClasses.Contains(first.Name));

            Test_Module_First_Particular2<IMyModuleInterface>(module, ns, _strMyInterfacedClass);
            Test_Module_First_Particular2<IMyModuleInterface>(module, ns, _strMyInterfacedClassByInterfaceInheritence);
            Test_Module_First_Particular2<IMyModuleInterfaceByInheritence>(module, ns, _strMyInterfacedClassByInterfaceInheritence);
            Test_Module_First_Particular2<IMyModuleInterface>(module, ns, _strMyComplexClass);
            Test_Module_First_Particular2<IOtherInterface>(module, ns, _strMyComplexClass);

        }

        private void Test_Module_First_Particular2<T>(Module module, string ns, string interfacedClass)
        {
            ModuleClass first;
            // generic by search: class
            first = module.GetFirstClass<T>(interfacedClass);
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Name == interfacedClass);

            // generic by search: namespace.class
            first = module.GetFirstClass<T>(ns + "." + interfacedClass);
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Name == interfacedClass);

            // param by search: class
            first = module.GetFirstClass(typeof(T), interfacedClass);
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Name == interfacedClass);

            // param by search: namespace.class
            first = module.GetFirstClass(typeof(T), ns + "." + interfacedClass);
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Name == interfacedClass);

            // any by search: class
            first = module.GetFirstClass(interfacedClass);
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Name == interfacedClass);

            // any by search: namespace.class
            first = module.GetFirstClass(ns + "." + interfacedClass);
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Name == interfacedClass);

        }
        #endregion

        #region Test_Module_ClassList(): Check if Classes list contains exact correct classes
        [TestMethod]
        public void Test_Module_ClassList()
        {

            var module1 = Kernel.Module.ModuleLoader.Load(file1);
            var module2 = Kernel.Module.ModuleLoader.Load(file2);

            Test_Module_ClassList_Particular(module1, namespace1);
            Test_Module_ClassList_Particular(module2, namespace2);
        }

        private void Test_Module_ClassList_Particular(Module module, string ns)
        {
            Assert.IsNotNull(module);
            // Get name of all modules
            var moduleClassListStr = new List<string>();
            foreach (var t in module.Classes)
            {
                moduleClassListStr.Add(t.FullName);
            }
            // Test if count is correct
            Assert.IsTrue(moduleClassListStr.Count == AllClasses.Count,
                          "Class count in loaded module \"" + module.FileName + "\" is not correct: Expecting: " + AllClasses.Count +
                          ", actual: " + moduleClassListStr.Count);
            // Test if all classes we expect is there
            foreach (var c in AllClasses)
            {
                var className = ns + "." + c;
                Assert.IsTrue(moduleClassListStr.Contains(className),
                              "Class named \"" + className + "\" not found in module.");
            }
        }
        #endregion

        #region Test_ModuleClass_InstancingDynamic(): Test if we can create dynamic instances

        [TestMethod]
        public void Test_ModuleClass_InstancingDynamic()
        {

            var module1 = Kernel.Module.ModuleLoader.Load(file1);
            var module2 = Kernel.Module.ModuleLoader.Load(file2);

            Test_ModuleClass_InstancingDynamic_Particular<IMyModuleInterface>(module1, namespace1, _strMyInterfacedClass);
            Test_ModuleClass_InstancingDynamic_Particular<IMyModuleInterface>(module2, namespace2, _strMyInterfacedClass);
        }

        private void Test_ModuleClass_InstancingDynamic_Particular<T>(Module module, string ns, string className)
        {

            var first = module.GetFirstClass<T>();
            Assert.IsNotNull(first);

            var instance = first.CreateInstance();
            var testStr = "weifweoiAAAAAAAAAAAAAAA3iuofjh32wwo9ijr#¤F%WGFREGFERWF";
            Assert.IsTrue(instance.ToLower(testStr) == testStr.ToLower());
            Assert.IsTrue(instance.Last == testStr.ToLower());
        }

        #endregion

        #region Test_ModuleClass_InstancingGeneric(): Test if we can create dynamic instances

        [TestMethod]
        public void Test_ModuleClass_InstancingGeneric()
        {

            var module1 = Kernel.Module.ModuleLoader.Load(file1);
            var module2 = Kernel.Module.ModuleLoader.Load(file2);

            Test_ModuleClass_InstancingGeneric_Particular(module1, namespace1, _strMyInterfacedClass);
            Test_ModuleClass_InstancingGeneric_Particular(module2, namespace2, _strMyInterfacedClass);
        }

        private void Test_ModuleClass_InstancingGeneric_Particular(Module module, string ns, string className)
        {
            var first = module.GetFirstClass<IMyModuleInterface>();
            Assert.IsNotNull(first);

            var instance = first.CreateInstance<IMyModuleInterface>();
            var testStr = "weifweoiAAAAAAAffffAAAAAAAA3iuofjh32o9ijr#¤F%WGFREGFERWF";
            Assert.IsTrue(instance.ToLower(testStr) == testStr.ToLower());
            Assert.IsTrue(instance.Last == testStr.ToLower());
        }

        #endregion
        //dynamic anon1 = repository.CreateInstanceFromName(assemblyList[0], "Kernel.Module.TestModule1.MyAnonymousClass");
        //Assert.IsTrue(anon1.Last == null);

        //dynamic anon2 = repository.CreateInstanceFromName(assemblyList[0], "Kernel.Module.TestModule1.MyAnonymousClassWithConstructor", "Test123");
        //Assert.IsTrue(anon2.Last == "Test123");

        //Assert.IsTrue(anon1.ToUpper("abc") == "abc".ToUpper());
        //Assert.IsTrue(anon1.Last == "abc".ToUpper());

        //Assert.IsTrue(anon2.ToUpper("cde") == "cde".ToUpper());
        //Assert.IsTrue(anon2.Last == "cde".ToUpper());


        //dynamic anon2 = repository.CreateInstanceFromName(assemblyList[0], "Kernel.Module.TestModule1.MyAnonymousClassWithConstructor", "Test123");
        //Assert.IsTrue(anon2.Last == "Test123");
    }

}
