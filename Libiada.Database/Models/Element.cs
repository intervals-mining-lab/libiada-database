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
public partial class Element
{
    /// <summary>
    /// Unique internal identifier of the element.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Content of the element.
    /// </summary>
    [Column("value")]
    [StringLength(255)]
    public string? Value { get; set; }

    /// <summary>
    /// Description of the element.
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Name of the element.
    /// </summary>
    [Column("name")]
    [StringLength(255)]
    public string? Name { get; set; }

    /// <summary>
    /// Notation enum numeric value.
    /// </summary>
    [Column("notation")]
    public Notation Notation { get; set; }

    /// <summary>
    /// Element creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    public DateTimeOffset Modified { get; set; }
}
