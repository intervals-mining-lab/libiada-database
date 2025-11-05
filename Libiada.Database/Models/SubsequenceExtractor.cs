namespace Libiada.Database.Models;

using Bio.Extensions;

using Libiada.Core.Core;
using Libiada.Core.Extensions;

using Libiada.Database.Models.Repositories.Sequences;

using Microsoft.EntityFrameworkCore;

using BioSequence = Bio.Sequence;
using IBioSequence = Bio.ISequence;


/// <summary>
/// The subsequence extractor.
/// </summary>
public class SubsequenceExtractor
{
    /// <summary>
    /// Database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICombinedSequenceEntityRepository sequenceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubsequenceExtractor"/> class.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    public SubsequenceExtractor(LibiadaDatabaseEntities db, ICombinedSequenceEntityRepository sequenceRepository)
    {
        this.db = db;
        this.sequenceRepository = sequenceRepository;
    }

    /// <summary>
    /// Extracts sequences for given subsequences from database.
    /// </summary>
    /// <param name="subsequences">
    /// The subsequences.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{long, Libiada.Core.Core.ComposedSequence}"/>.
    /// </returns>
    public Dictionary<long, ComposedSequence> GetSubsequencesSequences(Subsequence[] subsequences)
    {
        long[] parentSequenceIds = subsequences.Select(s => s.SequenceId).Distinct().ToArray();
        Dictionary<long, BioSequence> parentSequences = [];
        foreach (long id in parentSequenceIds)
        {
            parentSequences[id] = GetDotNetBioSequence(id);
        }

        Dictionary<long, ComposedSequence> result = [];

        foreach (Subsequence subsequence in subsequences)
        {
            ComposedSequence sequence = GetSequence(parentSequences[subsequence.SequenceId], subsequence);
            result[subsequence.Id] = sequence;
        }

        return result;
    }

    /// <summary>
    /// Extracts sequence for given subsequence from database.
    /// </summary>
    /// <param name="subsequence">
    /// Subsequence to be extracted from database.
    /// </param>
    /// <returns></returns>
    public ComposedSequence GetSubsequenceSequence(Subsequence subsequence)
    {
        BioSequence sourceSequence = GetDotNetBioSequence(subsequence.SequenceId);
        return GetSequence(sourceSequence, subsequence);
    }

    /// <summary>
    /// The extract sequences.
    /// </summary>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <param name="features">
    /// The feature ids.
    /// </param>
    /// <returns>
    /// The <see cref="List{Libiada.Database.Models.Subsequence}"/>.
    /// </returns>
    public Subsequence[] GetSubsequences(long sequenceId, IReadOnlyList<Feature> features)
    {
        Feature[] allFeatures = EnumExtensions.ToArray<Feature>();
        if (allFeatures.Length == features.Count)
        {
            return db.Subsequences.Where(s => s.SequenceId == sequenceId)
                .Include(s => s.Positions)
                .Include(s => s.SequenceAttributes)
                .ToArray();
        }

        if (allFeatures.Length - 1 == features.Count)
        {
            Feature exceptFeature = allFeatures.Except(features).Single();

            return db.Subsequences.Where(s => s.SequenceId == sequenceId && s.Feature != exceptFeature)
                .Include(s => s.Positions)
                .Include(s => s.SequenceAttributes)
                .ToArray();
        }

        return db.Subsequences.Where(s => s.SequenceId == sequenceId && features.Contains(s.Feature))
                             .Include(s => s.Positions)
                             .Include(s => s.SequenceAttributes)
                             .ToArray();
    }

