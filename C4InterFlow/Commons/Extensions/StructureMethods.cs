using C4InterFlow.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace C4InterFlow.Commons.Extensions
{
    public static class StructureMethods
    {

        public static T? GetInstance<T>(this Structure structure) where T : Structure
        {
            return Utils.GetInstance<T>(structure.Alias);
        }
    }
}
