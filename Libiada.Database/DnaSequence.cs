using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Contains sequences that represent genetic texts (DNA, RNA, gene sequecnes, etc).
/// </summary>
public partial class DnaSequence  
{
    /// <summary>
    /// Unique internal identifier of the sequence.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    public DateTimeOffset Created { get; set; }

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
    /// Id of the sequence in remote database.
    /// </summary>
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Description of the sequence.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Flag indicating whether sequence is partial or complete.
    /// </summary>
    public bool Partial { get; set; }

    public virtual Matter Matter { get; set; } = null!;
}
