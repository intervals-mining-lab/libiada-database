using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Libiada.Database;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains sequences that represent literary works and their various translations.
/// </summary>
[Table("literature_chain")]
[Index("MatterId", Name = "ix_literature_chain_matter_id")]
[Index("MatterId", "Language", Name = "ix_literature_chain_matter_language")]
[Index("Notation", Name = "ix_literature_chain_notation_id")]
[Index("Notation", "MatterId", "Language", "Translator", Name = "uk_literature_chain", IsUnique = true)]
public partial class LiteratureSequence  
{
    /// <summary>
    /// Unique internal identifier of the sequence.
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
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    [Column("matter_id")]
    public long MatterId { get; set; }

    /// <summary>
    /// Sequence&apos;s alphabet (array of elements ids).
    /// </summary>
    [Column("alphabet")]
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence&apos;s order.
    /// </summary>
    [Column("building")]
    public List<int> Building { get; set; } = null!;

    /// <summary>
    /// Id of the sequence in remote database.
    /// </summary>
    [Column("remote_id")]
    [StringLength(255)]
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    [Column("remote_db")]
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Sequence description.
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Flag indicating if this sequence is in original language or was translated.
    /// </summary>
    [Column("original")]
    public bool Original { get; set; }

    /// <summary>
    /// Primary language of literary work.
    /// </summary>
    [Column("language")]
    public Language Language { get; set; }

    /// <summary>
    /// Author of translation or automated translator.
    /// </summary>
    [Column("translator")]
    public Translator Translator { get; set; }

    [ForeignKey("MatterId")]
    [InverseProperty("LiteratureSequence")]
    public virtual Matter Matter { get; set; } = null!;
}
