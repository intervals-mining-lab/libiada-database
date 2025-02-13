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
[Comment("Contains information on additional fragment positions (for subsequences concatenated from several parts).")]
public partial class Position
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    [Comment("Unique identifier.")]
    public long Id { get; set; }

    /// <summary>
    /// Parent subsequence id.
    /// </summary>
    [Column("subsequence_id")]
    [Comment("Parent subsequence id.")]
    public long SubsequenceId { get; set; }

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

    [ForeignKey(nameof(SubsequenceId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    [InverseProperty("Positions")]
    public virtual Subsequence Subsequence { get; set; } = null!;
}
