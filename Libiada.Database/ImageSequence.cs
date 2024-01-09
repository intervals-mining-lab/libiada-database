using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libiada.Database;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.
/// </summary>
[Table("image_sequence")]
[Index("MatterId", Name = "ix_image_sequence_matter_id")]
public partial class ImageSequence  
{
    /// <summary>
    /// Unique internal identifier of the image sequence.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    [Column("notation")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Order extractor enum numeric value used in the process of creation of the sequence.
    /// </summary>
    [Column("order_extractor")]
    public ImageOrderExtractor OrderExtractor { get; set; }

    /// <summary>
    /// Array of image transformations applied begore the extraction of the sequence.
    /// </summary>
    [Column("image_transformations")]
    public List<short> ImageTransformations { get; set; } = null!;

    /// <summary>
    /// Array of matrix transformations applied begore the extraction of the sequence.
    /// </summary>
    [Column("matrix_transformations")]
    public List<short> MatrixTransformations { get; set; } = null!;

    /// <summary>
    /// Id of the research object (image) to which the sequence belongs.
    /// </summary>
    [Column("matter_id")]
    public long MatterId { get; set; }

    /// <summary>
    /// Id of the sequence in remote database.
    /// </summary>
    [Column("remote_id")]
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    [Column("remote_db")]
    public short? RemoteDb { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }

    [ForeignKey("MatterId")]
    [InverseProperty("ImageSequence")]
    public virtual Matter Matter { get; set; } = null!;
}
