using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibiadaCore.Core;
using LibiadaCore.Core.ArrangementManagers;
using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
using Microsoft.EntityFrameworkCore;


namespace Libiada.Database;

/// <summary>
/// Contatins list of possible combinations of congeneric characteristics parameters.
/// </summary>
[Table("congeneric_characteristic_link")]
[Index("CongenericCharacteristic", "Link", "ArrangementType", Name = "uk_congeneric_characteristic_link", IsUnique = true)]
public partial class CongenericCharacteristicLink  
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
    [Column("congeneric_characteristic")]
    public CongenericCharacteristic CongenericCharacteristic { get; set; }

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

    [InverseProperty("CongenericCharacteristicLink")]
    public virtual ICollection<CongenericCharacteristicValue> CongenericCharacteristicValues { get; set; } = new List<CongenericCharacteristicValue>();
}
