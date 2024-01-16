namespace Libiada.Database.Models.Calculators;

public interface ISequencesCharacteristicsCalculator
{
    Dictionary<long, Dictionary<short, double>> Calculate(Dictionary<long, short[]> chainCharacteristicsIds);
    double[] Calculate(long[] chainIds, short characteristicLinkId);
    double[][] Calculate(long[][] chainIds, short[] characteristicLinkIds);
    double[][] Calculate(long[][] chainIds, short[] characteristicLinkIds, bool rotate, bool complementary, uint? rotationLength);
}
