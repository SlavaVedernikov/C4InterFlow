using C4InterFlow.Elements;
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

        public static T? GetInstance<T>(this Structure structure) where T : class
        {
            return Utils.GetInstance<T>(structure.Alias);
        }

        public static string GetPhase(this Structure structure)
        {
            // TODO: Add support for a concept of Phase (or possibly different name) e.g. "Design" or "Initiative name" etc.
            return string.Empty;
        }
    }
}
