namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains numeric chracteristics of congeneric sequences.
/// </summary>
[Table("congeneric_characteristic")]
[Index("SequenceId", "ElementId", Name = "fki_congeneric_characteristic_alphabet_element")]
[Index("SequenceId", Name = "ix_congeneric_characteristic_sequence_id")]
[Index("SequenceId", "ElementId", "CharacteristicLinkId", Name = "uk_congeneric_characteristic", IsUnique = true)]
[Comment("Contains numeric chracteristics of congeneric sequences.")]
public partial class CongenericCharacteristicValue
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
    /// Id of the sequence for which the characteristic is calculated.
    /// </summary>
    [Column("sequence_id")]
    [Comment("Id of the sequence for which the characteristic is calculated.")]
    public long SequenceId { get; set; }

    /// <summary>
    /// Numerical value of the characteristic.
    /// </summary>
    [Column("value")]
    [Comment("Numerical value of the characteristic.")]
    public double Value { get; set; }

    /// <summary>
    /// Id of the element for which the characteristic is calculated.
    /// </summary>
    [Column("element_id")]
    [Display(Name = "Element")]
    [Comment("Id of the element for which the characteristic is calculated.")]
    public long ElementId { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    [Column("characteristic_link_id")]
    [Comment("Characteristic type id.")]
    public short CharacteristicLinkId { get; set; }

    [ForeignKey(nameof(CharacteristicLinkId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    [InverseProperty("CongenericCharacteristicValues")]
    public virtual CongenericCharacteristicLink CongenericCharacteristicLink { get; set; } = null!;

    [ForeignKey(nameof(ElementId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Element Element { get; set; } = null!;
}
