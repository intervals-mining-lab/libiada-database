using System;
using System.Collections.Generic;
using Libiada.Database;
using LibiadaCore.Music;


namespace Libiada.Database;

/// <summary>
/// Contains sequences that represent musical works in form of note, fmotive or measure sequences.
/// </summary>
public partial class MusicSequence  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    public long MatterId { get; set; }

    /// <summary>
    /// Sequence&apos;s alphabet (array of elements ids).
    /// </summary>
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence&apos;s order.
    /// </summary>
    public List<int> Building { get; set; } = null!;

    /// <summary>
    /// Id of the sequence in the remote database.
    /// </summary>
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTime Modified { get; set; }

    /// <summary>
    /// Sequence description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Pause treatment enum numeric value.
    /// </summary>
    public PauseTreatment PauseTreatment { get; set; }

    /// <summary>
    /// Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.
    /// </summary>
    public bool SequentialTransfer { get; set; }

    public virtual Matter Matter { get; set; } = null!;
}
