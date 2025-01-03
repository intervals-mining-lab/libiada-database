namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains research objects, samples, texts, etc (one research object may be represented by several sequences).
/// </summary>
[Table("matter")]
[Index("Nature", Name = "ix_matter_nature")]
[Index("Name", "Nature", Name = "uk_matter", IsUnique = true)]
[Index("MultisequenceId", "MultisequenceNumber", Name = "uk_matter_multisequence", IsUnique = true)]
[Comment("Contains research objects, samples, texts, etc (one research object may be represented by several sequences).")]
public partial class Matter
{
    /// <summary>
    /// Unique internal identifier of the research object.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    [Comment("Unique identifier of the research object.")]
    public long Id { get; set; }

    /// <summary>
    /// Research object name.
    /// </summary>
    [Column("name")]
    [Comment("Research object name.")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Research object nature enum numeric value.
    /// </summary>
    [Column("nature")]
    [Comment("Research object nature enum numeric value.")]
    public Nature Nature { get; set; }

    /// <summary>
    /// Description of the research object.
    /// </summary>
    [Column("description")]
    [Comment("Description of the research object.")]
    public string? Description { get; set; }

    /// <summary>
    /// Record creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Sequence type enum numeric value.
    /// </summary>
    [Column("sequence_type")]
    [Display(Name = "Sequence type")]
    [Comment("Sequence type enum numeric value.")]
    public SequenceType SequenceType { get; set; }

    /// <summary>
    /// Group enum numeric value.
    /// </summary>
    [Column("group")]
    [Comment("Group enum numeric value.")]
    public Group Group { get; set; }

    /// <summary>
    /// Id of the parent multisequence.
    /// </summary>
    [Column("multisequence_id")]
    [Comment("Id of the parent multisequence.")]
    public int? MultisequenceId { get; set; }

    /// <summary>
    /// Serial number in multisequence.
    /// </summary>
    [Column("multisequence_number")]
    [Display(Name = "Multisequence number")]
    [Comment("Serial number in multisequence.")]
    public short? MultisequenceNumber { get; set; }

    /// <summary>
    /// Source of the genetic sequence.
    /// </summary>
    [Column("source")]
    [Comment("Source of the genetic sequence.")]
    public byte[]? Source { get; set; }

    /// <summary>
    /// Collection country of the genetic sequence.
    /// </summary>
    [Column("collection_country")]
    [Display(Name = "Collection country")]
    [Comment("Collection country of the genetic sequence.")]
    public string? CollectionCountry { get; set; }

    /// <summary>
    /// Collection date of the genetic sequence.
    /// </summary>
    [Column("collection_date")]
    [Display(Name = "Collection date")]
    [Comment("Collection date of the genetic sequence.")]
    public DateOnly? CollectionDate { get; set; }

    /// <summary>
    /// Collection location of the genetic sequence.
    /// </summary>
    [Column("collection_location")]
    [Comment("Collection location of the genetic sequence.")]
    public string? CollectionLocation { get; set; }

    [InverseProperty("Matter")]
    public virtual ICollection<DataSequence> DataSequence { get; set; } = new List<DataSequence>();

    [InverseProperty("Matter")]
    public virtual ICollection<DnaSequence> DnaSequence { get; set; } = new List<DnaSequence>();

    [InverseProperty("Matter")]
    public virtual ICollection<ImageSequence> ImageSequence { get; set; } = new List<ImageSequence>();

    [InverseProperty("Matter")]
    public virtual ICollection<LiteratureSequence> LiteratureSequence { get; set; } = new List<LiteratureSequence>();

    [ForeignKey("MultisequenceId")]
    [DeleteBehavior(DeleteBehavior.SetNull)]
    [InverseProperty("Matters")]
    public virtual Multisequence? Multisequence { get; set; }

    [InverseProperty("Matter")]
    public virtual ICollection<MusicSequence> MusicSequence { get; set; } = new List<MusicSequence>();

    [InverseProperty("Matter")]
    public virtual ICollection<CommonSequence> Sequence { get; set; } = new List<CommonSequence>();

    [ForeignKey("MatterId")]
    [InverseProperty("Matters")]
    public virtual ICollection<SequenceGroup> Groups { get; set; } = new List<SequenceGroup>();
}
