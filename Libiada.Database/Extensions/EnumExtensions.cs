namespace Libiada.Database.Extensions;

using Libiada.Core.Extensions;

using Libiada.Database.Attributes;

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
    public static Nature GetNature<T>(this T value) where T : struct, IComparable, IFormattable, IConvertible
    {
        return value.GetAttribute<T, NatureAttribute>().Value;
    }
}
