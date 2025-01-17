namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core;
using Libiada.Core.Core.ArrangementManagers;
using Libiada.Core.Core.Characteristics.Calculators.CongenericCalculators;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contatins list of possible combinations of congeneric characteristics parameters.
/// </summary>
[Table("congeneric_characteristic_link")]
[Index("CongenericCharacteristic", "Link", "ArrangementType", Name = "uk_congeneric_characteristic_link", IsUnique = true)]
[Comment("Contatins list of possible combinations of congeneric characteristics parameters.")]
public partial class CongenericCharacteristicLink
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public short Id { get; set; }

    /// <summary>
    /// Characteristic enum numeric value.
    /// </summary>
    [Column("congeneric_characteristic")]
    [Comment("Characteristic enum numeric value.")]
    public CongenericCharacteristic CongenericCharacteristic { get; set; }

    /// <summary>
    /// Link enum numeric value.
    /// </summary>
    [Column("link")]
    [Comment("Link enum numeric value.")]
    public Link Link { get; set; }

    /// <summary>
    /// Arrangement type enum numeric value.
    /// </summary>
    [Column("arrangement_type")]
    [Comment("Arrangement type enum numeric value.")]
    public ArrangementType ArrangementType { get; set; }

    [InverseProperty("CongenericCharacteristicLink")]
    public virtual ICollection<CongenericCharacteristicValue> CongenericCharacteristicValues { get; set; } = [];
}
