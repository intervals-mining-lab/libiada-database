namespace Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The fragment data of local characteristics (sliding window).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FragmentData"/> class.
/// </remarks>
/// <param name="Characteristics">
/// The characteristics values.
/// </param>
/// <param name="Name">
/// The name of the fragment.
/// </param>
/// <param name="start">
/// The starting position of the fragment in full sequence.
/// </param>
/// <param name="length">
/// The length of the fragment.
/// </param>
public record struct FragmentData(double[] Characteristics, string Name, int Start, int Length);
