﻿namespace Libiada.Database;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Libiada.Database.Attributes;

/// <summary>
/// Remote database
/// </summary>
public enum RemoteDb : byte
{
    /// <summary>
    /// GenBank / NCBI
    /// </summary>
    [Display(Name = "GenBank / NCBI")]
    [Description("National center for biotechnological information")]
    [Nature(Nature.Genetic)]
    GenBank = 1
}
