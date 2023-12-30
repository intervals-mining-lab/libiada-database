using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Contains sequences that represent literary works and their various translations.
/// </summary>
public partial class LiteratureSequence  
{
    /// <summary>
    /// Unique internal identifier of the sequence.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    public Notation Notation { get; set; }

    /// <summary>
    /// Sequence creation date and time (filled trough trigger).
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Id of the research object to which the sequence belongs.
    /// </summary>
    public long MatterId { get; set; }

    /// <summary>
    /// Sequence&apos;s alphabet (array of elements ids).
    /// </summary>
    public List<long> Alphabet { get; set; } = null!;

    /// <summary>
    /// Sequence&apos;s order.
    /// </summary>
    public List<int> Building { get; set; } = null!;

    /// <summary>
    /// Id of the sequence in remote database.
    /// </summary>
    public string? RemoteId { get; set; }

    /// <summary>
    /// Enum numeric value of the remote db from which sequence is downloaded.
    /// </summary>
    public RemoteDb? RemoteDb { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTimeOffset Modified { get; set; }

    /// <summary>
    /// Sequence description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Flag indicating if this sequence is in original language or was translated.
    /// </summary>
    public bool Original { get; set; }

    /// <summary>
    /// Primary language of literary work.
    /// </summary>
    public Language Language { get; set; }

    /// <summary>
    /// Author of translation or automated translator.
    /// </summary>
    public Translator Translator { get; set; }

    public virtual Matter Matter { get; set; } = null!;
}
