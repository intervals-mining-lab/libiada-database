namespace Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Core.Extensions;

/// <summary>
/// The attribute repository.
/// </summary>
public class AttributeRepository
{
    /// <summary>
    /// The attributes dictionary.
    /// </summary>
    private readonly Dictionary<string, AnnotationAttribute> attributesDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeRepository"/> class.
    /// </summary>
    public AttributeRepository()
    {
        AnnotationAttribute[] attributes = EnumExtensions.ToArray<AnnotationAttribute>();
        attributesDictionary = attributes.ToDictionary(a => a.GetDisplayValue());
    }

    /// <summary>
    /// Gets attribute by name.
    /// </summary>
    /// <param name="name">
    /// The name.
    /// </param>
    /// <returns>
    /// The <see cref="AnnotationAttribute"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if attribute with given name is not found.
    /// </exception>
    public AnnotationAttribute GetAttributeByName(string name)
    {
        if (attributesDictionary.TryGetValue(name, out AnnotationAttribute value))
        {
            return value;
        }
        else
        {
            throw new Exception($"Unknown attribute: {name}");
        }
    }
}
