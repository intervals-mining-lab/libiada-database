namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;


/// <summary>
/// Contains numeric chracteristics of elements dependece based on their arrangement in sequence.
/// </summary>
[Table("binary_characteristic")]
[Index("SequenceId", Name = "ix_binary_characteristic_sequence_id")]
[Index("SequenceId", "FirstElementId", "SecondElementId", "CharacteristicLinkId", Name = "uk_binary_characteristic", IsUnique = true)]
[Comment("Contains numeric chracteristics of elements dependece based on their arrangement in sequence.")]
public partial class BinaryCharacteristicValue
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
    [Display(Name = "Sequence")]
    [Comment("Id of the sequence for which the characteristic is calculated.")]
    public long SequenceId { get; set; }

    /// <summary>
    /// Numerical value of the characteristic.
    /// </summary>
    [Column("value")]
    [Comment("Numerical value of the characteristic.")]
    public double Value { get; set; }

    /// <summary>
    /// Id of the first element of the sequence for which the characteristic is calculated.
    /// </summary>
    [Column("first_element_id")]
    [Display(Name = "First element")]
    [Comment("Id of the first element of the sequence for which the characteristic is calculated.")]
    public long FirstElementId { get; set; }

    /// <summary>
    /// Id of the second element of the sequence for which the characteristic is calculated.
    /// </summary>
    [Column("second_element_id")]
    [Display(Name = "Second element")]
    [Comment("Id of the second element of the sequence for which the characteristic is calculated.")]
    public long SecondElementId { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    [Column("characteristic_link_id")]
    [Comment("Characteristic type id.")]
    public short CharacteristicLinkId { get; set; }

    [ForeignKey(nameof(CharacteristicLinkId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    [InverseProperty("BinaryCharacteristicValues")]
    public virtual BinaryCharacteristicLink BinaryCharacteristicLink { get; set; } = null!;

    [ForeignKey(nameof(FirstElementId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Element FirstElement { get; set; } = null!;

    [ForeignKey(nameof(SecondElementId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Element SecondElement { get; set; } = null!;

    [ForeignKey(nameof(SequenceId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual AbstractSequenceEntity Sequence { get; set; } = null!;

}
