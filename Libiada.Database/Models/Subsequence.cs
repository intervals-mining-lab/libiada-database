namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information on location and length of the fragments within complete sequences.
/// </summary>
[Table("subsequence")]
[Index("SequenceId", "Feature", Name = "ix_subsequence_chain_feature")]
[Index("SequenceId", Name = "ix_subsequence_chain_id")]
[Comment("Contains information on location and length of the fragments within complete sequences.")]
public partial class Subsequence
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public long Id { get; set; }

    /// <summary>
    /// Subsequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Subsequence creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Parent sequence id.
    /// </summary>
    [Column("chain_id")]
    [Comment("Parent sequence id.")]
    public long SequenceId { get; set; }

    /// <summary>
    /// Index of the fragment beginning (from zero).
    /// </summary>
    [Column("start")]
    [Comment("Index of the fragment beginning (from zero).")]
    public int Start { get; set; }

    /// <summary>
    /// Fragment length.
    /// </summary>
    [Column("length")]
    [Comment("Fragment length.")]
    public int Length { get; set; }

    /// <summary>
    /// Subsequence feature enum numeric value.
    /// </summary>
    [Column("feature")]
    [Comment("Subsequence feature enum numeric value.")]
    public Feature Feature { get; set; }

    /// <summary>
    /// Flag indicating whether subsequence is partial or complete.
    /// </summary>
    [Column("partial")]
    [Comment("Flag indicating whether subsequence is partial or complete.")]
    public bool Partial { get; set; }

    /// <summary>
    /// Id of the subsequence in the remote database (ncbi or same as paren sequence remote db).
    /// </summary>
    [Column("remote_id")]
    [StringLength(255)]
    [Comment("Id of the subsequence in the remote database (ncbi or same as paren sequence remote db).")]
    public string? RemoteId { get; set; }

    [InverseProperty("Subsequence")]
    public virtual ICollection<Position> Position { get; set; } = new List<Position>();

    [InverseProperty("Subsequence")]
    public virtual ICollection<SequenceAttribute> SequenceAttribute { get; set; } = new List<SequenceAttribute>();
}
