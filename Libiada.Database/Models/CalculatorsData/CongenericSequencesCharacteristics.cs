namespace Libiada.Database.Models.CalculatorsData;

using Libiada.Core.Core;

public record struct CongenericSequencesCharacteristics
{
    /// <summary>
    /// The research object name.
    /// </summary>
    public string ResearchObjectName;

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
    public double[] Characteristics;
}