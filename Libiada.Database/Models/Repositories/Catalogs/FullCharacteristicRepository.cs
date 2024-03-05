namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Core.Extensions;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The full characteristic repository.
/// </summary>
public class FullCharacteristicRepository : IFullCharacteristicRepository
{
    /// <summary>
    /// The characteristic type links.
    /// </summary>
    private readonly FullCharacteristicLink[] characteristicsLinks;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullCharacteristicRepository"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public FullCharacteristicRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        characteristicsLinks = db.FullCharacteristicLinks.ToArray();
    }

    /// <summary>
    /// Gets the characteristic type links.
    /// </summary>
    public IEnumerable<FullCharacteristicLink> CharacteristicLinks => characteristicsLinks.ToArray();

    /// <summary>
    /// The get libiada link.
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
    /// The get characteristic type.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
    /// </param>
    /// <returns>
    /// The <see cref="FullCharacteristic"/>.
    /// </returns>
    public FullCharacteristic GetCharacteristic(int characteristicLinkId)
    {
        return characteristicsLinks.Single(c => c.Id == characteristicLinkId).FullCharacteristic;
    }

    /// <summary>
    /// The get characteristic name.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type and link id.
    /// </param>
    /// <param name="notation">
    /// The notation id.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public string GetCharacteristicName(int characteristicLinkId, Notation notation)
    {
        return string.Join("  ", GetCharacteristicName(characteristicLinkId), notation.GetDisplayValue());
    }

    /// <summary>
    /// The get characteristic name.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type and link id.
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
