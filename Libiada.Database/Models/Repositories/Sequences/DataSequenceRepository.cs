namespace Libiada.Database.Models.Repositories.Sequences;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;

using Libiada.Database.Helpers;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The data sequence repository.
/// </summary>
public class DataSequenceRepository : SequenceImporter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSequenceRepository"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public DataSequenceRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, IResearchObjectsCache cache) : base(dbFactory, cache)
    {
    }

    /// <summary>
    /// Create data sequence and research object.
    /// </summary>
    /// <param name="sequence">
    /// The data sequence to create in database.
    /// </param>
    /// <param name="sequenceStream">
    /// The sequence stream.
    /// </param>
    /// <param name="precision">
    /// Precision of data sequence.
    /// </param>
    public void Create(DataSequence sequence, Stream sequenceStream, int precision)
    {
        string stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);

        string[] text = stringSequence.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        string[] cleanedSequence = text.Where(t => !t.Equals("\"volume\"") && !string.IsNullOrEmpty(t) && !string.IsNullOrWhiteSpace(t)).ToArray();

        List<IBaseObject> elements = new(cleanedSequence.Length);

        for (int i = 0; i < cleanedSequence.Length; i++)
        {
            string element = cleanedSequence[i];
            if (element.Substring(element.Length - 2, 2).Equals(".0"))
            {
                cleanedSequence[i] = cleanedSequence[i].Substring(0, cleanedSequence[i].Length - 2);
            }

            int intElement = int.Parse(cleanedSequence[i]);
            int multiplier = (int)Math.Pow(10, precision);
            intElement /= multiplier;
            intElement *= multiplier;

            elements.Add(new ValueInt(intElement));
        }

        Sequence libiadaSequence = new(elements);

        long[] alphabet = ElementRepository.ToDbElements(libiadaSequence.Alphabet, sequence.Notation, true);
        CombinedSequenceEntity dbSequence = sequence.ToCombinedSequence();

        ResearchObjectRepository.CreateOrExtractExistingResearchObjectForSequence(dbSequence);

        Create(sequence);
    }

    /// <summary>
    /// Create sequence.
    /// </summary>
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    /// <param name="alphabet">
    /// The sequence's alphabet.
    /// </param>
    /// <param name="order">
    /// The sequence's order.
    /// </param>
    public void Create(DataSequence sequence)
    {
        CombinedSequenceEntity dbSequence = sequence.ToCombinedSequence();

        Db.CombinedSequenceEntities.Add(dbSequence);
        Db.SaveChanges();
        sequence.Id = dbSequence.Id;
    }
}
