namespace Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The sequence characteristics.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SequenceData"/> struct.
/// </remarks>
/// <param name="matterId">
/// The matter id.
/// </param>
/// <param name="matterName">
/// The matter name.
/// </param>
/// <param name="remoteId">
/// Sequence web api id.
/// </param>
/// <param name="characteristic">
/// The characteristic.
/// </param>
/// <param name="subsequencesData">
/// The genes data.
/// </param>
public readonly record struct SequenceData(long matterId, string matterName, string remoteId, double characteristic, SubsequenceData[] subsequencesData)
{
    /// <summary>
    /// The matter id.
    /// </summary>
    public readonly long MatterId = matterId;

    /// <summary>
    /// The matter name.
    /// </summary>
    public readonly string MatterName = matterName;

    /// <summary>
    /// Sequence web api id.
    /// </summary>
    public readonly string RemoteId = remoteId;

    /// <summary>
    /// The characteristic.
    /// </summary>
    public readonly double Characteristic = characteristic;

    /// <summary>
    /// The genes data.
    /// </summary>
    public readonly SubsequenceData[] SubsequencesData = subsequencesData;
}
