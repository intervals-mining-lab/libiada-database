namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.BinaryCalculators;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contatins list of possible combinations of dependence characteristics parameters.
/// </summary>
[Table("binary_characteristic_link")]
[Index("BinaryCharacteristic", "Link", Name = "uk_binary_characteristic_link", IsUnique = true)]
public partial class BinaryCharacteristicLink
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public short Id { get; set; }

    /// <summary>
    /// Characteristic enum numeric value.
    /// </summary>
    [Column("binary_characteristic")]
    public BinaryCharacteristic BinaryCharacteristic { get; set; }

    /// <summary>
    /// Link enum numeric value.
    /// </summary>
    [Column("link")]
    public Link Link { get; set; }

    [InverseProperty("BinaryCharacteristicLink")]
    public virtual ICollection<BinaryCharacteristicValue> BinaryCharacteristicValues { get; set; } = new List<BinaryCharacteristicValue>();
}
