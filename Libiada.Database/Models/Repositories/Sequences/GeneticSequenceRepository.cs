namespace Libiada.Database.Models.Repositories.Sequences;

using Bio.Extensions;

using Libiada.Core.Core;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The DNA sequence repository.
/// </summary>
public class GeneticSequenceRepository : SequenceImporter, IGeneticSequenceRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneticSequenceRepository"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public GeneticSequenceRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, Cache cache) : base(dbFactory, cache)
    {
    }

    /// <summary>
    /// Splits given accessions into existing and not imported.
    /// </summary>
    /// <param name="accessions">
    /// The accessions.
    /// </param>
    /// <returns>
    /// First array in tuple are existing accessions and second array are not imported accessions.
    /// </returns>
    public (string[], string[]) SplitAccessionsIntoExistingAndNotImported(string[] accessions)
    {
        var allExistingAccessions = Db.CombinedSequenceEntities
                                      .Where(d => d.RemoteId != null)
                                      .Select(d => d.RemoteId)
                                      .ToArray()
                                      .Select(r => r.Split('.')[0])
                                      .Distinct();
        var existing = accessions.Intersect(allExistingAccessions);
        var notExisting = accessions.Except(allExistingAccessions);

        return (existing.ToArray(), notExisting.ToArray());
    }

    /// <summary>
    /// The create DNA sequence.
    /// </summary>
    /// <param name="sequence">
    /// The genetic sequence to create in database.
    /// </param>
    /// <param name="fastaSequence">
    /// Sequence as <see cref="ISequence"/>>.
    /// </param>
    /// <param name="partial">
    /// The partial.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown if at least one element of new sequence is missing in db
    /// or if sequence is empty or invalid.
    /// </exception>
    public void Create(DnaSequence sequence, Bio.ISequence fastaSequence)
    {
        if (fastaSequence.ID.Contains("Resource temporarily unavailable"))
        {
            throw new Exception("Sequence is empty or invalid (probably ncbi is not responding).");
        }

        string stringSequence = fastaSequence.ConvertToString().ToUpper();

        BaseChain chain = new(stringSequence);

        if (!ElementRepository.ElementsInDb(chain.Alphabet, sequence.Notation))
        {
            throw new Exception("At least one element of new sequence is invalid (not A, C, T, G or U).");
        }

        CombinedSequenceEntity dbSequence = sequence.ToCombinedSequence();

        MatterRepository.CreateOrExtractExistingMatterForSequence(dbSequence);
        sequence.Alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.Notation, false);
        sequence.Order = chain.Order;

        Create(sequence);
    }

    /// <summary>
    /// The insert.
    /// </summary>
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    public void Create(DnaSequence sequence)
    {
        CombinedSequenceEntity dbSequence = sequence.ToCombinedSequence();

        Db.CombinedSequenceEntities.Add(dbSequence);
        Db.SaveChanges();
        sequence.Id = dbSequence.Id;
    }

    /// <summary>
    /// Extracts nucleotide sequences ids from database.
    /// </summary>
    /// <param name="matterIds">
    /// The matter ids.
    /// </param>
    /// <returns>
    /// The <see cref="T:long[]"/>.
    /// </returns>
    public long[] GetNucleotideSequenceIds(long[] matterIds)
    {
        long[] chains = new long[matterIds.Length];
        CombinedSequenceEntity[] sequences = Db.CombinedSequenceEntities
                                               .Where(c => matterIds.Contains(c.MatterId) && c.Notation == Notation.Nucleotides)
                                               .ToArray();
        
        // TODO: use orderby insted of cycle
        for (int i = 0; i < matterIds.Length; i++)
        {
            chains[i] = sequences.Single(c => c.MatterId == matterIds[i]).Id;
        }

        return chains;
    }
}
