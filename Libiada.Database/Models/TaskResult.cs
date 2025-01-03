namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains JSON results of tasks calculation. Results are stored as key/value pairs.
/// </summary>
[Table("task_result")]
[Index("TaskId", "Key", Name = "uk_task_result", IsUnique = true)]
[Comment("Contains JSON results of tasks calculation. Results are stored as key/value pairs.")]
public partial class TaskResult
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    [Comment("Unique identifier.")]
    public long Id { get; set; }

    /// <summary>
    /// Parent task id.
    /// </summary>
    [Column("task_id")]
    [Comment("Parent task id.")]
    public long TaskId { get; set; }

    /// <summary>
    /// Results element name.
    /// </summary>
    [Column("key")]
    [Comment("Results element name.")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Results element value (as json).
    /// </summary>
    [Column("value", TypeName = "json")]
    [Comment("Results element value (as json).")]
    public string Value { get; set; } = null!;

    [ForeignKey("TaskId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    [InverseProperty("TaskResult")]
    public virtual CalculationTask Task { get; set; } = null!;
}
