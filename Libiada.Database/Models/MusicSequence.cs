namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Music;

/// <summary>
/// Contains sequences that represent musical works in form of note, fmotive or measure sequences.
/// </summary>
[NotMapped]
public class MusicSequence :  AbstractCombinedSequence
{
    /// <summary>
    /// Pause treatment enum numeric value.
    /// </summary>
    [Display(Name = "Pause treatment")]
    public PauseTreatment PauseTreatment { get; set; }

    /// <summary>
    /// Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.
    /// </summary>
    [Display(Name = "Sequential transfer is used")]
    public bool SequentialTransfer { get; set; }

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
        SequentialTransfer = SequentialTransfer,
        PauseTreatment = PauseTreatment
    };
}
