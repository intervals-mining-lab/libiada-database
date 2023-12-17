using System;
using System.Collections.Generic;


namespace Libiada.Database;

/// <summary>
/// Contains numeric chracteristics of complete sequences.
/// </summary>
public partial class CharacteristicValue  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Id of the sequence for which the characteristic is calculated.
    /// </summary>
    public long SequenceId { get; set; }

    /// <summary>
    /// Numerical value of the characteristic.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    public short CharacteristicLinkId { get; set; }

    public virtual FullCharacteristicLink FullCharacteristicLink { get; set; } = null!;
}
