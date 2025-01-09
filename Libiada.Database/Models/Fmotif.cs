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
public partial class Fmotif : Element
{
    /// <summary>
    /// Fmotif's alphabet (array of notes ids).
    /// </summary>
    [Column("alphabet")]
    [Comment("Fmotif's alphabet (array of notes ids).")]
    public long[] Alphabet { get; set; } = null!;

    /// <summary>
    /// Fmotif's order.
    /// </summary>
    [Column("order")]
    [Comment("Fmotif's order.")]
    public int[] Order { get; set; } = null!;

    /// <summary>
    /// Fmotif type enum numeric value.
    /// </summary>
    [Column("fmotif_type")]
    [Display(Name = "Fmotif type")]
    [Comment("Fmotif type enum numeric value.")]
    public FmotifType FmotifType { get; set; }
}
