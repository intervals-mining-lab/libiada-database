namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core.SimpleTypes;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains elements that represent note sequences in form of formal motifs that are used as elements of segmented music sequences.
/// </summary>
[Table("fmotif")]
[Comment("Contains elements that represent note sequences in form of formal motifs that are used as elements of segmented music sequences.")]
public partial class Fmotif
{
    /// <summary>
    /// Unique internal identifier of the fmotif.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier of the fmotif.")]
    public long Id { get; set; }

    /// <summary>
    /// Fmotif hash value.
    /// </summary>
    [Column("value")]
    [StringLength(255)]
    [Comment("Fmotif hash value.")]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Fmotif description.
    /// </summary>
    [Column("description")]
    [Comment("Fmotif description.")]
    public string? Description { get; set; }

    /// <summary>
    /// Fmotif name.
    /// </summary>
    [Column("name")]
    [StringLength(255)]
    [Comment("Fmotif name.")]
    public string? Name { get; set; }

    /// <summary>
    /// Fmotif notation enum numeric value (always 6).
    /// </summary>
    [Column("notation")]
    [Comment("Fmotif notation enum numeric value (always 6).")]
    public Notation Notation { get; } = Notation.FormalMotifs;

    /// <summary>
    /// Fmotif creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Fmotif creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Fmotif's alphabet (array of notes ids).
    /// </summary>
    [Column("alphabet")]
    [Comment("Fmotif's alphabet (array of notes ids).")]
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Fmotif's order.
    /// </summary>
    [Column("building")]
    [Comment("Fmotif's order.")]
    public List<int> Order { get; set; } = null!;

    /// <summary>
    /// Fmotif type enum numeric value.
    /// </summary>
    [Column("fmotif_type")]
    [Display(Name = "Fmotif type")]
    [Comment("Fmotif type enum numeric value.")]
    public FmotifType FmotifType { get; set; }
}
