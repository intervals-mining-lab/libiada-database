using System;
using System.Collections.Generic;


namespace Libiada.Database;

/// <summary>
/// Contains JSON results of tasks calculation. Results are stored as key/value pairs.
/// </summary>
public partial class TaskResult  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Parent task id.
    /// </summary>
    public long TaskId { get; set; }

    /// <summary>
    /// Results element name.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Results element value (as json).
    /// </summary>
    public string Value { get; set; } = null!;

    public virtual CalculationTask Task { get; set; } = null!;
}
