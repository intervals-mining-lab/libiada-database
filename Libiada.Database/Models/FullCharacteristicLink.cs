namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core;
using Libiada.Core.Core.ArrangementManagers;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contatins list of possible combinations of characteristics parameters.
/// </summary>
[Table("full_characteristic_link")]
[Index("FullCharacteristic", "Link", "ArrangementType", Name = "uk_full_characteristic_link", IsUnique = true)]
public partial class FullCharacteristicLink
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public short Id { get; set; }

    /// <summary>
    /// Characteristic enum numeric value.
    /// </summary>
    [Column("full_characteristic")]
    public FullCharacteristic FullCharacteristic { get; set; }

    /// <summary>
    /// Link enum numeric value.
    /// </summary>
    [Column("link")]
    public Link Link { get; set; }

    /// <summary>
    /// Arrangement type enum numeric value.
    /// </summary>
    [Column("arrangement_type")]
    public ArrangementType ArrangementType { get; set; }

    [InverseProperty("FullCharacteristicLink")]
    public virtual ICollection<CharacteristicValue> CharacteristicValues { get; set; } = new List<CharacteristicValue>();
}
