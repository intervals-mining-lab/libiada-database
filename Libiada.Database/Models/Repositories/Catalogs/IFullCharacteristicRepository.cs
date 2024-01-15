namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;

public interface IFullCharacteristicRepository
{
    IEnumerable<FullCharacteristicLink> CharacteristicLinks { get; }

    FullCharacteristic GetCharacteristic(int characteristicLinkId);
    string GetCharacteristicName(int characteristicLinkId);
    string GetCharacteristicName(int characteristicLinkId, Notation notation);
    Link GetLinkForCharacteristic(int characteristicLinkId);
}