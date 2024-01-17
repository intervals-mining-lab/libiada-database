namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core.SimpleTypes;

/// <summary>
/// Contains elements that represent note sequences in form of formal motifs that are used as elements of segmented music sequences.
/// </summary>
[Table("fmotif")]
public partial class Fmotif
{
    /// <summary>
    /// Unique internal identifier of the fmotif.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Fmotif hash value.
    /// </summary>
    [Column("value")]
    [StringLength(255)]
    public string? Value { get; set; }

    /// <summary>
    /// Fmotif description.
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Fmotif name.
    /// </summary>
    [Column("name")]
    [StringLength(255)]
    public string? Name { get; set; }

    /// <summary>
    /// Fmotif notation enum numeric value (always 6).
    /// </summary>
    [Column("notation")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Fmotif creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Fmotif&apos;s alphabet (array of notes ids).
    /// </summary>
    [Column("alphabet")]
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Fmotif&apos;s order.
    /// </summary>
    [Column("building")]
    public List<int> Order { get; set; } = null!;

    /// <summary>
    /// Fmotif type enum numeric value.
    /// </summary>
    [Column("fmotif_type")]
    [Display(Name = "Fmotif type")]
    public FmotifType FmotifType { get; set; }
}
