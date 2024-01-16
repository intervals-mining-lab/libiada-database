namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.CongenericCalculators;

public interface ICongenericCharacteristicRepository
{
    IEnumerable<CongenericCharacteristicLink> CharacteristicLinks { get; }

    CongenericCharacteristic GetCharacteristic(int characteristicLinkId);
    string GetCharacteristicName(int characteristicLinkId);
    string GetCharacteristicName(int characteristicLinkId, Notation notation);
    Link GetLinkForCharacteristic(int characteristicLinkId);
}
