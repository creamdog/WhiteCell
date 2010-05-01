using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WhiteCell.NrpeServer.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetTypes(this IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                foreach (var type in assembly.GetTypes())
                    yield return type;
        }
    }
}
