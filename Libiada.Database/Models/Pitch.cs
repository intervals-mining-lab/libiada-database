namespace Libiada.Database.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core.SimpleTypes;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Note&apos;s pitch.
/// </summary>
[Table("pitch")]
[Index("Octave", "Instrument", "Accidental", "NoteSymbol", Name = "uk_pitch", IsUnique = true)]
public partial class Pitch
{
    /// <summary>
    /// Unique internal identifier of the pitch.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Octave number.
    /// </summary>
    [Column("octave")]
    [Display(Name = "Octave number")]
    public int Octave { get; set; }

    /// <summary>
    /// Unique number by midi standard.
    /// </summary>
    [Column("midinumber")]
    [Display(Name = " Note hight as midinumber standard")]
    [Description("Unique number by midi standard.")]
    public int Midinumber { get; set; }

    /// <summary>
    /// Pitch instrument enum numeric value.
    /// </summary>
    [Column("instrument")]
    public Instrument Instrument { get; set; }

    /// <summary>
    /// Pitch key signature enum numeric value.
    /// </summary>
    [Column("accidental")]
    public Accidental Accidental { get; set; }

    /// <summary>
    /// Note symbol enum numeric value.
    /// </summary>
    [Column("note_symbol")]
    [Display(Name = "Note Symbol")]
    public NoteSymbol NoteSymbol { get; set; }

    [ForeignKey("PitchId")]
    [InverseProperty("Pitches")]
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
}
