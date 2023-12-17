using System;
using System.Collections.Generic;


namespace Libiada.Database;

/// <summary>
/// Contains numeric chracteristics of elements dependece based on their arrangement in sequence.
/// </summary>
public partial class BinaryCharacteristicValue  
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
    /// Id of the first element of the sequence for which the characteristic is calculated.
    /// </summary>
    public long FirstElementId { get; set; }

    /// <summary>
    /// Id of the second element of the sequence for which the characteristic is calculated.
    /// </summary>
    public long SecondElementId { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    public short CharacteristicLinkId { get; set; }

    public virtual BinaryCharacteristicLink BinaryCharacteristicLink { get; set; } = null!;
}
