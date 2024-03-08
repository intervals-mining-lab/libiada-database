namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Base table for all elements that are stored in the database and used in alphabets of sequences.
/// </summary>
[Table("element")]
[Index("Notation", Name = "ix_element_notation_id")]
[Index("Value", Name = "ix_element_value")]
[Index("Value", "Notation", Name = "uk_element_value_notation", IsUnique = true)]
[Comment("Base table for all elements that are stored in the database and used in alphabets of sequences.")]
public partial class Element
{
    /// <summary>
    /// Unique internal identifier of the element.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier of the element.")]
    public long Id { get; set; }

    /// <summary>
    /// Content of the element.
    /// </summary>
    [Column("value")]
    [StringLength(255)]
    [Comment("Content of the element.")]
    public string? Value { get; set; }

    /// <summary>
    /// Description of the element.
    /// </summary>
    [Column("description")]
    [Comment("Description of the element.")]
    public string? Description { get; set; }

    /// <summary>
    /// Name of the element.
    /// </summary>
    [Column("name")]
    [StringLength(255)]
    [Comment("Name of the element.")]
    public string? Name { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    [Column("notation")]
    [Comment("Notation enum numeric value.")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Element creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Element creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }
}
