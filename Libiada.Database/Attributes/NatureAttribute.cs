namespace Libiada.Database.Attributes;

/// <summary>
/// Used to set nature value to other enums.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class NatureAttribute : Attribute
{
    /// <summary>
    /// Nature attribute value.
    /// </summary>
    public readonly Nature Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="NatureAttribute"/> class.
    /// </summary>
    /// <param name="value">
    /// The Nature value.
    /// </param>
    public NatureAttribute(Nature value)
    {
        if (!Enum.IsDefined<Nature>(value))
        {
            throw new ArgumentException("Nature attribute value is not valid nature", nameof(value));
        }

        Value = value;
    }
}
