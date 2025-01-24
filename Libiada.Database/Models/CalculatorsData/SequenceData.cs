namespace Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The sequence characteristics.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SequenceData"/> struct.
/// </remarks>
/// <param name="ResearchObjectId">
/// The research object id.
/// </param>
/// <param name="ResearchObjectName">
/// The research object name.
/// </param>
/// <param name="RemoteId">
/// Sequence web api id.
/// </param>
/// <param name="Characteristic">
/// The characteristic.
/// </param>
/// <param name="SubsequencesData">
/// The genes data.
/// </param>
public readonly record struct SequenceData(
    long ResearchObjectId,
    string ResearchObjectName,
    string RemoteId,
    double Characteristic,
    SubsequenceData[] SubsequencesData);
