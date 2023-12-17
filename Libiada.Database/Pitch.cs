using System;
using System.Collections.Generic;
using LibiadaCore.Core.SimpleTypes;


namespace Libiada.Database;

/// <summary>
/// Note&apos;s pitch.
/// </summary>
public partial class Pitch  
{
    /// <summary>
    /// Unique internal identifier of the pitch.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Octave number.
    /// </summary>
    public int Octave { get; set; }

    /// <summary>
    /// Unique number by midi standard.
    /// </summary>
    public int Midinumber { get; set; }

    /// <summary>
    /// Pitch instrument enum numeric value.
    /// </summary>
    public Instrument Instrument { get; set; }

    /// <summary>
    /// Pitch key signature enum numeric value.
    /// </summary>
    public Accidental Accidental { get; set; }

    /// <summary>
    /// Note symbol enum numeric value.
    /// </summary>
    public NoteSymbol NoteSymbol { get; set; }

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
}
