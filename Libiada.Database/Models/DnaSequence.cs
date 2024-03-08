namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains sequences that represent genetic texts (DNA, RNA, gene sequecnes, etc).
/// </summary>
[Table("dna_chain")]
[Index("MatterId", Name = "ix_dna_chain_matter_id")]
[Index("Notation", Name = "ix_dna_chain_notation_id")]
[Index("MatterId", "Notation", Name = "uk_dna_chain", IsUnique = true)]
[Comment("Contains sequences that represent genetic texts (DNA, RNA, gene sequecnes, etc).")]
public partial class DnaSequence
{
    /// <summary>
    /// Unique internal identifier of the sequence.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier of the sequence.")]
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    [Column("notation")]
    [Display(Name = "Notation of elements in sequence")]
    [Comment("Notation enum numeric value.")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Sequence creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    [Column("matter_id")]
    [Display(Name = "Sequence belonges to research object")]
    [Comment("Id of the research object to which the sequence belongs.")]
    public long MatterId { get; set; }

    /// <summary>
    /// Sequence's alphabet (array of elements ids).
    /// </summary>
    [Column("alphabet")]
    [Comment("Sequence's alphabet (array of elements ids).")]
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence's order.
    /// </summary>
    [Column("building")]
    [Comment("Sequence's order.")]
    public List<int> Order { get; set; } = null!;

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
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Description of the sequence.
    /// </summary>
    [Column("description")]
    [Comment("Description of the sequence.")]
    public string? Description { get; set; }

    /// <summary>
    /// Flag indicating whether sequence is partial or complete.
    /// </summary>
    [Column("partial")]
    [Display(Name = "Sequence is partial (incomplete)")]
    [Comment("Flag indicating whether sequence is partial or complete.")]
    public bool Partial { get; set; }

    [ForeignKey("MatterId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    [InverseProperty("DnaSequence")]
    public virtual Matter Matter { get; set; } = null!;
}
