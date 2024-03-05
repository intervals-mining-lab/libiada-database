namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.AccordanceCalculators;
using Libiada.Core.Extensions;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The accordance characteristic repository.
/// </summary>
public class AccordanceCharacteristicRepository : IAccordanceCharacteristicRepository
{
    /// <summary>
    /// The accordance characteristics links.
    /// </summary>
    private readonly AccordanceCharacteristicLink[] characteristicsLinks;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccordanceCharacteristicRepository"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public AccordanceCharacteristicRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        characteristicsLinks = db.AccordanceCharacteristicLinks.ToArray();
    }

    /// <summary>
    /// Gets the accordance characteristic links.
    /// </summary>
    public IEnumerable<AccordanceCharacteristicLink> CharacteristicLinks => characteristicsLinks.ToArray();

    /// <summary>
    /// The get link for accordance characteristic.
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
    /// The get accordance characteristic type.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
    /// </param>
    /// <returns>
    /// The <see cref="AccordanceCharacteristic"/>.
    /// </returns>
    public AccordanceCharacteristic GetCharacteristic(int characteristicLinkId)
    {
        return characteristicsLinks.Single(c => c.Id == characteristicLinkId).AccordanceCharacteristic;
    }

    /// <summary>
    /// The get accordance characteristic name.
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
    /// The get accordance characteristic name.
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
