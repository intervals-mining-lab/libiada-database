namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information on location and length of the fragments within complete sequences.
/// </summary>
[Table("subsequence")]
[Index("SequenceId","Notation", "Feature", Name = "ix_subsequence_sequence_notation_feature")]
[Index("SequenceId", Name = "ix_subsequence_sequence_id")]
[Comment("Contains information on location and length of the fragments within complete sequences.")]
public partial class Subsequence : AbstractSequenceEntity
{
    /// <summary>
    /// Parent sequence id.
    /// </summary>
    [Column("sequence_id")]
    [Display(Name = "Parent sequence")]
    [Comment("Parent sequence id.")]
    public long SequenceId { get; set; }

    /// <summary>
    /// Notation of the subsequence (words, letters, notes, nucleotides, etc.).
    /// </summary>
    [Column("notation")]
    [Display(Name = "Notation of elements in the subsequence")]
    [Comment("Notation of the subsequence (nucleotides, triplets or aminoacids).")]
    public Notation Notation { get; set; }

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

    [ForeignKey(nameof(SequenceId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    [InverseProperty("Subsequences")]
    public virtual AbstractSequenceEntity ParentSequence { get; set; } = null!;

    [InverseProperty("Subsequence")]
    public virtual ICollection<Position> Position { get; set; } = [];

    [InverseProperty("Sequence")]
    public virtual ICollection<SequenceAttribute> SequenceAttribute { get; set; } = [];
}
