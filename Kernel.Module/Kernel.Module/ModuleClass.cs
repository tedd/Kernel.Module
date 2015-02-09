using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Module
{
    /// <summary>
    /// Representation of a class that has been loaded.
    /// </summary>
    /// <remarks>This class should be completely thread safe.</remarks>
    public sealed class ModuleClass
    {
        /// <summary>
        /// Module this class belongs to
        /// </summary>
        public Module Module { get; private set; }
        /// <summary>
        /// Namespace of module (example: "My.Project")
        /// </summary>
        public string Namespace { get; private set; }
        /// <summary>
        /// Name of module (example: "MyClass")
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Full name of module including namespace (example: "My.Project.MyClass")
        /// </summary>
        public string FullName { get; private set; }
        /// <summary>
        /// Type of module (class).
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// Cached list of interfaces. Only created if required.
        /// Used for lookups, therefore a dictionary.
        /// </summary>
        private Dictionary<Type, bool> _interfaces;
        /// <summary>
        /// Cached list of all interfaces. Only created if required.
        /// Used for lookups, therefore a dictionary.
        /// </summary>
        private Dictionary<Type, bool> _allInterfaces;
        /// <summary>
        /// Strings that can be searched for to find this module.
        /// </summary>
        private Dictionary<string, bool> _searchStrings;
        /// <summary>
        /// Cached list of base classes. Only created if required.
        /// </summary>
        ///      /// <summary>
        /// Lock for _interfaces updating. Used to avoid double creating of list if more than one thread triggers interface precaching.
        /// </summary>
        private object _interfacesLock = new object();
        private List<Type> _baseClasses;
        /// <summary>
        /// Parent type of module. This is the base class that module inherited from directly. See <see cref="BaseClasses">BaseClasses</see> for list of full baseclass chain.
        /// </summary>
        public Type BaseClass { get { return Type.BaseType; } }

        /// <summary>
        /// Lock for _baseClasses updating. Used to avoid double creating of list if more than one thread triggers interface precaching.
        /// </summary>
        private object _baseClassesLock = new object();

        internal ModuleClass(Module module, Type type)
        {
            Module = module;
            Type = type;
            Namespace = Type.Namespace;
            Name = Type.Name;
            FullName = Type.FullName;
        }

        private void AddSearchString(string str)
        {
            if (_searchStrings.ContainsKey(str))
                return;
            _searchStrings.Add(str, true);
        }

        private void PreCacheInterfaceList()
        {
            lock (_interfacesLock)
            {
                if (_interfaces != null)
                    return;

                _interfaces = new Dictionary<Type, bool>();
                if (ModuleLoader.CaseInsensitiveNames)
                    _searchStrings = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                else
                    _searchStrings = new Dictionary<string, bool>();

                _searchStrings.Add(Name, false);
                if (!_searchStrings.ContainsKey(FullName))
                    _searchStrings.Add(FullName, false);

                // We first need to update a list of baseclasses, we'll be checking them too for interfaces
                PreCacheBaseClassesList();

                // Add all interfaces for this type (directly referenced)
                AddAllInterfacesRecursively(_interfaces, Type, recursively: false);

                // Copy that list to all-list
                _allInterfaces = new Dictionary<Type, bool>(_interfaces);
                foreach (var i in _interfaces.Keys)
                {
                    // Recurse through all interfaces
                    AddAllInterfacesRecursively(_allInterfaces, i);
                }

                // Add all interfaces for all basetypes
                foreach (var b in _baseClasses)
                {
                    AddAllInterfacesRecursively(_allInterfaces, b);
                }
            }
        }

        private void AddAllInterfacesRecursively(Dictionary<Type, bool> interfaces, Type type, bool recursively = true)
        {
            foreach (Type childInterface in Type.GetInterfaces())
            {
                // Already have it? Go to next
                if (interfaces.ContainsKey(childInterface))
                    continue;

                // Add to our dictionary
                _interfaces.Add(childInterface, false);

                // Add search strings
                if (!string.IsNullOrEmpty(childInterface.Name))
                    AddSearchString(childInterface.Name);
                if (!string.IsNullOrEmpty(childInterface.FullName))
                    AddSearchString(childInterface.FullName);

                // Check children
                if (recursively)
                    AddAllInterfacesRecursively(interfaces, childInterface);
            }
        }

        private void PreCacheBaseClassesList()
        {
            lock (_baseClassesLock)
            {
                if (_baseClasses != null)
                    return;
                _baseClasses = new List<Type>();
                AddAllBaseClassesRecursively(_baseClasses, Type);
            }
        }

        private void AddAllBaseClassesRecursively(List<Type> baseClasses, Type type)
        {
            if (type.BaseType == null)
                return;

            // Add to list
            baseClasses.Add(type.BaseType);
            // Add search strings
            if (!string.IsNullOrEmpty(type.BaseType.Name))
                AddSearchString(type.BaseType.Name);
            if (!string.IsNullOrEmpty(type.BaseType.FullName))
                AddSearchString(type.BaseType.FullName);

            // Recurse
            AddAllBaseClassesRecursively(baseClasses, type.BaseType);
        }

        /// <summary>
        /// Get a list of interfaces this module uses directly.
        /// </summary>
        /// <remarks>This list is cached on first use.</remarks>
        public IEnumerable<Type> Interfaces
        {
            get
            {
                PreCacheInterfaceList();
                foreach (var i in _interfaces.Keys)
                {
                    yield return i;
                }
            }
        }
        /// <summary>
        /// Get a list of all interfaces this module inherits from directly or indirectly.
        /// </summary>
        /// <remarks>This list is cached on first use.</remarks>
        public IEnumerable<Type> AllInterfaces
        {
            get
            {
                PreCacheInterfaceList();
                foreach (var i in _allInterfaces.Keys)
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Get a list of all base classes in inheritance chain.
        /// </summary>
        public IEnumerable<Type> BaseClasses
        {
            get
            {
                PreCacheInterfaceList();
                foreach (var b in _baseClasses)
                {
                    yield return b;
                }
            }
        }

        /// <summary>
        /// Check if a module can use an interface.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>This check will recursively search this module as well as all inherited baseclasses and their interfaces. In other words a complete check.</remarks>
        public bool HasInterface(Type type)
        {
            PreCacheInterfaceList();
            return _allInterfaces.ContainsKey(type);
        }


        /// <summary>
        /// Check if this modules inheritance chain contains a type.
        /// </summary>
        /// <param name="type">Type to check (a baseclass)</param>
        /// <returns>True if this type is in class inheritance chain</returns>
        public bool HasBaseIn(Type type)
        {
            PreCacheInterfaceList();
            foreach (var b in _baseClasses)
            {
                if (b == type)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if this module is compatible with (contains inherited classes or interfaces) a type by string.
        /// </summary>
        /// <param name="typeName">Type string to check for</param>
        /// <returns>True if this type is compatible</returns>
        /// <remarks>Uses indexed search.</remarks>
        public bool HasType(string typeName)
        {
            PreCacheInterfaceList();
            return _searchStrings.ContainsKey(typeName);
        }

        /// <summary>
        /// Creates an instance of this class using known type T.
        /// </summary>
        /// <typeparam name="T">Type of class to create</typeparam>
        /// <param name="args">Optional arguments required in class ctor</param>
        /// <returns>Class instance</returns>
        public T CreateInstance<T>(params object[] args)
        {
            object obj = Activator.CreateInstance(Type, args);
            T objT = (T)obj;
            return objT;
        }

        /// <summary>
        /// Creates an instance of this class of type dynamic.
        /// </summary>
        /// <param name="args">Optional arguments required in class ctor</param>
        /// <returns>Class instance</returns>
        public dynamic CreateInstance(params object[] args)
        {
            dynamic obj = Activator.CreateInstance(Type, args);
            return obj;
        }
    }
}
