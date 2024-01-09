using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libiada.Database;
using LibiadaCore.Music;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains sequences that represent musical works in form of note, fmotive or measure sequences.
/// </summary>
[Table("music_chain")]
[Index("MatterId", Name = "ix_music_chain_matter_id")]
[Index("Notation", Name = "ix_music_chain_notation_id")]
[Index("MatterId", "Notation", "PauseTreatment", "SequentialTransfer", Name = "uk_music_chain", IsUnique = true)]
public partial class MusicSequence  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    [Column("notation")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    [Column("matter_id")]
    public long MatterId { get; set; }

    /// <summary>
    /// Sequence&apos;s alphabet (array of elements ids).
    /// </summary>
    [Column("alphabet")]
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence&apos;s order.
    /// </summary>
    [Column("building")]
    public List<int> Building { get; set; } = null!;

    /// <summary>
    /// Id of the sequence in the remote database.
    /// </summary>
    [Column("remote_id")]
    [StringLength(255)]
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    [Column("remote_db")]
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Sequence description.
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Pause treatment enum numeric value.
    /// </summary>
    [Column("pause_treatment")]
    public PauseTreatment PauseTreatment { get; set; }

    /// <summary>
    /// Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.
    /// </summary>
    [Column("sequential_transfer")]
    public bool SequentialTransfer { get; set; }

    [ForeignKey("MatterId")]
    [InverseProperty("MusicSequence")]
    public virtual Matter Matter { get; set; } = null!;
}
