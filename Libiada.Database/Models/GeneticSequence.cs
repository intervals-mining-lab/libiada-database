﻿namespace Libiada.Database.Models;

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

    public CombinedSequenceEntity ToCombinedSequence() => new()
    {
        Id = Id,
        Alphabet = Alphabet,
        Order = Order,
        Nature = Nature.Genetic,
        Notation = Notation,
        ResearchObjectId = ResearchObjectId,
        RemoteDb = RemoteDb,
        RemoteId = RemoteId,
        ResearchObject = ResearchObject,
        CreatorId = CreatorId,
        ModifierId = ModifierId,
        Partial = Partial,
    };
}
