
namespace Libiada.Database.Models;

using Libiada.Core.Music;

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


/// <summary>
/// Base table for all sequences that are stored in the database as alphabet and order.
/// </summary>
[Table("sequence")]
[Index(nameof(ResearchObjectId), Name = "ix_sequence_research_object_id")]
[Index("Notation", Name = "ix_sequence_notation_id")]
[Index(nameof(ResearchObjectId), "Language", Name = "ix_sequence_research_object_id_language")]

// TODO: add separate indexes with filters
[Index(nameof(ResearchObjectId), "Notation", "PauseTreatment", "SequentialTransfer", "Language", "Translator", Name = "uk_sequence", IsUnique = true)]
[Comment("Base table for all sequences that are stored in the database as alphabet and order.")]
public class CombinedSequenceEntity : AbstractSequenceEntity
{
    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    [Column("research_object_id")]
    [Display(Name = "Research object to which the sequence belongs")]
    [Comment("Id of the research object to which the sequence belongs.")]
    public long ResearchObjectId { get; set; }

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
    public virtual ResearchObject ResearchObject { get; set; } = null!;

    public static CombinedSequenceEntity FromMusicSequence(MusicSequence musicSequence) => new()
    {
        Id = musicSequence.Id,
        Alphabet = musicSequence.Alphabet,
        Order = musicSequence.Order,
        Nature = Nature.Music,
        Notation = musicSequence.Notation,
        ResearchObjectId = musicSequence.ResearchObjectId,
        RemoteDb = musicSequence.RemoteDb,
        RemoteId = musicSequence.RemoteId,
        ResearchObject = musicSequence.ResearchObject,
        CreatorId = musicSequence.CreatorId,
        Created = musicSequence.Created,
        ModifierId = musicSequence.ModifierId,
        Modified = musicSequence.Modified,
        SequentialTransfer = musicSequence.SequentialTransfer,
        PauseTreatment = musicSequence.PauseTreatment
    };

    public static CombinedSequenceEntity FromGeneticSequence(GeneticSequence geneticSequence) => new()
    {
        Id = geneticSequence.Id,
        Alphabet = geneticSequence.Alphabet,
        Order = geneticSequence.Order,
        Nature = Nature.Genetic,
        Notation = geneticSequence.Notation,
        ResearchObjectId = geneticSequence.ResearchObjectId,
        RemoteDb = geneticSequence.RemoteDb,
        RemoteId = geneticSequence.RemoteId,
        ResearchObject = geneticSequence.ResearchObject,
        CreatorId = geneticSequence.CreatorId,
        Created = geneticSequence.Created,
        ModifierId = geneticSequence.ModifierId,
        Modified = geneticSequence.Modified,
        Partial = geneticSequence.Partial,
    };

    public static CombinedSequenceEntity FromLiteratureSequence(LiteratureSequence literatureSequence) => new()
    {
        Id = literatureSequence.Id,
        Alphabet = literatureSequence.Alphabet,
        Order = literatureSequence.Order,
        Nature = Nature.Literature,
        Notation = literatureSequence.Notation,
        ResearchObjectId = literatureSequence.ResearchObjectId,
        RemoteDb = literatureSequence.RemoteDb,
        RemoteId = literatureSequence.RemoteId,
        ResearchObject = literatureSequence.ResearchObject,
        CreatorId = literatureSequence.CreatorId,
        Created = literatureSequence.Created,
        ModifierId = literatureSequence.ModifierId,
        Modified = literatureSequence.Modified,
        Original = literatureSequence.Original,
        Language = literatureSequence.Language,
        Translator = literatureSequence.Translator
    };
}
