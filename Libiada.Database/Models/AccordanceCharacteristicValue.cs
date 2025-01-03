namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains numeric chracteristics of accordance of element in different sequences.
/// </summary>
[Table("accordance_characteristic")]
[Index("FirstSequenceId", Name = "ix_accordance_characteristic_first_chain_id")]
[Index("SecondSequenceId", Name = "ix_accordance_characteristic_second_chain_id")]
[Index("FirstSequenceId", "SecondSequenceId", "FirstElementId", "SecondElementId", "CharacteristicLinkId", Name = "uk_accordance_characteristic", IsUnique = true)]
[Comment("Contains numeric chracteristics of accordance of element in different sequences.")]
public partial class AccordanceCharacteristicValue
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    [Comment("Unique internal identifier.")]
    public long Id { get; set; }

    /// <summary>
    /// Id of the first sequence for which the characteristic is calculated.
    /// </summary>
    [Column("first_chain_id")]
    [Display(Name = "First sequence")]
    [Comment("Id of the first sequence for which the characteristic is calculated.")]
    public long FirstSequenceId { get; set; }

    /// <summary>
    /// Id of the second sequence for which the characteristic is calculated.
    /// </summary>
    [Column("second_chain_id")]
    [Display(Name = "Second sequence")]
    [Comment("Id of the second sequence for which the characteristic is calculated.")]
    public long SecondSequenceId { get; set; }

    /// <summary>
    /// Numerical value of the characteristic.
    /// </summary>
    [Column("value")]
    [Comment("Numerical value of the characteristic.")]
    public double Value { get; set; }

    /// <summary>
    /// Id of the element of the first sequence for which the characteristic is calculated.
    /// </summary>
    [Column("first_element_id")]
    [Display(Name = "First element")]
    [Comment("Id of the element of the first sequence for which the characteristic is calculated.")]
    public long FirstElementId { get; set; }

    /// <summary>
    /// Id of the element of the second sequence for which the characteristic is calculated.
    /// </summary>
    [Column("second_element_id")]
    [Display(Name = "Second element")]
    [Comment("Id of the element of the second sequence for which the characteristic is calculated.")]
    public long SecondElementId { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    [Column("characteristic_link_id")]
    [Comment("Characteristic type id.")]
    public short CharacteristicLinkId { get; set; }

    [ForeignKey("CharacteristicLinkId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    [InverseProperty("AccordanceCharacteristicValues")]
    public virtual AccordanceCharacteristicLink AccordanceCharacteristicLink { get; set; } = null!;

    [ForeignKey("FirstElementId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Element FirstElement { get; set; } = null!;

    [ForeignKey("SecondElementId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Element SecondElement { get; set; } = null!;
}
