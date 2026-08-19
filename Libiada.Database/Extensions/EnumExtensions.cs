
using Libiada.Core.Extensions;

using Libiada.Database.Attributes;

namespace Libiada.Database.Extensions;
/// <summary>
/// The enum helper.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets nature attribute value for given enum value.
    /// </summary>
    /// <typeparam name="T">
    /// Enum with nature attribute.
    /// </typeparam>
    /// <param name="value">
    /// Enum value.
    /// </param>
    /// <returns>
    /// Nature attribute value as <see cref="Nature"/>
    /// </returns>
    public static Nature GetNature<T>(this T value) where T : struct, Enum => value.GetAttribute<T, NatureAttribute>().Value;
}
