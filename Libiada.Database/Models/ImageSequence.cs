namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.
/// </summary>
[Table("image_sequence")]
[Index("MatterId", Name = "ix_image_sequence_matter_id")]
[Comment("Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.")]
public partial class ImageSequence
{
    /// <summary>
    /// Unique internal identifier of the image sequence.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier of the image sequence.")]
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    [Column("notation")]
    [Comment("Notation enum numeric value.")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Order extractor enum numeric value used in the process of creation of the sequence.
    /// </summary>
    [Column("order_extractor")]
    [Comment("Order extractor enum numeric value used in the process of creation of the sequence.")]
    public ImageOrderExtractor OrderExtractor { get; set; }

    /// <summary>
    /// Array of image transformations applied begore the extraction of the sequence.
    /// </summary>
    [Column("image_transformations")]
    [Comment("Array of image transformations applied begore the extraction of the sequence.")]
    public List<short> ImageTransformations { get; set; } = null!;

    /// <summary>
    /// Array of matrix transformations applied begore the extraction of the sequence.
    /// </summary>
    [Column("matrix_transformations")]
    [Comment("Array of matrix transformations applied begore the extraction of the sequence.")]
    public List<short> MatrixTransformations { get; set; } = null!;

    /// <summary>
    /// Id of the research object (image) to which the sequence belongs.
    /// </summary>
    [Column("matter_id")]
    [Comment("Id of the research object (image) to which the sequence belongs.")]
    public long MatterId { get; set; }

    /// <summary>
    /// Id of the sequence in remote database.
    /// </summary>
    [Column("remote_id")]
    [Comment("Id of the sequence in remote database.")]
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    [Column("remote_db")]
    [Comment("Enum numeric value of the remote db from which sequence is downloaded.")]
    public short? RemoteDb { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Sequence creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    [ForeignKey("MatterId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    [InverseProperty("ImageSequence")]
    public virtual Matter Matter { get; set; } = null!;
}
