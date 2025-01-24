namespace Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The research object import result.
/// </summary>
public record struct ResearchObjectImportResult
{
    /// <summary>
    /// All possible names of the research object.
    /// </summary>
    public string AllNames;

    /// <summary>
    /// The saved name.
    /// </summary>
    public string ResearchObjectName;

    /// <summary>
    /// The result of import.
    /// </summary>
    public string Result;

    /// <summary>
    /// The status.
    /// </summary>
    public string Status;

    /// <summary>
    /// The sequence type.
    /// </summary>
    public string SequenceType;

    /// <summary>
    /// The group.
    /// </summary>
    public string Group;
}
