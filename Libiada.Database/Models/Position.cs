namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information on additional fragment positions (for subsequences concatenated from several parts).
/// </summary>
[Table("position")]
[Index("SubsequenceId", Name = "ix_position_subsequence_id")]
[Index("SubsequenceId", "Start", "Length", Name = "uk_piece", IsUnique = true)]
public partial class Position
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Parent subsequence id.
    /// </summary>
    [Column("subsequence_id")]
    public long SubsequenceId { get; set; }

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

    [ForeignKey("SubsequenceId")]
    [InverseProperty("Position")]
    public virtual Subsequence Subsequence { get; set; } = null!;
}
