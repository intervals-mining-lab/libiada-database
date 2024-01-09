using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libiada.Database;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains information on location and length of the fragments within complete sequences.
/// </summary>
[Table("subsequence")]
[Index("SequenceId", "Feature", Name = "ix_subsequence_chain_feature")]
[Index("SequenceId", Name = "ix_subsequence_chain_id")]
public partial class Subsequence  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Sequence group creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Parent sequence id.
    /// </summary>
    [Column("chain_id")]
    public long SequenceId { get; set; }

    /// <summary>
    /// Index of the fragment beginning (from zero).
    /// </summary>
    [Column("start")]
    public int Start { get; set; }

    /// <summary>
    /// Fragment length.
    /// </summary>
    [Column("length")]
    public int Length { get; set; }

    /// <summary>
    /// Subsequence feature enum numeric value.
    /// </summary>
    [Column("feature")]
    public Feature Feature { get; set; }

    /// <summary>
    /// Flag indicating whether subsequence is partial or complete.
    /// </summary>
    [Column("partial")]
    public bool Partial { get; set; }

    /// <summary>
    /// Id of the subsequence in the remote database (ncbi or same as paren sequence remote db).
    /// </summary>
    [Column("remote_id")]
    [StringLength(255)]
    public string? RemoteId { get; set; }

    [InverseProperty("Subsequence")]
    public virtual ICollection<Position> Position { get; set; } = new List<Position>();

    [InverseProperty("Subsequence")]
    public virtual ICollection<SequenceAttribute> SequenceAttribute { get; set; } = new List<SequenceAttribute>();
}
