using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains chains&apos; attributes and their values.
/// </summary>
[Table("chain_attribute")]
[Index("SequenceId", "Attribute", "Value", Name = "uk_chain_attribute", IsUnique = true)]
public partial class SequenceAttribute  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Id of the sequence to which attribute belongs.
    /// </summary>
    [Column("chain_id")]
    public long SequenceId { get; set; }

    /// <summary>
    /// Attribute enum numeric value.
    /// </summary>
    [Column("attribute")]
    public Attribute Attribute { get; set; }

    /// <summary>
    /// Text of the attribute.
    /// </summary>
    [Column("value")]
    public string Value { get; set; } = null!;

    [ForeignKey("SequenceId")]
    [InverseProperty("SequenceAttribute")]
    public virtual Subsequence Subsequence { get; set; } = null!;
}
