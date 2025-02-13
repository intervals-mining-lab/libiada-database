namespace Libiada.Database.Models.Repositories.Sequences;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

using Bio.IO.GenBank;

using Libiada.Core.Extensions;

using Libiada.Database.Models.Repositories.Catalogs;

/// <summary>
/// The research object repository.
/// </summary>
public class ResearchObjectRepository : IResearchObjectRepository
{
    /// <summary>
    /// GenBank date formats.
    /// </summary>
    private readonly string[] GenBankDateFormats = ["dd-MMM-yyyy", "MMM-yyyy", "yyyy", "yyyy-MM-ddTHH:mmZ", "yyyy-MM-ddTHHZ", "yyyy-MM-dd", "yyyy-MM"];

    /// <summary>
    /// Database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResearchObjectRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    public ResearchObjectRepository(LibiadaDatabaseEntities db, IResearchObjectsCache cache)
    {
        this.db = db;
        this.cache = cache;
    }

    /// <summary>
    /// Determines group and sequence type parameters for a research object.
    /// </summary>
    /// <param name="name">
    /// The research object's name.
    /// </param>
    /// <param name="nature">
    /// Nature of the research object.
    /// </param>
    /// <returns>
    /// Tuple of <see cref="Group"/> and <see cref="SequenceType"/>.
    /// </returns>
    public static (Group, SequenceType) GetGroupAndSequenceType(string name, Nature nature)
    {
        name = name.ToLower();
        switch (nature)
        {
            case Nature.Literature:
                return (Group.ClassicalLiterature, SequenceType.CompleteText);
            case Nature.Music:
                return (Group.ClassicalMusic, SequenceType.CompleteMusicalComposition);
            case Nature.MeasurementData:
                return (Group.ObservationData, SequenceType.CompleteNumericSequence);
            case Nature.Image:
                // TODO: add distinction between photo and picture, painting and photo
                return (Group.Picture, SequenceType.CompleteImage);
            case Nature.Genetic:
                if (name.Contains("mitochondrion") || name.Contains("mitochondrial"))
                {
                    SequenceType sequenceType = name.Contains("16s") ? SequenceType.Mitochondrion16SRRNA
                                              : name.Contains("plasmid") ? SequenceType.MitochondrialPlasmid
                                              : SequenceType.MitochondrialGenome;
                    return (Group.Eucariote, sequenceType);
                }
                else if (name.Contains("18s"))
                {
                    return (Group.Eucariote, SequenceType.RRNA18S);
                }
                else if (name.Contains("chloroplast"))
                {
                    return (Group.Eucariote, SequenceType.ChloroplastGenome);
                }
                else if (name.Contains("plastid") || name.Contains("apicoplast"))
                {
                    return (Group.Eucariote, SequenceType.Plastid);
                }
                else if (name.Contains("plasmid"))
                {
                    return (Group.Bacteria, SequenceType.Plasmid);
                }
                else if (name.Contains("16s"))
                {
                    return (Group.Bacteria, SequenceType.RRNA16S);
                }
                else
                {
                    Group group = name.Contains("virus") || name.Contains("viroid") || name.Contains("phage") ? Group.Virus
                                : name.Contains("archaea") ? Group.Archaea
                                                           : Group.Bacteria;
                    return (group, SequenceType.CompleteGenome);
                }
            default:
                throw new InvalidEnumArgumentException(nameof(nature), (int)nature, typeof(Nature));
        }
    }

    /// <summary>
    /// Trims the name ending of the GenBank sequence.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <returns>
    /// Trimmed name as <see cref="string"/>
    /// </returns>
    public static string TrimGenBankNameEnding(string name)
    {
        return name.TrimEnd('.')
                   .TrimEnd(", complete genome")
                   .TrimEnd(", complete sequence")
                   .TrimEnd(", complete CDS")
                   .TrimEnd(", complete cds")
                   .TrimEnd(", genome");
    }

    /// <summary>
    /// Creates new researchoObject or extracts existing research object from database.
    /// </summary>
    /// <param name="sequence">
    /// The sequence to be used for research object creation or extraction.
    /// </param>
    public void CreateOrExtractExistingResearchObjectForSequence(CombinedSequenceEntity sequence)
    {
        ResearchObject researchObject = sequence.ResearchObject;
        if (researchObject != null)
        {
            researchObject.Sequences = new Collection<CombinedSequenceEntity>();
            sequence.ResearchObjectId = SaveToDatabase(researchObject);
        }
        else
        {
            sequence.ResearchObject = db.ResearchObjects.Single(m => m.Id == sequence.ResearchObjectId);
        }
    }

