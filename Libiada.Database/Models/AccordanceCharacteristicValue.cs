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
public partial class AccordanceCharacteristicValue
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Id of the first sequence for which the characteristic is calculated.
    /// </summary>
    [Column("first_chain_id")]
    public long FirstSequenceId { get; set; }

    /// <summary>
    /// Id of the second sequence for which the characteristic is calculated.
    /// </summary>
    [Column("second_chain_id")]
    public long SecondSequenceId { get; set; }

    /// <summary>
    /// Numerical value of the characteristic.
    /// </summary>
    [Column("value")]
    public double Value { get; set; }

    /// <summary>
    /// Id of the element of the first sequence for which the characteristic is calculated.
    /// </summary>
    [Column("first_element_id")]
    public long FirstElementId { get; set; }

    /// <summary>
    /// Id of the element of the second sequence for which the characteristic is calculated.
    /// </summary>
    [Column("second_element_id")]
    public long SecondElementId { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    [Column("characteristic_link_id")]
    public short CharacteristicLinkId { get; set; }

    [ForeignKey("CharacteristicLinkId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    [InverseProperty("AccordanceCharacteristicValues")]
    public virtual AccordanceCharacteristicLink AccordanceCharacteristicLink { get; set; } = null!;
}
