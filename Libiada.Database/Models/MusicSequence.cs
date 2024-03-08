namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Music;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains sequences that represent musical works in form of note, fmotive or measure sequences.
/// </summary>
[Table("music_chain")]
[Index("MatterId", Name = "ix_music_chain_matter_id")]
[Index("Notation", Name = "ix_music_chain_notation_id")]
[Index("MatterId", "Notation", "PauseTreatment", "SequentialTransfer", Name = "uk_music_chain", IsUnique = true)]
[Comment("Contains sequences that represent musical works in form of note, fmotive or measure sequences.")]
public partial class MusicSequence
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    [Column("notation")]
    [Display(Name = "Notation of elements in sequence")]
    [Comment("Notation enum numeric value.")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Sequence creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    [Column("matter_id")]
    [Display(Name = "Sequence belonges to research object")]
    [Comment("Id of the research object to which the sequence belongs.")]
    public long MatterId { get; set; }

    /// <summary>
    /// Sequence's alphabet (array of elements ids).
    /// </summary>
    [Column("alphabet")]
    [Comment("Sequence's alphabet (array of elements ids).")]
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence's order.
    /// </summary>
    [Column("building")]
    [Comment("Sequence's order.")]
    public List<int> Order { get; set; } = null!;

    /// <summary>
    /// Id of the sequence in the remote database.
    /// </summary>
    [Column("remote_id")]
    [StringLength(255)]
    [Display(Name = "Id in remote database")]
    [Comment("Id of the sequence in the remote database.")]
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    [Column("remote_db")]
    [Display(Name = "Remote database")]
    [Comment("Enum numeric value of the remote db from which sequence is downloaded.")]
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Sequence description.
    /// </summary>
    [Column("description")]
    [Comment("Sequence description.")]
    public string? Description { get; set; }

    /// <summary>
    /// Pause treatment enum numeric value.
    /// </summary>
    [Column("pause_treatment")]
    [Display(Name = "Pause treatment")]
    [Comment("Pause treatment enum numeric value.")]
    public PauseTreatment PauseTreatment { get; set; }

    /// <summary>
    /// Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.
    /// </summary>
    [Column("sequential_transfer")]
    [Display(Name = "Sequential transfer is used")]
    [Comment("Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.")]
    public bool SequentialTransfer { get; set; }

    [ForeignKey("MatterId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    [InverseProperty("MusicSequence")]
    public virtual Matter Matter { get; set; } = null!;
}
