using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Module.TestModule2
{
    public class MyAnonymousClass
    {
        private string _last;

        public string ToUpper(string str)
        {
            _last = str.ToUpper();
            return _last;
        }

        public string Last
        {
            get { return _last; }
        }
    }
}
