using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
using System.Collections.Generic;

namespace Libiada.Database.Models.Repositories.Catalogs
{
    public interface IBinaryCharacteristicRepository
    {
        IEnumerable<BinaryCharacteristicLink> CharacteristicLinks { get; }

        BinaryCharacteristicValue CreateCharacteristic(long sequenceId, short characteristicLinkId, long firstElementId, long secondElementId, double value);
        BinaryCharacteristic GetCharacteristic(int characteristicLinkId);
        string GetCharacteristicName(int characteristicLinkId);
        string GetCharacteristicName(int characteristicLinkId, Notation notation);
        Link GetLinkForCharacteristic(int characteristicLinkId);
    }
}