using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Base table for all elements that are stored in the database and used in alphabets of sequences.
/// </summary>
public partial class Element  
{
    /// <summary>
    /// Unique internal identifier of the element.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Content of the element.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Description of the element.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Name of the element.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    public Notation Notation { get; set; }

    /// <summary>
    /// Element creation date and time (filled trough trigger).
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTimeOffset Modified { get; set; }
}
