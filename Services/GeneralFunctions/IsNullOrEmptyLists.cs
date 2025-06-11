using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GeneralFunctions;

public static class CollectionExtensions
{
    // Extensión para verificar si una lista es nula o está vacía
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
    {
        return list == null || !list.Any();
    }
    public static bool IsNullOrEmpty(this string str)
    {
        return str == null || str.Length == 0;
    }
}
