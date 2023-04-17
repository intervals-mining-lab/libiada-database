using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
using System.Collections.Generic;

namespace Libiada.Database.Models.Repositories.Catalogs
{
    public interface IAccordanceCharacteristicRepository
    {
        IEnumerable<AccordanceCharacteristicLink> CharacteristicLinks { get; }

        AccordanceCharacteristic GetCharacteristic(int characteristicLinkId);
        string GetCharacteristicName(int characteristicLinkId);
        string GetCharacteristicName(int characteristicLinkId, Notation notation);
        Link GetLinkForCharacteristic(int characteristicLinkId);
    }
}