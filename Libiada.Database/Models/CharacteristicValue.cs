namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains numeric chracteristics of complete sequences.
/// </summary>
[Table("full_characteristic")]
[Index("SequenceId", Name = "ix_characteristic_chain_id")]
[Index("CharacteristicLinkId", Name = "ix_characteristic_characteristic_type_link")]
[Index("SequenceId", "CharacteristicLinkId", Name = "uk_characteristic", IsUnique = true)]
public partial class CharacteristicValue
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
    /// Characteristic type id.
    /// </summary>
    [Column("characteristic_link_id")]
    public short CharacteristicLinkId { get; set; }

    [ForeignKey("CharacteristicLinkId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    [InverseProperty("CharacteristicValues")]
    public virtual FullCharacteristicLink FullCharacteristicLink { get; set; } = null!;
}
