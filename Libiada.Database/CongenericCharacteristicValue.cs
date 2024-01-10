using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contains numeric chracteristics of congeneric sequences.
/// </summary>
[Table("congeneric_characteristic")]
[Index("SequenceId", "ElementId", Name = "fki_congeneric_characteristic_alphabet_element")]
[Index("SequenceId", Name = "ix_congeneric_characteristic_chain_id")]
[Index("SequenceId", "ElementId", "CharacteristicLinkId", Name = "uk_congeneric_characteristic", IsUnique = true)]
public partial class CongenericCharacteristicValue  
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
    /// Id of the element for which the characteristic is calculated.
    /// </summary>
    [Column("element_id")]
    [Display(Name = "Element")]
    public long ElementId { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    [Column("characteristic_link_id")]
    public short CharacteristicLinkId { get; set; }

    [ForeignKey("CharacteristicLinkId")]
    [InverseProperty("CongenericCharacteristicValues")]
    public virtual CongenericCharacteristicLink CongenericCharacteristicLink { get; set; } = null!;
}
