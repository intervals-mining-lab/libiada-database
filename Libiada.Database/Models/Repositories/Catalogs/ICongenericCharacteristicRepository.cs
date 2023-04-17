using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
using System.Collections.Generic;

namespace Libiada.Database.Models.Repositories.Catalogs
{
    public interface ICongenericCharacteristicRepository
    {
        IEnumerable<CongenericCharacteristicLink> CharacteristicLinks { get; }

        CongenericCharacteristic GetCharacteristic(int characteristicLinkId);
        string GetCharacteristicName(int characteristicLinkId);
        string GetCharacteristicName(int characteristicLinkId, Notation notation);
        Link GetLinkForCharacteristic(int characteristicLinkId);
    }
}