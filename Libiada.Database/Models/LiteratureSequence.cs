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

    public CombinedSequenceEntity ToCombinedSequence() => new()
    {
        Id = Id,
        Alphabet = Alphabet,
        Order = Order,
        Nature = Nature.Literature,
        Notation = Notation,
        ResearchObjectId = ResearchObjectId,
        RemoteDb = RemoteDb,
        RemoteId = RemoteId,
        ResearchObject = ResearchObject,
        CreatorId = CreatorId,
        ModifierId = ModifierId,
        Original = Original,
        Language = Language,
        Translator = Translator
    };
}
