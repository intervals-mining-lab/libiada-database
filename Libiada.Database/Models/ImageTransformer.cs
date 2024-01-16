namespace Libiada.Database.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Libiada.Core.Images;

using Libiada.Database.Attributes;

/// <summary>
/// The image transformer.
/// </summary>
public enum ImageTransformer : byte
{
    /// <summary>
    /// Resizes image to given width and height.
    /// </summary>
    [Display(Name = "Resize image")]
    [Description("Resizes image to given width and height")]
    [ImageTransformerType(typeof(ImageResizer))]
    ImageResizer = 1
}