namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.BinaryCalculators;

public interface IBinaryCharacteristicRepository
{
    IEnumerable<BinaryCharacteristicLink> CharacteristicLinks { get; }

    BinaryCharacteristicValue CreateCharacteristic(long sequenceId, short characteristicLinkId, long firstElementId, long secondElementId, double value);
    BinaryCharacteristic GetCharacteristic(int characteristicLinkId);
    string GetCharacteristicName(int characteristicLinkId);
    string GetCharacteristicName(int characteristicLinkId, Notation notation);
    Link GetLinkForCharacteristic(int characteristicLinkId);
}
