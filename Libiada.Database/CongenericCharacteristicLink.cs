using System;
using System.Collections.Generic;
using LibiadaCore.Core;
using LibiadaCore.Core.ArrangementManagers;
using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;


namespace Libiada.Database;

/// <summary>
/// Contatins list of possible combinations of congeneric characteristics parameters.
/// </summary>
public partial class CongenericCharacteristicLink  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public short Id { get; set; }

    /// <summary>
    /// Characteristic enum numeric value.
    /// </summary>
    public CongenericCharacteristic CongenericCharacteristic { get; set; }

    /// <summary>
    /// Link enum numeric value.
    /// </summary>
    public Link Link { get; set; }

    /// <summary>
    /// Arrangement type enum numeric value.
    /// </summary>
    public ArrangementType ArrangementType { get; set; }

    public virtual ICollection<CongenericCharacteristicValue> CongenericCharacteristicValues { get; set; } = new List<CongenericCharacteristicValue>();
}
