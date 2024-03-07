namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Database.Tasks;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information about computational tasks.
/// </summary>
[Table("task")]
public partial class CalculationTask
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Task type enum numeric value.
    /// </summary>
    [Column("task_type")]
    public TaskType TaskType { get; set; }

    /// <summary>
    /// Task description.
    /// </summary>
    [Column("description")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Task status enum numeric value.
    /// </summary>
    [Column("status")]
    public TaskState Status { get; set; }

    /// <summary>
    /// Creator user id.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// Task creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Task beginning of computation date and time.
    /// </summary>
    [Column("started")]
    public DateTimeOffset? Started { get; set; }

    /// <summary>
    /// Task completion date and time.
    /// </summary>
    [Column("completed")]
    public DateTimeOffset? Completed { get; set; }

    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    [InverseProperty("CalculationTasks")]
    public virtual AspNetUser AspNetUser { get; set; } = null!;

    [InverseProperty("Task")]
    public virtual ICollection<TaskResult> TaskResult { get; set; } = new List<TaskResult>();
}
