
namespace Libiada.Database.Models;

using Libiada.Core.Music;

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


/// <summary>
/// Base table for all sequences that are stored in the database as alphabet and order.
/// </summary>
[Table("sequence")]
[Index("MatterId", Name = "ix_sequence_research_object_id")]
[Index("Notation", Name = "ix_sequence_notation_id")]
[Index("MatterId", "Language", Name = "ix_sequence_research_object_id_language")]

// TODO: add separate indexes with filters
[Index("MatterId", "Notation", "PauseTreatment", "SequentialTransfer", "Language", "Translator", Name = "uk_sequence", IsUnique = true)]
[Comment("Base table for all sequences that are stored in the database as alphabet and order.")]
public class CombinedSequenceEntity : AbstractSequenceEntity
{
    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    [Column("research_object_id")]
    [Display(Name = "Research object to which the sequence belongs")]
    [Comment("Id of the research object to which the sequence belongs.")]
    public long MatterId { get; set; }

    /// <summary>
    /// Sequence nature enum numeric value.
    /// </summary>
    [Column("nature")]
    [Comment("Sequence nature enum numeric value.")]
    public Nature Nature { get; set; }

    /// <summary>
    /// Notation of the sequence (words, letters, notes, nucleotides, etc.).
    /// </summary>
    [Column("notation")]
    [Display(Name = "Notation of elements in the sequence")]
    [Comment("Notation of the sequence (words, letters, notes, nucleotides, etc.).")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence's alphabet (array of elements ids).
    /// </summary>
    [Column("alphabet")]
    [Comment("Sequence's alphabet (array of elements ids).")]
    public long[] Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence's order.
    /// </summary>
    [Column("order")]
    [Comment("Sequence's order.")]
    public int[] Order { get; set; } = null!;

    /// <summary>
    /// Flag indicating whether sequence is partial or complete.
    /// </summary>
    [Column("partial")]
    [Display(Name = "Sequence is partial (incomplete)")]
    [Comment("Flag indicating whether sequence is partial or complete.")]
    public bool? Partial { get; set; }

    /// <summary>
    /// Flag indicating if this sequence is in original language or was translated.
    /// </summary>
    [Column("original")]
    [Display(Name = "Literary work is in original language (not in translation)")]
    [Comment("Flag indicating if this sequence is in original language or was translated.")]
    public bool? Original { get; set; }

    /// <summary>
    /// Primary language of literary work.
    /// </summary>
    [Column("language")]
    [Comment("Primary language of literary work.")]
    public Language? Language { get; set; }

    /// <summary>
    /// Author of translation or automated translator.
    /// </summary>
    [Column("translator")]
    [Comment("Author of translation or automated translator.")]
    public Translator? Translator { get; set; }

    /// <summary>
    /// Pause treatment enum numeric value.
    /// </summary>
    [Column("pause_treatment")]
    [Display(Name = "Pause treatment")]
    [Comment("Pause treatment enum numeric value.")]
    public PauseTreatment? PauseTreatment { get; set; }

    /// <summary>
    /// Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.
    /// </summary>
    [Column("sequential_transfer")]
    [Display(Name = "Sequential transfer is used")]
    [Comment("Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.")]
    public bool? SequentialTransfer { get; set; }

    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Matter Matter { get; set; } = null!;

    public MusicSequence ToMusicSequence() => new()
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
        SequentialTransfer = SequentialTransfer ?? throw new Exception("Music sequence sequential transfer is not present in form data"),
        PauseTreatment = PauseTreatment ?? throw new Exception("Music sequence pause treatment is not present in form data")
    };

    public GeneticSequence ToGeneticSequence() => new()
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
        Partial = Partial ?? throw new Exception("Genetic sequence partial flag is not present in form data")
    };

    public LiteratureSequence ToLiteratureSequence() => new()
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
        Original = Original ?? throw new Exception("Literature sequence original flag is not present in form data"),
        Language = Language ?? throw new Exception("Literature sequence language is not present in form data"),
        Translator = Translator ?? throw new Exception("Literature sequence translator is not present in form data")
    };
}
