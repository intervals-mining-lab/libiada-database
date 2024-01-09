using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libiada.Database;
using LibiadaCore.Core.SimpleTypes;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains elements that represent notes that are used as elements of music sequences.
/// </summary>
[Table("note")]
[Index("Notation", Name = "ix_note_notation_id")]
[Index("Value", Name = "uk_note", IsUnique = true)]
public partial class Note  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Note hash code.
    /// </summary>
    [Column("value")]
    [StringLength(255)]
    public string? Value { get; set; }

    /// <summary>
    /// Note description.
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Note name.
    /// </summary>
    [Column("name")]
    [StringLength(255)]
    public string? Name { get; set; }

    /// <summary>
    /// Measure notation enum numeric value (always 8).
    /// </summary>
    [Column("notation")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Measure creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Note duration fraction numerator.
    /// </summary>
    [Column("numerator")]
    public int Numerator { get; set; }

    /// <summary>
    /// Note duration fraction denominator.
    /// </summary>
    [Column("denominator")]
    public int Denominator { get; set; }

    /// <summary>
    /// Flag indicating if note is a part of triplet (tuplet).
    /// </summary>
    [Column("triplet")]
    public bool Triplet { get; set; }

    /// <summary>
    /// Note tie type enum numeric value.
    /// </summary>
    [Column("tie")]
    public Tie Tie { get; set; }

    [ForeignKey("NoteId")]
    [InverseProperty("Notes")]
    public virtual ICollection<Pitch> Pitches { get; set; } = new List<Pitch>();
}
