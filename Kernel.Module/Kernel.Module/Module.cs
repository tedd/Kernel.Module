using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Kernel.Module
{
    /// <summary>
    /// Represents a loaded .Net assembly (.dll or .exe).
    /// </summary>
    /// <remarks>This class should be completely thread safe.</remarks>
    public sealed class Module
    {
        /// <summary>
        /// Filename used to load this assembly.
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Full filename used to load this assembly.
        /// </summary>
        public string FullFileName { get; private set; }
        public string Name { get; private set; }
        private readonly Assembly _assembly;
        //private ReaderWriterLock _classesLock = new ReaderWriterLock();
        private List<ModuleClass> _moduleClasses;
        private object _moduleClassesLock = new object();

        internal Module(string filename)
        {
            FileName = filename;
            if (string.IsNullOrWhiteSpace(FileName))
                throw new Exception("Filename can't be null or empty.");

            // Load assembly
            _assembly = Assembly.LoadFrom(FileName);
            if (_assembly == null)
                throw new Exception(string.Format("Unable to load assembly \"{0}\". System.Reflection.Assembly.LoadFrom(filename) returned null object.", FileName ?? "<null>"));
            Name = _assembly.FullName ?? _assembly.GetName().FullName;

            // Get full path of assembly we just loaded
            string codeBase = _assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            FullFileName = Path.GetDirectoryName(path);

            UpdateClassList();
        }

        private void UpdateClassList()
        {
            lock (_moduleClassesLock)
            {
                _moduleClasses = new List<ModuleClass>();
                foreach (Type t in _assembly.GetTypes())
                {
                    if (t.IsPublic
                        && !t.IsAbstract
                        && t.IsClass)
                    {
                        var moduleClass = new ModuleClass(this, t);
                        _moduleClasses.Add(moduleClass);
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="System.Reflection.Assembly">Assembly</see> of loaded module.
        /// </summary>
        public Assembly Assembly
        {
            get { return _assembly; }
        }

        /// <summary>
        /// List of all classes in this module.
        /// This list will only show usable classes, meaning any type of public non-abstract class.
        /// </summary>
        /// <remarks>This list is cached load to avoid speed penalty of using reflection more than once.</remarks>
        public IEnumerable<ModuleClass> Classes
        {
            get
            {
                foreach (var c in _moduleClasses)
                {
                    yield return c;
                }
            }
        }

        /// <summary>
        /// Get all classes that inherit from type.
        /// </summary>
        /// <typeparam name="T">Type of interface</typeparam>
        /// <returns>List of classes.</returns>
        /// <remarks>Uses cached list of <see cref="Classes">Classes</see> to avoid speed penalty of using reflection.</remarks>
        public IEnumerable<ModuleClass> GetClasses<T>()
        {
            return GetClasses(typeof(T));
        }

        /// <summary>
        /// Get all classes that inherit from specified interface or base type.
        /// </summary>
        /// <typeparam name="T">Type of base or interface</typeparam>
        /// <returns>List of classes.</returns>
        /// <remarks>Uses cached list of <see cref="Classes">Classes</see> to avoid speed penalty of using reflection.</remarks>
        public IEnumerable<ModuleClass> GetClasses(Type type)
        {
            // Go through all classes in this module
            foreach (var moduleClass in Classes)
            {
                // Match? Yield return and go on.
                if (moduleClass.HasInterface(type) || moduleClass.HasBaseIn(type))
                {
                    yield return moduleClass;
                }
            }
        }

        /// <summary>
        /// Find first class with a certain name.
        /// Name can be both classname ("Class1") and full path to class ("MyProject.SomePlace.Class1").
        /// </summary>
        /// <param name="type">Type of class to find.</param>
        /// <param name="name">Optional: Class name or namespace.classname to find by searching all inherited classes and all interfaces.</param>
        /// <returns>ModuleClass wrapper for class.</returns>
        public ModuleClass GetFirstClass(Type type, string name = null)
        {
            IEnumerable<ModuleClass> list = Classes;
            if (type != null)
                list = GetClasses(type);

            foreach (var c in list)
            {
                if (name == null || (name != null && c.HasType(name)))
                {
                    return c;
                }
            }
            return null;
        }
        /// <summary>
        /// Find first class with a certain name.
        /// Name can be both classname ("Class1") and full path to class ("MyProject.SomePlace.Class1").
        /// </summary>
        /// <param name="name">Class name or namespace.classname to find.</param>
        /// <returns>ModuleClass wrapper for class.</returns>
        public ModuleClass GetFirstClass(string name)
        {
            return GetFirstClass(null, name);
        }

        /// <summary>
        /// Find first class with a certain name and type.
        /// Name can be both classname ("Class1") and full path to class ("MyProject.SomePlace.Class1").
        /// </summary>
        /// <typeparam name="T">Type to filter for</typeparam>
        /// <param name="name">Optional: Class name or namespace.classname to find.</param>
        /// <returns>ModuleClass wrapper for class.</returns>
        public ModuleClass GetFirstClass<T>(string name = null)
        {
            return GetFirstClass(typeof(T), name);
        }
    }
}
