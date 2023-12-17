using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Contains chains&apos; attributes and their values.
/// </summary>
public partial class SequenceAttribute  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Id of the sequence to which attribute belongs.
    /// </summary>
    public long SequenceId { get; set; }

    /// <summary>
    /// Attribute enum numeric value.
    /// </summary>
    public Attribute Attribute { get; set; }

    /// <summary>
    /// Text of the attribute.
    /// </summary>
    public string Value { get; set; } = null!;

    public virtual Subsequence Subsequence { get; set; } = null!;
}
