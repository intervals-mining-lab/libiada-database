using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
using System.Collections.Generic;

namespace Libiada.Database.Models.Repositories.Catalogs
{
    public interface IFullCharacteristicRepository
    {
        IEnumerable<FullCharacteristicLink> CharacteristicLinks { get; }

        FullCharacteristic GetCharacteristic(int characteristicLinkId);
        string GetCharacteristicName(int characteristicLinkId);
        string GetCharacteristicName(int characteristicLinkId, Notation notation);
        Link GetLinkForCharacteristic(int characteristicLinkId);
    }
}