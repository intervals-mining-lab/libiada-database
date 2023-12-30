using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Contains elements that represent note sequences in form of measures (bars) that are used as elements of segmented music sequences.
/// </summary>
public partial class Measure  
{
    /// <summary>
    /// Unique internal identifier of the measure.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Measure hash code.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Description of the sequence.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Measure name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Measure notation enum numeric value (always 7).
    /// </summary>
    public Notation Notation { get; set; }

    /// <summary>
    /// Measure creation date and time (filled trough trigger).
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Measure alphabet (array of notes ids).
    /// </summary>
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Measure order.
    /// </summary>
    public List<int> Building { get; set; } = null!;

    /// <summary>
    /// Time signature upper numeral (Beat numerator).
    /// </summary>
    public int Beats { get; set; }

    /// <summary>
    /// Time signature lower numeral (Beat denominator).
    /// </summary>
    public int Beatbase { get; set; }

    /// <summary>
    /// Key signature of the measure (negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis)).
    /// </summary>
    public int Fifths { get; set; }

    /// <summary>
    /// Music mode of the measure. true  represents major and false represents minor.
    /// </summary>
    public bool major { get; set; }
}