    /// <summary>
    /// Creates research object from genBank metadata.
    /// </summary>
    /// <param name="metadata">
    /// The metadata.
    /// </param>
    /// <returns>
    /// The <see cref="ResearchObject"/>.
    /// </returns>
    public ResearchObject CreateResearchObjectFromGenBankMetadata(GenBankMetadata metadata)
    {
        FeatureItem[] sources = metadata.Features.All.Where(f => f.Key == "source").ToArray();
        string collectionCountry = SequenceAttributeRepository.GetAttributeSingleValue(sources, "country");
        string collectionCoordinates = SequenceAttributeRepository.GetAttributeSingleValue(sources, "lat_lon");

        string collectionDateValue = SequenceAttributeRepository.GetAttributeSingleValue(sources, "collection_date")?.Split('/')[0];
        bool hasCollectionDate = DateTime.TryParseExact(collectionDateValue, GenBankDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime collectionDate);
        if (!string.IsNullOrEmpty(collectionDateValue) && !hasCollectionDate)
        {
            throw new Exception($"Collection date was invalid. Value: {collectionDateValue}.");
        }

        string species = metadata.Source.Organism.Species;
        string commonName = metadata.Source.CommonName;
        string definition = metadata.Definition;

        ResearchObject researchObject = new()
        {
            Name = $"{ExtractResearchObjectName(species, commonName, definition)} | {metadata.Version.CompoundAccession}",
            Nature = Nature.Genetic,
            CollectionCountry = collectionCountry,
            CollectionLocation = collectionCoordinates,
            CollectionDate = hasCollectionDate ? DateOnly.FromDateTime(collectionDate) : null
        };

        (researchObject.Group, researchObject.SequenceType) = GetGroupAndSequenceType($"{species} {commonName} {definition}", researchObject.Nature);

        return researchObject;
    }

    /// <summary>
    /// Adds given research object to database.
    /// </summary>
    /// <param name="researchObject">
    /// The research object.
    /// </param>
    /// <returns>
    /// The <see cref="long"/>.
    /// </returns>
    public long SaveToDatabase(ResearchObject researchObject)
    {
        db.ResearchObjects.Add(researchObject);
        db.SaveChanges();
        cache.Clear();
        return researchObject.Id;
    }

    /// <summary>
    /// Removes sequence type from the the sequence name.
    /// </summary>
    /// <param name="name">The GenBank sequence name.</param>
    /// <returns>
    /// Cleaned up name as <see cref="string"/>.
    /// </returns>
    private static string RemoveSequenceTypeFromName(string name)
    {
        return name.Replace("mitochondrion", "")
                   .Replace("plastid", "")
                   .Replace("plasmid", "")
                   .Replace("chloroplast", "")
                   .Replace("  ", " ")
                   .Trim();
    }

    /// <summary>
    /// Extracts supposed sequence name from metadata.
    /// </summary>
    /// <param name="metadata">
    /// The metadata.
    /// </param>
    /// <returns>
    /// Supposed name as <see cref="string"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if all name fields are contradictory.
    /// </exception>
    private static string ExtractResearchObjectName(string species, string commonName, string definition)
    {
        species = RemoveSequenceTypeFromName(species.GetLargestRepeatingSubstring());
        commonName = RemoveSequenceTypeFromName(commonName);
        definition = RemoveSequenceTypeFromName(TrimGenBankNameEnding(definition));

        if (commonName.Contains(definition) || definition.IsSubsetOf(commonName))
        {
            if (species.Contains(commonName) || commonName.IsSubsetOf(species))
            {
                return species;
            }

            if (commonName.Contains(species) || species.IsSubsetOf(commonName))
            {
                return commonName;
            }

            return $"{commonName} | {species}";
        }

        if (definition.Contains(commonName) || commonName.IsSubsetOf(definition))
        {
            if (species.Contains(definition) || definition.IsSubsetOf(species))
            {
                return species;
            }

            if (definition.Contains(species) || species.IsSubsetOf(definition))
            {
                return definition;
            }

            return $"{species} | {definition}";
        }

        if (commonName.Contains(species) || species.IsSubsetOf(commonName))
        {
            return $"{commonName} | {definition}";
        }

        if (species.Contains(commonName) || commonName.IsSubsetOf(species))
        {
            return $"{species} | {definition}";
        }

        if (species.Contains(definition) || definition.IsSubsetOf(species))
        {
            return $"{commonName} | {species}";
        }

        throw new Exception($"Sequences names are not equal. CommonName = {commonName}, Species = {species}, Definition = {definition}");
    }
}
