namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Music;

/// <summary>
/// Contains sequences that represent musical works in form of note, fmotive or measure sequences.
/// </summary>
[NotMapped]
public class MusicSequence : AbstractCombinedSequence
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

    public static MusicSequence FromCombinedSequence(CombinedSequenceEntity combinedSequence) => new()
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
        SequentialTransfer = combinedSequence.SequentialTransfer ?? throw new Exception("Music sequence sequential transfer is not present in form data"),
        PauseTreatment = combinedSequence.PauseTreatment ?? throw new Exception("Music sequence pause treatment is not present in form data")
    };
}
