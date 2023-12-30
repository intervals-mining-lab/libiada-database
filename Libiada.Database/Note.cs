using System;
using System.Collections.Generic;
using Libiada.Database;
using LibiadaCore.Core.SimpleTypes;


namespace Libiada.Database;

/// <summary>
/// Contains elements that represent notes that are used as elements of music sequences.
/// </summary>
public partial class Note  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Note hash code.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Note description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Note name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Measure notation enum numeric value (always 8).
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
    /// Note duration fraction numerator.
    /// </summary>
    public int Numerator { get; set; }

    /// <summary>
    /// Note duration fraction denominator.
    /// </summary>
    public int Denominator { get; set; }

    /// <summary>
    /// Flag indicating if note is a part of triplet (tuplet).
    /// </summary>
    public bool Triplet { get; set; }

    /// <summary>
    /// Note tie type enum numeric value.
    /// </summary>
    public Tie Tie { get; set; }

    public virtual ICollection<Pitch> Pitches { get; set; } = new List<Pitch>();
}
