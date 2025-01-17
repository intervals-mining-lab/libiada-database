namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;


/// <summary>
/// Contains chains' attributes and their values.
/// </summary>
[Table("sequence_attribute")]
[Index("SequenceId", "Attribute", "Value", Name = "uk_sequence_attribute", IsUnique = true)] // for value md5 hash is calculated
[Comment("Contains sequences' attributes and their values.")]
public partial class SequenceAttribute
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
    /// Id of the sequence to which attribute belongs.
    /// </summary>
    [Column("sequence_id")]
    [Comment("Id of the sequence to which attribute belongs.")]
    public long SequenceId { get; set; }

    /// <summary>
    /// Attribute enum numeric value.
    /// </summary>
    [Column("attribute")]
    [Comment("Attribute enum numeric value.")]
    public AnnotationAttribute Attribute { get; set; }

    /// <summary>
    /// Text of the attribute.
    /// </summary>
    [Column("value")]
    [Comment("Text of the attribute.")]
    public string Value { get; set; } = null!;

    [ForeignKey(nameof(SequenceId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual AbstractSequenceEntity Sequence { get; set; } = null!;
}
