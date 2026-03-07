using System;
using System.Runtime.CompilerServices;

namespace PageLeaf.Utilities
{
    internal static class Guard
    {
        public static void ThrowIfNull(object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
