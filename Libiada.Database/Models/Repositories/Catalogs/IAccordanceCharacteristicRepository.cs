namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.AccordanceCalculators;

public interface IAccordanceCharacteristicRepository
{
    IEnumerable<AccordanceCharacteristicLink> CharacteristicLinks { get; }

    AccordanceCharacteristic GetCharacteristic(int characteristicLinkId);
    string GetCharacteristicName(int characteristicLinkId);
    string GetCharacteristicName(int characteristicLinkId, Notation notation);
    Link GetLinkForCharacteristic(int characteristicLinkId);
}
