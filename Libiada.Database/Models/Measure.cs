namespace Libiada.Database.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains elements that represent note sequences in form of measures (bars) that are used as elements of segmented music sequences.
/// </summary>
[Table("measure")]
[Index("Notation", Name = "ix_measure_notation_id")]
public partial class Measure
{
    /// <summary>
    /// Unique internal identifier of the measure.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Measure hash code.
    /// </summary>
    [Column("value")]
    [StringLength(255)]
    public string? Value { get; set; }

    /// <summary>
    /// Description of the sequence.
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Measure name.
    /// </summary>
    [Column("name")]
    [StringLength(255)]
    public string? Name { get; set; }

    /// <summary>
    /// Measure notation enum numeric value (always 7).
    /// </summary>
    [Column("notation")]
    public Notation Notation { get; } = Notation.Measures;

    /// <summary>
    /// Measure creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Measure alphabet (array of notes ids).
    /// </summary>
    [Column("alphabet")]
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Measure order.
    /// </summary>
    [Column("building")]
    public List<int> Order { get; set; } = null!;

    /// <summary>
    /// Time signature upper numeral (Beat numerator).
    /// </summary>
    [Column("beats")]
    [Display(Name = "Time signature upper numeral (Beat numerator)")]
    public int Beats { get; set; }

    /// <summary>
    /// Time signature lower numeral (Beat denominator).
    /// </summary>
    [Column("beatbase")]
    [Display(Name = "Time signature lower numeral (Beat denominator)")]
    public int Beatbase { get; set; }

    /// <summary>
    /// Key signature of the measure (negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis)).
    /// </summary>
    [Column("fifths")]
    [Display(Name = "Clef (key signature) of the measure")]
    [Description("Negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis).")]
    public int Fifths { get; set; }

    /// <summary>
    /// Music mode of the measure. true  represents major and false represents minor.
    /// </summary>
    [Column("major")]
    [Display(Name = "Music mode of the measure")]
    public bool Major { get; set; }
}
