namespace Libiada.Database.Models;

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Surrogate table that contains keys for all sequences tables and used for foreign key references.
/// </summary>
[Table("abstract_sequence")]
[Comment("Surrogate table that contains keys for all sequences tables and used for foreign key references.")]
public abstract class AbstractSequenceEntity
{
    /// <summary>
    /// Unique identifier of the sequence used in other tables. Surrogate for foreign keys.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    [Comment("Unique identifier of the sequence used in other tables. Surrogate for foreign keys.")]
    public long Id { get; set; }

    /// <summary>
    /// Id of the sequence in remote database.
    /// </summary>
    [Column("remote_id")]
    [StringLength(255)]
    [Display(Name = "Id in remote database")]
    [Comment("Id of the sequence in remote database.")]
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    [Column("remote_db")]
    [Display(Name = "Remote database")]
    [Comment("Enum numeric value of the remote db from which sequence is downloaded.")]
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Sequence creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record creator user id.
    /// </summary>
    [Column("creator_id")]
    [Display(Name = "Creator")]
    [Comment("Record creator user id.")]
    public int CreatorId { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Record editor user id.
    /// </summary>
    [Column("modifier_id")]
    [Display(Name = "Modifier")]
    [Comment("Record editor user id.")]
    public int ModifierId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public virtual AspNetUser? Creator { get; set; }

    [ForeignKey(nameof(ModifierId))]
    public virtual AspNetUser? Modifier { get; set; }

    [InverseProperty("Sequence")]
    public virtual ICollection<SequenceAttribute> SequenceAttributes { get; set; } = [];

    [InverseProperty("ParentSequence")]
    public virtual ICollection<Subsequence> Subsequences { get; set; } = [];
}
