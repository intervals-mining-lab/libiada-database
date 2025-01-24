namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations.Schema;


/// <summary>
/// Contains sequences that represent time series and other ordered data arrays.
/// </summary>
[NotMapped]
public class DataSequence : AbstractCombinedSequence
{
    public CombinedSequenceEntity ToCombinedSequence() => new()
    {
        Id = Id,
        Alphabet = Alphabet,
        Order = Order,
        Notation = Notation,
        ResearchObjectId = ResearchObjectId,
        RemoteDb = RemoteDb,
        RemoteId = RemoteId,
        ResearchObject = ResearchObject,
        CreatorId = CreatorId,
        ModifierId = ModifierId
    };
}
