﻿namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contains information about sequences groups.
/// </summary>
[Table("sequence_group")]
[Index("Name", Name = "uk_sequence_group_name", IsUnique = true)]
[Comment("Contains information about sequences groups.")]
public partial class SequenceGroup
{
    /// <summary>
    /// Unique internal identifier.
    /// </summary>
    [Key]
    [Column("id")]
    [Comment("Unique identifier.")]
    public int Id { get; set; }

    /// <summary>
    /// Sequences group name.
    /// </summary>
    [Column("name")]
    [Comment("Sequences group name.")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Sequence group creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Sequence group creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// Record creator user id.
    /// </summary>
    [Column("creator_id")]
    [Display(Name = "Creator")]
    [Comment("Record creator user id.")]
    public int CreatorId { get; set; }

    /// <summary>
    /// Record last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("Record last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    /// <summary>
    /// Record editor user id.
    /// </summary>
    [Column("modifier_id")]
    [Display(Name = "Modifier")]
    [Comment("Record editor user id.")]
    public int ModifierId { get; set; }

    /// <summary>
    /// Sequences group nature enum numeric value.
    /// </summary>
    [Column("nature")]
    [Comment("Sequences group nature enum numeric value.")]
    public Nature Nature { get; set; }

    /// <summary>
    /// Sequence group type enum numeric value.
    /// </summary>
    [Column("sequence_group_type")]
    [Comment("Sequence group type enum numeric value.")]
    public SequenceGroupType? SequenceGroupType { get; set; }

    [ForeignKey("CreatorId")]
    public virtual AspNetUser Creator { get; set; } = null!;

    [ForeignKey("ModifierId")]
    public virtual AspNetUser Modifier { get; set; } = null!;

    [ForeignKey("GroupId")]
    [InverseProperty("Groups")]
    public virtual ICollection<Matter> Matters { get; set; } = new List<Matter>();
}
