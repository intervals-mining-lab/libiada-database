using System;
using System.Collections.Generic;
using Libiada.Database;


namespace Libiada.Database;

/// <summary>
/// Contains information on groups of related research objects (such as series of books, chromosomes of the same organism, etc) and their order in these groups.
/// </summary>
public partial class Multisequence  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Multisequence name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Multisequence nature enum numeric value.
    /// </summary>
    public Nature Nature { get; set; }

    public virtual ICollection<Matter> Matters { get; set; } = new List<Matter>();
}
