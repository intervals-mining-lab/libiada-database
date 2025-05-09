﻿namespace Libiada.Database.Attributes;

using Libiada.Core.Images;

/// <summary>
/// The image transformer type attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ImageTransformerTypeAttribute : Attribute
{
    /// <summary>
    /// The image processor type.
    /// </summary>
    public readonly Type Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageTransformerTypeAttribute"/> class.
    /// </summary>
    /// <param name="imageProcessorType">
    /// The image processor type.
    /// </param>
    /// <exception cref="ArgumentException">
    /// </exception>
    public ImageTransformerTypeAttribute(Type imageProcessorType)
    {
        if (!imageProcessorType.IsAssignableTo(typeof(IImageTransformer)))
        {
            throw new ArgumentException($"Task class attribute value is invalid, it can only be subtype of {nameof(IImageTransformer)}", nameof(imageProcessorType));
        }

        Value = imageProcessorType;
    }
}