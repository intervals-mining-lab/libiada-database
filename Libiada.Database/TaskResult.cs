using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains JSON results of tasks calculation. Results are stored as key/value pairs.
/// </summary>
[Table("task_result")]
[Index("TaskId", "Key", Name = "uk_task_result", IsUnique = true)]
public partial class TaskResult  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Parent task id.
    /// </summary>
    [Column("task_id")]
    public long TaskId { get; set; }

    /// <summary>
    /// Results element name.
    /// </summary>
    [Column("key")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Results element value (as json).
    /// </summary>
    [Column("value", TypeName = "json")]
    public string Value { get; set; } = null!;

    [ForeignKey("TaskId")]
    [InverseProperty("TaskResult")]
    public virtual CalculationTask Task { get; set; } = null!;
}
