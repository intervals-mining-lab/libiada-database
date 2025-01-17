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
        MatterId = MatterId,
        RemoteDb = RemoteDb,
        RemoteId = RemoteId,
        Matter = Matter,
        CreatorId = CreatorId,
        ModifierId = ModifierId
    };
}
