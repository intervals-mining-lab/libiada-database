using System;
using System.Collections.Generic;
using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;


namespace Libiada.Database;

/// <summary>
/// Contatins list of possible combinations of dependence characteristics parameters.
/// </summary>
public partial class BinaryCharacteristicLink  
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public short Id { get; set; }

    /// <summary>
    /// Characteristic enum numeric value.
    /// </summary>
    public BinaryCharacteristic BinaryCharacteristic { get; set; }

    /// <summary>
    /// Link enum numeric value.
    /// </summary>
    public Link Link { get; set; }

    public virtual ICollection<BinaryCharacteristicValue> BinaryCharacteristicValues { get; set; } = new List<BinaryCharacteristicValue>();
}
