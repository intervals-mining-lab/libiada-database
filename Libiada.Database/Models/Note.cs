namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core.SimpleTypes;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains elements that represent notes that are used as elements of music sequences.
/// </summary>
[Table("note")]
[Index("Notation", Name = "ix_note_notation_id")]
[Index("Value", Name = "uk_note", IsUnique = true)]
[Comment("Contains elements that represent notes that are used as elements of music sequences.")]
public partial class Note
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public long Id { get; set; }

    /// <summary>
    /// Note hash value.
    /// </summary>
    [Column("value")]
    [StringLength(255)]
    [Comment("Note hash value.")]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Note description.
    /// </summary>
    [Column("description")]
    [Comment("Note description.")]
    public string? Description { get; set; }

    /// <summary>
    /// Note name.
    /// </summary>
    [Column("name")]
    [StringLength(255)]
    [Comment("Note name.")]
    public string? Name { get; set; }

    /// <summary>
    /// Note notation enum numeric value (always 8).
    /// </summary>
    [Column("notation")]
    [Comment("Note notation enum numeric value (always 8).")]
    public Notation Notation { get; } = Notation.Notes;

    /// <summary>
    /// Note creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Note creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

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
    public virtual ICollection<Pitch> Pitches { get; set; } = new List<Pitch>();
}
