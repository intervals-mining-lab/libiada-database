namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.
/// </summary>
[Table("image_sequence")]
[Index("MatterId", Name = "ix_image_sequence_research_object_id")]
[Comment("Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.")]
public partial class ImageSequence : AbstractSequenceEntity
{
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
    [Column("research_object_id")]
    [Comment("Id of the research object (image) to which the sequence belongs.")]
    public long MatterId { get; set; }

    [ForeignKey(nameof(MatterId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Matter Matter { get; set; } = null!;
}
