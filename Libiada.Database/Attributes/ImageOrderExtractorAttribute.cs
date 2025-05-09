﻿namespace Libiada.Database.Attributes;

using Libiada.Core.Images;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ImageOrderExtractorAttribute : Attribute
{
    /// <summary>
    /// Image order extractor trajectory type.
    /// </summary>
    public readonly Type Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageOrderExtractorAttribute"/> class.
    /// </summary>
    /// <param name="value">
    /// Image order extractor trajectory type.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if value is not derived from <see cref="IImageOrderExtractor"/>
    /// </exception>
    public ImageOrderExtractorAttribute(Type value)
    {
        if (!value.IsAssignableTo(typeof(IImageOrderExtractor)))
        {
            throw new ArgumentException($"Image order extractor attribute value is invalid, it can only class implementing {nameof(IImageOrderExtractor)} interface", nameof(value));
        }

        Value = value;
    }
}
