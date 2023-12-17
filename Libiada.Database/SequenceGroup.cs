using System;
using System.Collections.Generic;
using Libiada.Database;
using Microsoft.AspNetCore.Identity;


namespace Libiada.Database;

/// <summary>
/// Contains information about sequences groups.
/// </summary>
public partial class SequenceGroup  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Sequences group name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Sequence group creation date and time (filled trough trigger).
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Record creator user id.
    /// </summary>
    public int CreatorId { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    public DateTime Modified { get; set; }

    /// <summary>
    /// Record editor user id.
    /// </summary>
    public int ModifierId { get; set; }

    /// <summary>
    /// Sequences group nature enum numeric value.
    /// </summary>
    public Nature Nature { get; set; }

    /// <summary>
    /// Sequence group type enum numeric value.
    /// </summary>
    public SequenceGroupType? SequenceGroupType { get; set; }

    public virtual IdentityUser<int> Creator { get; set; } = null!;

    public virtual IdentityUser<int> Modifier { get; set; } = null!;

    public virtual ICollection<Matter> Matters { get; set; } = new List<Matter>();
}
