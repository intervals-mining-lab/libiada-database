namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Database.Tasks;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information about computational tasks.
/// </summary>
[Table("task")]
[Comment("Contains information about computational tasks.")]
public partial class CalculationTask
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public long Id { get; set; }

    /// <summary>
    /// Task type enum numeric value.
    /// </summary>
    [Column("task_type")]
    [Comment("Task type enum numeric value.")]
    public TaskType TaskType { get; set; }

    /// <summary>
    /// Task description.
    /// </summary>
    [Column("description")]
    [Comment("Task description.")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Task status enum numeric value.
    /// </summary>
    [Column("status")]
    [Comment("Task status enum numeric value.")]
    public TaskState Status { get; set; }

    /// <summary>
    /// Creator user id.
    /// </summary>
    [Column("user_id")]
    [Comment("Creator user id.")]
    public int UserId { get; set; }

    /// <summary>
    /// Task creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Task creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Date and time of the task computation beginning.
    /// </summary>
    [Column("started")]
    [Comment("Date and time of the task computation beginning.")]
    public DateTimeOffset? Started { get; set; }

    /// <summary>
    /// Task completion date and time.
    /// </summary>
    [Column("completed")]
    [Comment("Task completion date and time.")]
    public DateTimeOffset? Completed { get; set; }

    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    [InverseProperty("CalculationTasks")]
    public virtual AspNetUser AspNetUser { get; set; } = null!;

    [InverseProperty("Task")]
    public virtual ICollection<TaskResult> TaskResult { get; set; } = new List<TaskResult>();
}
