namespace Libiada.Database.Models.CalculatorsData;

public record struct LocalCharacteristicsData(
    string ResearchObjectName,
    FragmentData[] FragmentsData,
    double[][] DifferenceData,
    double[][] FourierData,
    double[][] AutocorrelationData);
