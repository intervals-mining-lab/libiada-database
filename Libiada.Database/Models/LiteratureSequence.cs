namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Contains sequences that represent literary works and their various translations.
/// </summary>
[NotMapped]
public class LiteratureSequence : AbstractCombinedSequence
{

    /// <summary>
    /// Flag indicating if this sequence is in original language or was translated.
    /// </summary>
    [Display(Name = "Literary work is in original language (not in translation)")]
    public bool Original { get; set; }

    /// <summary>
    /// Primary language of literary work.
    /// </summary>
    public Language Language { get; set; }

    /// <summary>
    /// Author of translation or automated translator.
    /// </summary>
    public Translator Translator { get; set; }

    public static LiteratureSequence FromCombinedSequence(CombinedSequenceEntity combinedSequenceEntity) => new()
    {
        Id = combinedSequenceEntity.Id,
        Alphabet = combinedSequenceEntity.Alphabet,
        Order = combinedSequenceEntity.Order,
        Notation = combinedSequenceEntity.Notation,
        ResearchObjectId = combinedSequenceEntity.ResearchObjectId,
        RemoteDb = combinedSequenceEntity.RemoteDb,
        RemoteId = combinedSequenceEntity.RemoteId,
        ResearchObject = combinedSequenceEntity.ResearchObject,
        CreatorId = combinedSequenceEntity.CreatorId,
        Created = combinedSequenceEntity.Created,
        ModifierId = combinedSequenceEntity.ModifierId,
        Modified = combinedSequenceEntity.Modified,
        Original = combinedSequenceEntity.Original ?? throw new Exception("Literature sequence original flag is not present in form data"),
        Language = combinedSequenceEntity.Language ?? throw new Exception("Literature sequence language is not present in form data"),
        Translator = combinedSequenceEntity.Translator ?? throw new Exception("Literature sequence translator is not present in form data")
    };
}
