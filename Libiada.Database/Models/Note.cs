namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core.SimpleTypes;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains elements that represent notes that are used as elements of music sequences.
/// </summary>
[Table("note")]
[Comment("Contains elements that represent notes that are used as elements of music sequences.")]
public partial class Note : Element
{
    /// <summary>
    /// Note duration fraction numerator.
    /// </summary>
    [Column("numerator")]
    [Display(Name = "Note duration fraction numerator")]
    [Comment("Note duration fraction numerator.")]
    public int Numerator { get; set; }

    /// <summary>
    /// Note duration fraction denominator.
    /// </summary>
    [Column("denominator")]
    [Display(Name = "Note duration fraction denominator")]
    [Comment("Note duration fraction denominator.")]
    public int Denominator { get; set; }

    /// <summary>
    /// Flag indicating if note is a part of triplet (tuplet).
    /// </summary>
    [Column("triplet")]
    [Display(Name = "Flag indicating if note is a part of triplet (tuplet)")]
    [Comment("Flag indicating if note is a part of triplet (tuplet).")]
    public bool Triplet { get; set; }

    /// <summary>
    /// Note tie type enum numeric value.
    /// </summary>
    [Column("tie")]
    [Comment("Note tie type enum numeric value.")]
    public Tie Tie { get; set; }

    [ForeignKey("NoteId")]
    [InverseProperty("Notes")]
    public virtual ICollection<Pitch> Pitches { get; set; } = [];
}
