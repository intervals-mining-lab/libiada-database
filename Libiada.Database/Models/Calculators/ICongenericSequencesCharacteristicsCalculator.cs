
namespace Libiada.Database.Models.Calculators;

public interface ICongenericSequencesCharacteristicsCalculator
{
    Dictionary<long, Dictionary<(short, long), double>> Calculate(Dictionary<long, short[]> sequenceCharacteristicsIds);
    Dictionary<long, Dictionary<(short, long), double>> Calculate(long[][] sequenceIds, short[] characteristicLinkIds);
}
