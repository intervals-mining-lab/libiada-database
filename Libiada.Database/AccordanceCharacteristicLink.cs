using System;
using System.Collections.Generic;
using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;


namespace Libiada.Database;

/// <summary>
/// Contatins list of possible combinations of accordance characteristics parameters.
/// </summary>
public partial class AccordanceCharacteristicLink  
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public short Id { get; set; }

    /// <summary>
    /// Characteristic enum numeric value.
    /// </summary>
    public AccordanceCharacteristic AccordanceCharacteristic { get; set; }

    /// <summary>
    /// Link enum numeric value.
    /// </summary>
    public Link Link { get; set; }

    public virtual ICollection<AccordanceCharacteristicValue> AccordanceCharacteristicValues { get; set; } = new List<AccordanceCharacteristicValue>();
}
