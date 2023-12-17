using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Contains information on location and length of the fragments within complete sequences.
/// </summary>
public partial class Subsequence  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Sequence group creation date and time (filled trough trigger).
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTime Modified { get; set; }

    /// <summary>
    /// Parent sequence id.
    /// </summary>
    public long SequenceId { get; set; }

    /// <summary>
    /// Index of the fragment beginning (from zero).
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Fragment length.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Subsequence feature enum numeric value.
    /// </summary>
    public Feature Feature { get; set; }

    /// <summary>
    /// Flag indicating whether subsequence is partial or complete.
    /// </summary>
    public bool Partial { get; set; }

    /// <summary>
    /// Id of the subsequence in the remote database (ncbi or same as paren sequence remote db).
    /// </summary>
    public string? RemoteId { get; set; }

    public virtual ICollection<Position> Position { get; set; } = new List<Position>();

    public virtual ICollection<SequenceAttribute> SequenceAttribute { get; set; } = new List<SequenceAttribute>();
}
