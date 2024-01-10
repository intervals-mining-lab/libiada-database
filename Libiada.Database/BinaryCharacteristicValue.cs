using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains numeric chracteristics of elements dependece based on their arrangement in sequence.
/// </summary>
[Table("binary_characteristic")]
[Index("SequenceId", Name = "ix_binary_characteristic_chain_id")]
[Index("SequenceId", "FirstElementId", "SecondElementId", "CharacteristicLinkId", Name = "uk_binary_characteristic", IsUnique = true)]
public partial class BinaryCharacteristicValue  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Id of the sequence for which the characteristic is calculated.
    /// </summary>
    [Column("chain_id")]
    public long SequenceId { get; set; }

    /// <summary>
    /// Numerical value of the characteristic.
    /// </summary>
    [Column("value")]
    public double Value { get; set; }

    /// <summary>
    /// Id of the first element of the sequence for which the characteristic is calculated.
    /// </summary>
    [Column("first_element_id")]
    [Display(Name = "First element")]
    public long FirstElementId { get; set; }

    /// <summary>
    /// Id of the second element of the sequence for which the characteristic is calculated.
    /// </summary>
    [Column("second_element_id")]
    [Display(Name = "Second element")]
    public long SecondElementId { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    [Column("characteristic_link_id")]
    public short CharacteristicLinkId { get; set; }

    [ForeignKey("CharacteristicLinkId")]
    [InverseProperty("BinaryCharacteristicValues")]
    public virtual BinaryCharacteristicLink BinaryCharacteristicLink { get; set; } = null!;
}
