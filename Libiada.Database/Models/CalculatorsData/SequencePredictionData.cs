namespace Libiada.Database.Models.CalculatorsData;

public record struct SequencePredictionData(double theoreticalCharacteristic, double actualCharacteristic, string fragment, string predicted)
{
    public double TheoreticalCharacteristic = theoreticalCharacteristic;
    public double ActualCharacteristic = actualCharacteristic;
    public string Fragment = fragment;
    public string Predicted = predicted;
}