    /// <summary>
    /// Extracts only filtered subsequences.
    /// </summary>
    /// <param name="sequenceId">
    /// Sequences id.
    /// </param>
    /// <param name="features">
    /// Subsequences features.
    /// </param>
    /// <param name="filters">
    /// Filters for the subsequences.
    /// Filters are applied in "OR" logic 
    /// (if subsequence corresponds to any filter it is added to calculation).
    /// Product, Gene, Note and LocusTag fields are used.
    /// </param>
    /// <returns>
    /// Array of subsequences.
    /// </returns>
    public Subsequence[] GetSubsequences(long sequenceId, IReadOnlyList<Feature> features, string[] filters)
    {
        filters = filters.ConvertAll(f => f.ToLowerInvariant()).ToArray();
        List<Subsequence> result = [];
        Subsequence[] allSubsequences = GetSubsequences(sequenceId, features);

        foreach (Subsequence subsequence in allSubsequences)
        {
            if (IsSubsequenceAttributePassesFilters(subsequence, AnnotationAttribute.Product, filters)
             || IsSubsequenceAttributePassesFilters(subsequence, AnnotationAttribute.Gene, filters)
             || IsSubsequenceAttributePassesFilters(subsequence, AnnotationAttribute.Note, filters)
             || IsSubsequenceAttributePassesFilters(subsequence, AnnotationAttribute.LocusTag, filters))
            {
                result.Add(subsequence);
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Extracts <see cref="BioSequence"/> for given subsequences ids
    /// and formats header sutable for fasta file.
    /// </summary>
    /// <param name="subsequencesIds">
    /// Subsequences ids.
    /// </param>
    /// <returns>
    /// Array of <see cref="ISequence"/> for given ids.
    /// </returns>
    public IBioSequence[] GetBioSequencesForFastaConverter(long[] subsequencesIds)
    {
        Subsequence[] subsequences;
        Dictionary<long, ComposedSequence> sequences;
        Dictionary<long, string> researchObjectsNames;
        subsequences = db.Subsequences.Where(s => subsequencesIds.Contains(s.Id))
                         .Include(s => s.Positions)
                         .Include(s => s.SequenceAttributes)
                         .ToArray();
        sequences = GetSubsequencesSequences(subsequences);
        long[] parentIds = subsequences.Select(s => s.SequenceId).ToArray();
        researchObjectsNames = db.CombinedSequenceEntities
                         .Include(ds => ds.ResearchObject)
                         .Where(ds => parentIds.Contains(ds.Id))
                         .ToDictionary(ds => ds.Id, ds => ds.ResearchObject.Name);

        IBioSequence[] bioSequences = new IBioSequence[subsequences.Length];
        for (int i = 0; i < subsequences.Length; i++)
        {
            Subsequence subsequence = subsequences[i];
            BioSequence bioSequence = new(Bio.Alphabets.DNA, sequences[subsequence.Id].ToString())
            {
                ID = $"{researchObjectsNames[subsequence.SequenceId].Replace(' ', '_')}?from={subsequence.Start}to={subsequence.Start + subsequence.Length}"
            };
            bioSequences[i] = bioSequence;
        }

        return bioSequences;
    }

    /// <summary>
    ///Extracts subsequence from given parent sequence.
    /// </summary>
    /// <param name="source">
    /// Parent sequence for extraction.
    /// </param>
    /// <param name="subsequence">
    /// Subsequence to be extracted from parent sequence.
    /// </param>
    /// <returns>
    /// Extracted from given position sequence as <see cref="ComposedSequence"/>.
    /// </returns>
    private ComposedSequence GetSequence(BioSequence source, Subsequence subsequence)
    {
        if (subsequence.Positions.Count == 0)
        {
            return GetSimpleSubsequence(source, subsequence);
        }
        else
        {
            return GetJoinedSubsequence(source, subsequence);
        }
    }

    /// <summary>
    /// Extracts .net bio <see cref="BioSequence"/> from database.
    /// </summary>
    /// <param name="sequenceId">
    /// Id of the sequence to be retrieved from database.
    /// </param>
    /// <returns>
    /// Subsequence as .net bio <see cref="BioSequence"/>.
    /// </returns>
    private BioSequence GetDotNetBioSequence(long sequenceId)
    {
        string parentSequence = sequenceRepository.GetString(sequenceId);
        return new BioSequence(Bio.Alphabets.DNA, parentSequence);
    }

    /// <summary>
    /// Checks if subsequence attribute passes filters.
    /// </summary>
    /// <param name="subsequence">
    /// The subsequence.
    /// </param>
    /// <param name="attribute">
    /// The attribute.
    /// </param>
    /// <param name="filters">
    /// The filters.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    private bool IsSubsequenceAttributePassesFilters(Subsequence subsequence, AnnotationAttribute attribute, string[] filters)
    {
        if (subsequence.SequenceAttributes.Any(sa => sa.Attribute == attribute))
        {
            string value = subsequence.SequenceAttributes.Single(sa => sa.Attribute == attribute).Value.ToLowerInvariant();
            return filters.Any(f => value.Contains(f));
        }

        return false;
    }

    /// <summary>
    /// Extracts subsequence without joins (additional positions).
    /// </summary>
    /// <param name="sourceSequence">
    /// The complete sequence.
    /// </param>
    /// <param name="subsequence">
    /// The subsequence.
    /// </param>
    /// <returns>
    /// The <see cref="ComposedSequence"/>.
    /// </returns>
    private ComposedSequence GetSimpleSubsequence(BioSequence sourceSequence, Subsequence subsequence)
    {
        IBioSequence bioSequence = sourceSequence.GetSubSequence(subsequence.Start, subsequence.Length);

        if (subsequence.SequenceAttributes.Any(sa => sa.Attribute == AnnotationAttribute.Complement))
        {
            bioSequence = bioSequence.GetReverseComplementedSequence();
        }

        return new ComposedSequence(bioSequence.ConvertToString());
    }

    /// <summary>
    /// Extracts joined subsequence.
    /// </summary>
    /// <param name="sourceSequence">
    /// The complete sequence.
    /// </param>
    /// <param name="subsequence">
    /// The subsequence.
    /// </param>
    /// <returns>
    /// The <see cref="ComposedSequence"/>.
    /// </returns>
    private ComposedSequence GetJoinedSubsequence(BioSequence sourceSequence, Subsequence subsequence)
    {
        if (subsequence.SequenceAttributes.Any(sa => sa.Attribute == AnnotationAttribute.Complement))
        {
            return GetJoinedSubsequenceWithComplement(sourceSequence, subsequence);
        }
        else
        {
            return GetJoinedSubsequenceWithoutComplement(sourceSequence, subsequence);
        }
    }

    /// <summary>
    /// Extracts joined subsequence without complement flag.
    /// </summary>
    /// <param name="sourceSequence">
    /// The complete sequence.
    /// </param>
    /// <param name="subsequence">
    /// The subsequence.
    /// </param>
    /// <returns>
    /// The <see cref="ComposedSequence"/>.
    /// </returns>
    private ComposedSequence GetJoinedSubsequenceWithoutComplement(BioSequence sourceSequence, Subsequence subsequence)
    {
        string joinedSequence = sourceSequence.GetSubSequence(subsequence.Start, subsequence.Length).ConvertToString();

        Position[] positions = subsequence.Positions.ToArray();

        foreach (Position position in positions)
        {
            joinedSequence += sourceSequence.GetSubSequence(position.Start, position.Length).ConvertToString();
        }

        return new ComposedSequence(joinedSequence);
    }

    /// <summary>
    /// Extracts joined subsequence with complement flag.
    /// </summary>
    /// <param name="sourceSequence">
    /// The complete sequence.
    /// </param>
    /// <param name="subsequence">
    /// The subsequence.
    /// </param>
    /// <returns>
    /// The <see cref="ComposedSequence"/>.
    /// </returns>
    private ComposedSequence GetJoinedSubsequenceWithComplement(BioSequence sourceSequence, Subsequence subsequence)
    {
        IBioSequence bioSequence = sourceSequence.GetSubSequence(subsequence.Start, subsequence.Length);
        Position[] positions = subsequence.Positions.ToArray();
        string resultSequence;

        if (subsequence.SequenceAttributes.Any(sa => sa.Attribute == AnnotationAttribute.ComplementJoin))
        {
            string joinedSequence = bioSequence.ConvertToString();

            foreach (Position position in positions)
            {
                joinedSequence += sourceSequence.GetSubSequence(position.Start, position.Length).ConvertToString();
            }

            resultSequence = new BioSequence(Bio.Alphabets.DNA, joinedSequence).GetReverseComplementedSequence().ConvertToString();
        }
        else
        {
            resultSequence = bioSequence.GetReverseComplementedSequence().ConvertToString();

            foreach (Position position in positions)
            {
                resultSequence += sourceSequence.GetSubSequence(position.Start, position.Length).GetReverseComplementedSequence().ConvertToString();
            }
        }

        return new ComposedSequence(resultSequence);
    }
}
