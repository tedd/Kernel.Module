using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Module
{
    public class ModuleRepository
    {
        private readonly List<Module> _modules = new List<Module>();

        public void Load(string filename)
        {
            var module = ModuleLoader.Load(filename);
            lock (_modules)
            {
                _modules.Add(module);
            }
        }

        public IEnumerable<Module> Modules
        {
            get
            {
                List<Module> modules;
                lock (_modules)
                {
                    modules = new List<Module>(_modules);
                }
                foreach (var module in modules)
                {
                    yield return module;
                }
            }
        }

        public IEnumerable<ModuleClass> GetClasses<T>()
        {
            foreach (var module in Modules)
            {
                foreach (var classModule in module.GetClasses<T>())
                {
                    yield return classModule;
                }
            }
        }

        public IEnumerable<ModuleClass> GetClasses(Type type)
        {

            foreach (var module in Modules)
            {
                foreach (var classModule in module.GetClasses(type))
                {
                    yield return classModule;
                }
            }
        }

        public ModuleClass GetFirstClass(Type type, string name = null)
        {
            foreach (var module in Modules)
            {
                var classModule = module.GetFirstClass(type, name);
                if (classModule != null)
                    return classModule;
            }
            return null;
        }
        public ModuleClass GetFirstClass(string name)
        {
            return GetFirstClass(null, name);
        }
        public ModuleClass GetFirstClass<T>(string name = null)
        {
            return GetFirstClass(typeof(T), name);
        }
    }
}
