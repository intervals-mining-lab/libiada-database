using System;
using System.Collections.Generic;


namespace Libiada.Database;

/// <summary>
/// Contains numeric chracteristics of accordance of element in different sequences.
/// </summary>
public partial class AccordanceCharacteristicValue  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Id of the first sequence for which the characteristic is calculated.
    /// </summary>
    public long FirstSequenceId { get; set; }

    /// <summary>
    /// Id of the second sequence for which the characteristic is calculated.
    /// </summary>
    public long SecondSequenceId { get; set; }

    /// <summary>
    /// Numerical value of the characteristic.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Id of the element of the first sequence for which the characteristic is calculated.
    /// </summary>
    public long FirstElementId { get; set; }

    /// <summary>
    /// Id of the element of the second sequence for which the characteristic is calculated.
    /// </summary>
    public long SecondElementId { get; set; }

    /// <summary>
    /// Characteristic type id.
    /// </summary>
    public short CharacteristicLinkId { get; set; }

    public virtual AccordanceCharacteristicLink AccordanceCharacteristicLink { get; set; } = null!;
}
