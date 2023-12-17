using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Contains research objects, samples, texts, etc (one research object may be represented by several sequences).
/// </summary>
public partial class Matter  
{
    /// <summary>
    /// Unique internal identifier of the research object.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Research object name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Research object nature enum numeric value.
    /// </summary>
    public Nature Nature { get; set; }

    /// <summary>
    /// Description of the research object.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Record creation date and time (filled trough trigger).
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTime Modified { get; set; }

    /// <summary>
    /// Sequence type enum numeric value.
    /// </summary>
    public SequenceType SequenceType { get; set; }

    /// <summary>
    /// Group enum numeric value.
    /// </summary>
    public Group Group { get; set; }

    /// <summary>
    /// Id of the parent multisequence.
    /// </summary>
    public int? MultisequenceId { get; set; }

    /// <summary>
    /// Serial number in multisequence.
    /// </summary>
    public short? MultisequenceNumber { get; set; }

    /// <summary>
    /// Source of the genetic sequence.
    /// </summary>
    public byte[]? Source { get; set; }

    /// <summary>
    /// Collection country of the genetic sequence.
    /// </summary>
    public string? CollectionCountry { get; set; }

    /// <summary>
    /// Collection date of the genetic sequence.
    /// </summary>
    public DateOnly? CollectionDate { get; set; }

    /// <summary>
    /// Collection location of the genetic sequence.
    /// </summary>
    public string? CollectionLocation { get; set; }

    public virtual ICollection<DataSequence> DataSequence { get; set; } = new List<DataSequence>();

    public virtual ICollection<DnaSequence> DnaSequence { get; set; } = new List<DnaSequence>();

    public virtual ICollection<ImageSequence> ImageSequence { get; set; } = new List<ImageSequence>();

    public virtual ICollection<LiteratureSequence> LiteratureSequence { get; set; } = new List<LiteratureSequence>();

    public virtual Multisequence? Multisequence { get; set; }

    public virtual ICollection<MusicSequence> MusicSequence { get; set; } = new List<MusicSequence>();

    public virtual ICollection<CommonSequence> Sequence { get; set; } = new List<CommonSequence>();

    public virtual ICollection<SequenceGroup> Groups { get; set; } = new List<SequenceGroup>();
}
