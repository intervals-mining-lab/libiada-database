namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core.SimpleTypes;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Note's pitch.
/// </summary>
[Table("pitch")]
[Index("Octave", "Instrument", "Accidental", "NoteSymbol", Name = "uk_pitch", IsUnique = true)]
[Comment("Note's pitch.")]
public partial class Pitch
{
    /// <summary>
    /// Unique internal identifier of the pitch.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier of the pitch.")]
    public int Id { get; set; }

    /// <summary>
    /// Octave number.
    /// </summary>
    [Column("octave")]
    [Display(Name = "Octave number")]
    [Comment("Octave number.")]
    public int Octave { get; set; }

    /// <summary>
    /// Unique number by midi standard.
    /// </summary>
    [Column("midinumber")]
    [Display(Name = " Note hight as midinumber standard")]
    [Comment("Unique number by midi standard.")]
    public int Midinumber { get; set; }

    /// <summary>
    /// Pitch instrument enum numeric value.
    /// </summary>
    [Column("instrument")]
    [Comment("Pitch instrument enum numeric value.")]
    public Instrument Instrument { get; set; }

    /// <summary>
    /// Pitch key signature enum numeric value.
    /// </summary>
    [Column("accidental")]
    [Comment("Pitch key signature enum numeric value.")]
    public Accidental Accidental { get; set; }

    /// <summary>
    /// Note symbol enum numeric value.
    /// </summary>
    [Column("note_symbol")]
    [Display(Name = "Note Symbol")]
    [Comment("Note symbol enum numeric value.")]
    public NoteSymbol NoteSymbol { get; set; }

    [ForeignKey("PitchId")]
    [InverseProperty("Pitches")]
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
}
