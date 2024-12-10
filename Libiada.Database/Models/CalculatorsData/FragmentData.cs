namespace Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The fragment data of local characteristics (sliding window).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FragmentData"/> class.
/// </remarks>
/// <param name="characteristics">
/// The characteristics values.
/// </param>
/// <param name="name">
/// The name of the fragment.
/// </param>
/// <param name="start">
/// The starting position of the fragment in full sequence.
/// </param>
/// <param name="length">
/// The length of the fragment.
/// </param>
public record struct FragmentData(double[] characteristics, string name, int start, int length)
{
    /// <summary>
    /// The characteristics values.
    /// </summary>
    public double[] Characteristics = characteristics;

    /// <summary>
    /// The name of the fragment.
    /// </summary>
    public string Name = name;

    /// <summary>
    /// The starting position of the fragment in full sequence.
    /// </summary>
    public int Start = start;

    /// <summary>
    /// The length of the fragment.
    /// </summary>
    public int Length = length;
}
