using System;
using System.Collections.Generic;
using Libiada.Database.Tasks;
using Microsoft.AspNetCore.Identity;


namespace Libiada.Database;

/// <summary>
/// Contains information about computational tasks.
/// </summary>
public partial class CalculationTask  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Task type enum numeric value.
    /// </summary>
    public TaskType TaskType { get; set; }

    /// <summary>
    /// Task description.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Task status enum numeric value.
    /// </summary>
    public TaskState Status { get; set; }

    /// <summary>
    /// Creator user id.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Task creation date and time (filled trough trigger).
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Task beginning of computation date and time.
    /// </summary>
    public DateTime? Started { get; set; }

    /// <summary>
    /// Task completion date and time.
    /// </summary>
    public DateTime? Completed { get; set; }

    public virtual IdentityUser<int> AspNetUser { get; set; } = null!;

    public virtual ICollection<TaskResult> TaskResult { get; set; } = new List<TaskResult>();
}
