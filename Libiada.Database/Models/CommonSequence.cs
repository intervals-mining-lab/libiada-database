namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Base table for all sequences that are stored in the database as alphabet and order.
/// </summary>
[Table("chain")]
[Index("MatterId", Name = "ix_chain_matter_id")]
[Index("Notation", Name = "ix_chain_notation_id")]
public partial class CommonSequence
{
    /// <summary>
    /// Unique internal identifier of the sequence.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Notation of the sequence (words, letters, notes, nucleotides, etc.).
    /// </summary>
    [Column("notation")]
    [Display(Name = "Notation of elements in the sequence")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    [Column("matter_id")]
    [Display(Name = "Sequence belonges to research object")]
    public long MatterId { get; set; }

    /// <summary>
    /// Sequence&apos;s alphabet (array of elements ids).
    /// </summary>
    [Column("alphabet")]
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence&apos;s order.
    /// </summary>
    [Column("building")]
    public List<int> Order { get; set; } = null!;

    /// <summary>
    /// Id of the sequence in remote database.
    /// </summary>
    [Column("remote_id")]
    [StringLength(255)]
    [Display(Name = "Id in remote database")]
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    [Column("remote_db")]
    [Display(Name = "Remote database")]
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Description of the sequence.
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    [ForeignKey("MatterId")]
    [InverseProperty("Sequence")]
    public virtual Matter Matter { get; set; } = null!;
}
