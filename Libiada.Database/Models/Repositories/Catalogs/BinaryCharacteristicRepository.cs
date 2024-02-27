namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.BinaryCalculators;
using Libiada.Core.Extensions;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The binary characteristic repository.
/// </summary>
public class BinaryCharacteristicRepository : IBinaryCharacteristicRepository
{
    /// <summary>
    /// The binary characteristic links.
    /// </summary>
    private readonly BinaryCharacteristicLink[] characteristicsLinks;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryCharacteristicRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// The db.
    /// </param>
    public BinaryCharacteristicRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        characteristicsLinks = db.BinaryCharacteristicLinks.ToArray();
    }

    /// <summary>
    /// Gets the binary characteristic links.
    /// </summary>
    public IEnumerable<BinaryCharacteristicLink> CharacteristicLinks => characteristicsLinks.ToArray();

    /// <summary>
    /// The create binary characteristic.
    /// </summary>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic id.
    /// </param>
    /// <param name="firstElementId">
    /// The first element id.
    /// </param>
    /// <param name="secondElementId">
    /// The second element id.
    /// </param>
    /// <param name="value">
    /// The value.
    /// </param>
    /// <returns>
    /// The <see cref="BinaryCharacteristicValue"/>.
    /// </returns>
    public BinaryCharacteristicValue CreateCharacteristic(long sequenceId, short characteristicLinkId, long firstElementId, long secondElementId, double value)
    {
        var characteristic = new BinaryCharacteristicValue
        {
            SequenceId = sequenceId,
            CharacteristicLinkId = characteristicLinkId,
            FirstElementId = firstElementId,
            SecondElementId = secondElementId,
            Value = value
        };
        return characteristic;
    }

    /// <summary>
    /// The get link for binary characteristic.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic link id.
    /// </param>
    /// <returns>
    /// The <see cref="Link"/>.
    /// </returns>
    public Link GetLinkForCharacteristic(int characteristicLinkId)
    {
        return characteristicsLinks.Single(c => c.Id == characteristicLinkId).Link;
    }

    /// <summary>
    /// The get binary characteristic type.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic link id.
    /// </param>
    /// <returns>
    /// The <see cref="BinaryCharacteristic"/>.
    /// </returns>
    public BinaryCharacteristic GetCharacteristic(int characteristicLinkId)
    {
        return characteristicsLinks.Single(c => c.Id == characteristicLinkId).BinaryCharacteristic;
    }

    /// <summary>
    /// The get binary characteristic name.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic link id.
    /// </param>
    /// <param name="notation">
    /// The notation.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public string GetCharacteristicName(int characteristicLinkId, Notation notation)
    {
        return string.Join("  ", GetCharacteristicName(characteristicLinkId), notation.GetDisplayValue());
    }

    /// <summary>
    /// The get binary characteristic name.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic link id.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public string GetCharacteristicName(int characteristicLinkId)
    {
        string characteristicTypeName = GetCharacteristic(characteristicLinkId).GetDisplayValue();

        Link link = GetLinkForCharacteristic(characteristicLinkId);
        string linkName = link == Link.NotApplied ? string.Empty : link.GetDisplayValue();

        return string.Join("  ", characteristicTypeName, linkName);
    }
}
