namespace Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The sequence characteristics.
/// </summary>
public readonly record struct SequenceCharacteristics
{
    /// <summary>
    /// The matter name.
    /// </summary>
    public readonly string MatterName { get; init; }

    /// <summary>
    /// Id of the sequence group the matter belongs to.
    /// </summary>
    /// <value>
    /// The sequence group id as <see cref="int"/>.
    /// </value>
    public readonly int? SequenceGroupId { get; init; }

    /// <summary>
    /// The sequence characteristics values.
    /// </summary>
    public readonly double[] Characteristics { get; init; }
}
