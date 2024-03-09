namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.AccordanceCalculators;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contatins list of possible combinations of accordance characteristics parameters.
/// </summary>
[Table("accordance_characteristic_link")]
[Index("AccordanceCharacteristic", "Link", Name = "uk_accordance_characteristic_link", IsUnique = true)]
[Comment("Contatins list of possible combinations of accordance characteristics parameters.")]
public partial class AccordanceCharacteristicLink
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public short Id { get; set; }

    /// <summary>
    /// Characteristic enum numeric value.
    /// </summary>
    [Column("accordance_characteristic")]
    [Comment("Characteristic enum numeric value.")]
    public AccordanceCharacteristic AccordanceCharacteristic { get; set; }

    /// <summary>
    /// Link enum numeric value.
    /// </summary>
    [Column("link")]
    [Comment("Link enum numeric value.")]
    public Link Link { get; set; }

    [InverseProperty("AccordanceCharacteristicLink")]
    public virtual ICollection<AccordanceCharacteristicValue> AccordanceCharacteristicValues { get; set; } = new List<AccordanceCharacteristicValue>();
}
