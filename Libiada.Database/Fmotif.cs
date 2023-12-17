using System;
using System.Collections.Generic;
using Libiada.Database;
using LibiadaCore.Core.SimpleTypes;


namespace Libiada.Database;

/// <summary>
/// Contains elements that represent note sequences in form of formal motifs that are used as elements of segmented music sequences.
/// </summary>
public partial class Fmotif  
{
    /// <summary>
    /// Unique internal identifier of the fmotif.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Fmotif hash value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Fmotif description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Fmotif name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Fmotif notation enum numeric value (always 6).
    /// </summary>
    public Notation Notation { get; set; }

    /// <summary>
    /// Fmotif creation date and time (filled trough trigger).
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTime Modified { get; set; }

    /// <summary>
    /// Fmotif&apos;s alphabet (array of notes ids).
    /// </summary>
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Fmotif&apos;s order.
    /// </summary>
    public List<int> Building { get; set; } = null!;

    /// <summary>
    /// Fmotif type enum numeric value.
    /// </summary>
    public FmotifType FmotifType { get; set; }
}
