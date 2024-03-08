namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information on groups of related research objects (such as series of books, chromosomes of the same organism, etc) and their order in these groups.
/// </summary>
[Table("multisequence")]
[Index("Name", Name = "uk_multisequence_name", IsUnique = true)]
[Comment("Contains information on groups of related research objects (such as series of books, chromosomes of the same organism, etc) and their order in these groups.")]
public partial class Multisequence
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public int Id { get; set; }

    /// <summary>
    /// Multisequence name.
    /// </summary>
    [Column("name")]
    [Comment("Multisequence name.")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Multisequence nature enum numeric value.
    /// </summary>
    [Column("nature")]
    [Comment("Multisequence nature enum numeric value.")]
    public Nature Nature { get; set; }

    [InverseProperty("Multisequence")]
    public virtual ICollection<Matter> Matters { get; set; } = new List<Matter>();
}
