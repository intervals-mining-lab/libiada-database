using System;
using System.Collections.Generic;
using LibiadaCore.Core;
using LibiadaCore.Core.ArrangementManagers;
using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;


namespace Libiada.Database;

/// <summary>
/// Contatins list of possible combinations of characteristics parameters.
/// </summary>
public partial class FullCharacteristicLink  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public short Id { get; set; }

    /// <summary>
    /// Characteristic enum numeric value.
    /// </summary>
    public FullCharacteristic FullCharacteristic { get; set; }

    /// <summary>
    /// Link enum numeric value.
    /// </summary>
    public Link Link { get; set; }

    /// <summary>
    /// Arrangement type enum numeric value.
    /// </summary>
    public ArrangementType ArrangementType { get; set; }

    public virtual ICollection<CharacteristicValue> CharacteristicValues { get; set; } = new List<CharacteristicValue>();
}
