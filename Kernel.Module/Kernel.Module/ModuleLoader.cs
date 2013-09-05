using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Module
{
    /// <summary>
    /// Static class for loading modules..
    /// </summary>
    public static class ModuleLoader
    {
        static ModuleLoader()
        {
            CaseInsensitiveNames = false;
        }

        /// <summary>
        /// Loads a .Net assembly (.dll or .exe).
        /// </summary>
        /// <param name="filename">Filename to load</param>
        /// <returns><see cref="Module">Module</see></returns>
        public static Module Load(string filename)
        {
            var module = new Module(filename);
            return module;
        }

        /// <summary>
        /// Toggles if text searches for modules should be case sensitive. Must be set before module is loaded.
        /// Modules use an internal index for searching. The index is build based on this setting. So once built its too late to change it.
        /// </summary>
        public static bool CaseInsensitiveNames { get; set; }
    }
}
