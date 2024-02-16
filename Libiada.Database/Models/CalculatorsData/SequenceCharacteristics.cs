namespace Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The sequence characteristics.
/// </summary>
public record struct SequenceCharacteristics
{
    /// <summary>
    /// The matter name.
    /// </summary>
    public string MatterName;

    /// <summary>
    /// The sequence characteristics values.
    /// </summary>
    public double[] Characteristics;
}
