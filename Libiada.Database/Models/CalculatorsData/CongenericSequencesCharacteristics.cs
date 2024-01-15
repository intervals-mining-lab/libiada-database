namespace Libiada.Database.Models.CalculatorsData;

using Libiada.Core.Core;

public class CongenericSequencesCharacteristics
{
    /// <summary>
    /// The matter name.
    /// </summary>
    public string MatterName;

    /// <summary>
    /// The elements of the sequence.
    /// </summary>
    public IBaseObject[] Elements;

    /// <summary>
    /// The elements names.
    /// </summary>
    public string[] ElementsNames;

    /// <summary>
    /// The sequence characteristics values.
    /// </summary>
    public double[][] Characteristics;
}