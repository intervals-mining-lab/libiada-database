﻿namespace Libiada.Database.Models.Repositories.Catalogs;

using Bio.IO.GenBank;

using Libiada.Database.Models.CalculatorsData;

/// <summary>
/// The sequence attribute.
/// </summary>
public class SequenceAttributeRepository : ISequenceAttributeRepository
{
    /// <summary>
    /// Database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;

    /// <summary>
    /// The attribute repository.
    /// </summary>
    private readonly AttributeRepository attributeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceAttributeRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    public SequenceAttributeRepository(LibiadaDatabaseEntities db)
    {
        this.db = db;
        attributeRepository = new AttributeRepository();
    }

    /// <summary>
    /// The dispose.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Cleans attribute value.
    /// </summary>
    /// <param name="attributeValue">
    /// The attribute value.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string CleanAttributeValue(string attributeValue)
    {
        return attributeValue.Replace("\"", string.Empty)
                             .Replace('\n', ' ')
                             .Replace('\r', ' ')
                             .Replace('\t', ' ')
                             .Replace("  ", " ");
    }

    /// <summary>
    /// Gets the attribute single value.
    /// </summary>
    /// <param name="source">
    /// Collection of <see cref="FeatureItem"/> to search in.
    /// </param>
    /// <param name="attributeName">
    /// Name of the attribute to  search for.
    /// </param>
    /// <returns>
    /// Cleaned attribute value or null if nothing was found.
    /// </returns>
    public static string GetAttributeSingleValue(IEnumerable<FeatureItem> source, string attributeName)
    {
        string result;
        try
        {
            result = source.Select(s => s.Qualifiers.SingleOrDefault(q => q.Key == attributeName)
                                         .Value?.SingleOrDefault())
                           .SingleOrDefault(s => s != null);
        }
        catch(Exception ex)
        {
            throw new Exception($"Attribute had more than one value with given key. Key: {attributeName}.", ex);
        }

        return result != null ? CleanAttributeValue(result) : null;
    }

    /// <summary>
    /// The get attributes.
    /// </summary>
    /// <param name="subsequenceIds">
    /// The subsequences ids.
    /// </param>
    /// <returns>
    /// The <see cref="T:Dictionary{long, Libiada.Database.Models.CalculatorsData.AttributeValue[]}"/>.
    /// </returns>
    public Dictionary<long, AttributeValue[]> GetAttributes(IEnumerable<long> subsequenceIds)
    {
        return db.SequenceAttributes.Where(sa => subsequenceIds.Contains(sa.SequenceId))
                                   .Select(sa => new
                                                 {
                                                     sa.SequenceId,
                                                     sa.Attribute,
                                                     sa.Value
                                                 })
                                   .ToArray()
                                   .GroupBy(sa => sa.SequenceId)
                                   .ToDictionary(sa => sa.Key, sa => sa.Select(av => new AttributeValue((byte)av.Attribute, av.Value)).ToArray());
    }

    /// <summary>
    /// Creates and adds to db subsequence attributes.
    /// </summary>
    /// <param name="qualifiers">
    /// The attributes to add.
    /// </param>
    /// <param name="complement">
    /// Complement flag.
    /// </param>
    /// <param name="complementJoin">
    /// Complement join flag.
    /// </param>
    /// <param name="subsequence">
    /// The subsequence.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown if qualifier has more than one value.
    /// </exception>
    /// <returns>
    /// The <see cref="List{Libiada.Database.Models.SequenceAttribute}"/>.
    /// </returns>
    public List<SequenceAttribute> Create(Dictionary<string, List<string>> qualifiers,
                                          bool complement,
                                          bool complementJoin,
                                          Subsequence subsequence)
    {
        List<SequenceAttribute> result = new(qualifiers.Count);

        foreach ((string key, List<string> values) in qualifiers)
        {
            foreach (string value in values)
            {
                if (key == "translation")
                {
                    break;
                }

                if (key == "protein_id")
                {
                    string remoteId = CleanAttributeValue(value);

                    if (!string.IsNullOrEmpty(subsequence.RemoteId) && subsequence.RemoteId != remoteId)
                    {
                        throw new Exception($"Several remote ids in one subsequence. First {subsequence.RemoteId} Second {remoteId}");
                    }

                    subsequence.RemoteId = remoteId;
                }

                result.Add(Create(key, CleanAttributeValue(value), subsequence));
            }
        }

        result.AddRange(CreateComplementJoinPartialAttributes(complement, complementJoin, subsequence));

        return result;
    }

    /// <summary>
    /// The create sequence attribute.
    /// </summary>
    /// <param name="attributeName">
    /// The attribute name.
    /// </param>
    /// <param name="attributeValue">
    /// The attribute value.
    /// </param>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <returns>
    /// The <see cref="SequenceAttribute"/>.
    /// </returns>
    private SequenceAttribute Create(string attributeName, string attributeValue, AbstractSequenceEntity sequence)
    {
        AnnotationAttribute attribute = attributeRepository.GetAttributeByName(attributeName);
        return Create(attribute, attributeValue, sequence);
    }

    /// <summary>
    /// The create sequence attribute.
    /// </summary>
    /// <param name="attribute">
    /// The attribute type.
    /// </param>
    /// <param name="attributeValue">
    /// The attribute value.
    /// </param>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <returns>
    /// The <see cref="SequenceAttribute"/>.
    /// </returns>
    private SequenceAttribute Create(AnnotationAttribute attribute, string attributeValue, AbstractSequenceEntity sequence)
    {
        return new SequenceAttribute
        {
            Attribute = attribute,
            Sequence = sequence,
            Value = attributeValue
        };
    }

    /// <summary>
    /// The create sequence attribute.
    /// </summary>
    /// <param name="attribute">
    /// The attribute type.
    /// </param>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <returns>
    /// The <see cref="SequenceAttribute"/>.
    /// </returns>
    private SequenceAttribute CreateSequenceAttribute(AnnotationAttribute attribute, AbstractSequenceEntity sequence)
    {
        return Create(attribute, string.Empty, sequence);
    }

    /// <summary>
    /// Creates complement, join and partial attributes.
    /// </summary>
    /// <param name="complement">
    /// The complement.
    /// </param>
    /// <param name="complementJoin">
    /// The complement join.
    /// </param>
    /// <param name="subsequence">
    /// The subsequence.
    /// </param>
    /// <returns>
    /// The <see cref="List{Libiada.Database.Models.SequenceAttribute}"/>.
    /// </returns>
    private List<SequenceAttribute> CreateComplementJoinPartialAttributes(bool complement, bool complementJoin, Subsequence subsequence)
    {
        List<SequenceAttribute> result = new(3);
        if (complement)
        {
            result.Add(CreateSequenceAttribute(AnnotationAttribute.Complement, subsequence));

            if (complementJoin)
            {
                result.Add(CreateSequenceAttribute(AnnotationAttribute.ComplementJoin, subsequence));
            }
        }

        if (subsequence.Partial)
        {
            result.Add(CreateSequenceAttribute(AnnotationAttribute.Partial, subsequence));
        }

        return result;
    }
}
