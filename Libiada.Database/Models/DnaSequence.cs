namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Contains sequences that represent genetic texts (DNA, RNA, gene sequecnes, etc).
/// </summary>
[NotMapped]
public class DnaSequence : AbstractCombinedSequence
{
    /// <summary>
    /// Flag indicating whether sequence is partial or complete.
    /// </summary>
    [Display(Name = "Sequence is partial (incomplete)")]
    public bool Partial { get; set; }

    public CombinedSequenceEntity ToCombinedSequence() => new()
    {
        Id = Id,
        Alphabet = Alphabet,
        Order = Order,
        Notation = Notation,
        MatterId = MatterId,
        RemoteDb = RemoteDb,
        RemoteId = RemoteId,
        Matter = Matter,
        CreatorId = CreatorId,
        ModifierId = ModifierId,
        Partial = Partial,
    };
}
