namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Contains sequences that represent genetic texts (DNA, RNA, gene sequecnes, etc).
/// </summary>
[NotMapped]
public class GeneticSequence : AbstractCombinedSequence
{
    /// <summary>
    /// Flag indicating whether sequence is partial or complete.
    /// </summary>
    [Display(Name = "Sequence is partial (incomplete)")]
    public bool Partial { get; set; }

    public static GeneticSequence FromCombinedSequence(CombinedSequenceEntity combinedSequence) => new()
    {
        Id = combinedSequence.Id,
        Alphabet = combinedSequence.Alphabet,
        Order = combinedSequence.Order,
        Notation = combinedSequence.Notation,
        ResearchObjectId = combinedSequence.ResearchObjectId,
        RemoteDb = combinedSequence.RemoteDb,
        RemoteId = combinedSequence.RemoteId,
        ResearchObject = combinedSequence.ResearchObject,
        CreatorId = combinedSequence.CreatorId,
        Created = combinedSequence.Created,
        ModifierId = combinedSequence.ModifierId,
        Modified = combinedSequence.Modified,
        Partial = combinedSequence.Partial ?? throw new Exception("Genetic sequence partial flag is not present in form data")
    };
}
