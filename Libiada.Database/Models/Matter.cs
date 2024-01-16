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
public partial class Matter
{
    /// <summary>
    /// Unique internal identifier of the research object.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Research object name.
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Research object nature enum numeric value.
    /// </summary>
    [Column("nature")]
    public Nature Nature { get; set; }

    /// <summary>
    /// Description of the research object.
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Record creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Sequence type enum numeric value.
    /// </summary>
    [Column("sequence_type")]
    [Display(Name = "Sequence type")]
    public SequenceType SequenceType { get; set; }

    /// <summary>
    /// Group enum numeric value.
    /// </summary>
    [Column("group")]
    public Group Group { get; set; }

    /// <summary>
    /// Id of the parent multisequence.
    /// </summary>
    [Column("multisequence_id")]
    public int? MultisequenceId { get; set; }

    /// <summary>
    /// Serial number in multisequence.
    /// </summary>
    [Column("multisequence_number")]
    [Display(Name = "Multisequence number")]
    public short? MultisequenceNumber { get; set; }

    /// <summary>
    /// Source of the genetic sequence.
    /// </summary>
    [Column("source")]
    public byte[]? Source { get; set; }

    /// <summary>
    /// Collection country of the genetic sequence.
    /// </summary>
    [Column("collection_country")]
    [Display(Name = "Collection country")]
    public string? CollectionCountry { get; set; }

    /// <summary>
    /// Collection date of the genetic sequence.
    /// </summary>
    [Column("collection_date")]
    [Display(Name = "Collection date")]
    public DateOnly? CollectionDate { get; set; }

    /// <summary>
    /// Collection location of the genetic sequence.
    /// </summary>
    [Column("collection_location")]
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
