namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[NotMapped]
public abstract class AbstractCombinedSequence
{
    /// <summary>
    /// Unique identifier of the sequence used in other tables. Surrogate for foreign keys.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Id of the sequence in remote database.
    /// </summary>
    [StringLength(255)]
    [Display(Name = "Id in remote database")]
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    [Display(Name = "Remote database")]
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record creator user id.
    /// </summary>
    [Display(Name = "Creator")]
    public int CreatorId { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Record editor user id.
    /// </summary>
    [Display(Name = "Modifier")]
    public int ModifierId { get; set; }

    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    [Display(Name = "Research object to which the sequence belongs")]
    public long ResearchObjectId { get; set; }

    /// <summary>
    /// Notation of the sequence (words, letters, notes, nucleotides, etc.).
    /// </summary>
    [Display(Name = "Notation of elements in the sequence")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence's alphabet (array of elements ids).
    /// </summary>
    public long[] Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence's order.
    /// </summary>
    public int[] Order { get; set; } = null!;

    public virtual ResearchObject ResearchObject { get; set; } = null!;
}
