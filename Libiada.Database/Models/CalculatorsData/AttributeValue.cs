namespace Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The attribute value struct.
/// </summary>
/// <param name="attributeId">
/// The attribute id.
/// </param>
/// <param name="value">
/// The attribute value.
/// </param>
public readonly record struct AttributeValue(byte AttributeId, string Value) : IEquatable<AttributeValue>;
