namespace Libiada.Database.Models.CalculatorsData;

public record struct LocalCharacteristicsData(
    string matterName,
    FragmentData[] fragmentsData,
    double[][] differenceData,
    double[][] fourierData,
    double[][] autocorrelationData)
{
    public readonly string MatterName = matterName;

    public readonly FragmentData[] FragmentsData = fragmentsData;

    public readonly double[][] DifferenceData = differenceData;

    public readonly double[][] FourierData = fourierData;

    public readonly double[][] AutocorrelationData = autocorrelationData;
}
