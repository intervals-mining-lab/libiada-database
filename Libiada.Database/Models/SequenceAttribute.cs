namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;


/// <summary>
/// Contains chains' attributes and their values.
/// </summary>
[Table("chain_attribute")]
[Index("SequenceId", "Attribute", "Value", Name = "uk_chain_attribute", IsUnique = true)]
[Comment("Contains chains' attributes and their values.")]
public partial class SequenceAttribute
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public long Id { get; set; }

    /// <summary>
    /// Id of the sequence to which attribute belongs.
    /// </summary>
    [Column("chain_id")]
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

    [ForeignKey("SequenceId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    [InverseProperty("SequenceAttribute")]
    public virtual Subsequence Subsequence { get; set; } = null!;
}
