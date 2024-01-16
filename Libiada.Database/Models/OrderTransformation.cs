namespace Libiada.Database.Models;

using Libiada.Core.Core;

using System.ComponentModel.DataAnnotations;

public enum OrderTransformation : byte
{
    [Display(Name = "Dissimilar order")]
    [Libiada.Core.Attributes.Link(Link.NotApplied)]
    Dissimilar = 1,

    [Display(Name = "High order with link to the begining")]
    [Libiada.Core.Attributes.Link(Link.Start)]
    HighOrderToStart = 2,

    [Display(Name = "High order with link to the end")]
    [Libiada.Core.Attributes.Link(Link.End)]
    HighOrderToEnd = 3,

    [Display(Name = "High order with cyclic link to the begining")]
    [Libiada.Core.Attributes.Link(Link.CycleStart)]
    HighOrderCyclicToStart = 4,

    [Display(Name = "High order with cyclic link to the end")]
    [Libiada.Core.Attributes.Link(Link.CycleEnd)]
    HighOrderCyclicToEnd = 5
}