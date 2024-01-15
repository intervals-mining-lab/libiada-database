namespace Libiada.Database.Extensions;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public static class CollectionExtensions
{
    /// <summary>
    /// Indicates whether the specified collection is null or has a length of zero.
    /// </summary>
    /// <param name="array">
    /// The <see cref="ICollection"/> to test.
    /// </param>
    /// <returns>
    /// True if the array parameter is null or has a length of zero; otherwise, false.
    /// </returns>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ICollection<T> collection) => collection == null || collection.Count == 0;
}
