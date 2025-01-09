namespace Libiada.Database.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains elements that represent note sequences in form of measures (bars) that are used as elements of segmented music sequences.
/// </summary>
[Table("measure")]
[Comment("Contains elements that represent note sequences in form of measures (bars) that are used as elements of segmented music sequences.")]
public partial class Measure : Element
{
    /// <summary>
    /// Measure alphabet (array of notes ids).
    /// </summary>
    [Column("alphabet")]
    [Comment("Measure alphabet (array of notes ids).")]
    public long[] Alphabet { get; set; } = null!;

    /// <summary>
    /// Measure order.
    /// </summary>
    [Column("order")]
    [Comment("Measure order.")]
    public int[] Order { get; set; } = null!;

    /// <summary>
    /// Time signature upper numeral (Beat numerator).
    /// </summary>
    [Column("beats")]
    [Display(Name = "Time signature upper numeral (Beat numerator)")]
    [Comment("Time signature upper numeral (Beat numerator).")]
    public int Beats { get; set; }

    /// <summary>
    /// Time signature lower numeral (Beat denominator).
    /// </summary>
    [Column("beatbase")]
    [Display(Name = "Time signature lower numeral (Beat denominator)")]
    [Comment("Time signature lower numeral (Beat denominator).")]
    public int Beatbase { get; set; }

    /// <summary>
    /// Key signature of the measure (negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis)).
    /// </summary>
    [Column("fifths")]
    [Display(Name = "Clef (key signature) of the measure")]
    [Description("Negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis).")]
    [Comment("Key signature of the measure (negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis)).")]
    public int Fifths { get; set; }

    /// <summary>
    /// Music mode of the measure. true  represents major and false represents minor.
    /// </summary>
    [Column("major")]
    [Display(Name = "Music mode of the measure")]
    [Comment("Music mode of the measure. true  represents major and false represents minor.")]
    public bool Major { get; set; }
}
