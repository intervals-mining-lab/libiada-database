namespace Libiada.Database.Models.Repositories.Sequences;

using Bio;
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
        var allExistingAccessions = Db.DnaSequences
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
    /// The common sequence.
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
    public void Create(CommonSequence sequence, ISequence fastaSequence, bool partial)
    {
        if (fastaSequence.ID.Contains("Resource temporarily unavailable"))
        {
            throw new Exception("Sequence is empty or invalid (probably ncbi is not responding).");
        }

        string stringSequence = fastaSequence.ConvertToString().ToUpper();

        var chain = new BaseChain(stringSequence);

        if (!ElementRepository.ElementsInDb(chain.Alphabet, sequence.Notation))
        {
            throw new Exception("At least one element of new sequence is invalid (not A, C, T, G or U).");
        }

        MatterRepository.CreateOrExtractExistingMatterForSequence(sequence);
        sequence.Alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.Notation, false);
        sequence.Order = chain.Order.ToList();

        Create(sequence, partial);
    }

    /// <summary>
    /// The insert.
    /// </summary>
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    public void Create(CommonSequence sequence, bool partial)
    {
        var dnaSequence = new DnaSequence
        {
            MatterId = sequence.MatterId,
            Notation = sequence.Notation,
            RemoteDb = sequence.RemoteDb,
            RemoteId = sequence.RemoteId,
            Description = sequence.Description,
            Alphabet = sequence.Alphabet,
            Order = sequence.Order,
            Partial = partial
        };

        Db.DnaSequences.Add(dnaSequence);
        Db.SaveChanges();
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
        var chains = new long[matterIds.Length];
        DnaSequence[] sequences = Db.DnaSequences.Where(c => matterIds.Contains(c.MatterId) && c.Notation == Notation.Nucleotides).ToArray();
        for (int i = 0; i < matterIds.Length; i++)
        {
            chains[i] = sequences.Single(c => c.MatterId == matterIds[i]).Id;
        }

        return chains;
    }

    /// <summary>
    /// The dispose.
    /// </summary>
    public void Dispose()
    {
    }
}
