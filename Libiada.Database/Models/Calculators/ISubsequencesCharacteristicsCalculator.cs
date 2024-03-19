namespace Libiada.Database.Models.Calculators;

using Libiada.Database.Models.CalculatorsData;

public interface ISubsequencesCharacteristicsCalculator
{
    double[] CalculateSubsequencesCharacteristics(long parentId, short characteristicId, Feature[] features, string[] filters = null);
    SubsequenceData[] CalculateSubsequencesCharacteristics(short[] characteristicIds, Feature[] features, long parentSequenceId, string[] filters = null);
}
