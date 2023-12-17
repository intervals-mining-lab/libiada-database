using System;
using System.Collections.Generic;


namespace Libiada.Database;

/// <summary>
/// Contains information on additional fragment positions (for subsequences concatenated from several parts).
/// </summary>
public partial class Position  
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Parent subsequence id.
    /// </summary>
    public long SubsequenceId { get; set; }

    /// <summary>
    /// Index of the fragment beginning (from zero).
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Fragment length.
    /// </summary>
    public int Length { get; set; }

    public virtual Subsequence Subsequence { get; set; } = null!;
}
