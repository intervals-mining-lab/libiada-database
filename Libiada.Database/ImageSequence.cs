using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.
/// </summary>
public partial class ImageSequence  
{
    /// <summary>
    /// Unique internal identifier of the image sequence.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    public Notation Notation { get; set; }

    /// <summary>
    /// Order extractor enum numeric value used in the process of creation of the sequence.
    /// </summary>
    public ImageOrderExtractor OrderExtractor { get; set; }

    /// <summary>
    /// Array of image transformations applied begore the extraction of the sequence.
    /// </summary>
    public List<short> ImageTransformations { get; set; } = null!;

    /// <summary>
    /// Array of matrix transformations applied begore the extraction of the sequence.
    /// </summary>
    public List<short> MatrixTransformations { get; set; } = null!;

    /// <summary>
    /// Id of the research object (image) to which the sequence belongs.
    /// </summary>
    public long MatterId { get; set; }

    /// <summary>
    /// Id of the sequence in remote database.
    /// </summary>
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    public short? RemoteDb { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTime Modified { get; set; }

    public virtual Matter Matter { get; set; } = null!;
}
