namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.CongenericCalculators;
using Libiada.Core.Extensions;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The congeneric characteristic repository.
/// </summary>
public class CongenericCharacteristicRepository : ICongenericCharacteristicRepository
{
    /// <summary>
    /// The congeneric characteristic links.
    /// </summary>
    private readonly CongenericCharacteristicLink[] characteristicsLinks;

    /// <summary>
    /// Initializes a new instance of the <see cref="CongenericCharacteristicRepository"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public CongenericCharacteristicRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        characteristicsLinks = db.CongenericCharacteristicLinks.ToArray();
    }

    /// <summary>
    /// Gets the congeneric characteristic links.
    /// </summary>
    public IEnumerable<CongenericCharacteristicLink> CharacteristicLinks => characteristicsLinks.ToArray();

    /// <summary>
    /// The get link for congeneric characteristic.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
    /// </param>
    /// <returns>
    /// The <see cref="Link"/>.
    /// </returns>
    public Link GetLinkForCharacteristic(int characteristicLinkId)
    {
        return characteristicsLinks.Single(c => c.Id == characteristicLinkId).Link;
    }

    /// <summary>
    /// The get congeneric characteristic type.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
    /// </param>
    /// <returns>
    /// The <see cref="CongenericCharacteristic"/>.
    /// </returns>
    public CongenericCharacteristic GetCharacteristic(int characteristicLinkId)
    {
        return characteristicsLinks.Single(c => c.Id == characteristicLinkId).CongenericCharacteristic;
    }

    /// <summary>
    /// The get congeneric characteristic name.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
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
    /// The get congeneric characteristic name.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
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
