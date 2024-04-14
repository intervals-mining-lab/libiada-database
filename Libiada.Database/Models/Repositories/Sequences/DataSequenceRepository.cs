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
    public DataSequenceRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, Cache cache) : base(dbFactory, cache)
    {
    }

    /// <summary>
    /// Create data sequence and matter.
    /// </summary>
    /// <param name="sequence">
    /// The common sequence.
    /// </param>
    /// <param name="sequenceStream">
    /// The sequence stream.
    /// </param>
    /// <param name="precision">
    /// Precision of data sequence.
    /// </param>
    public void Create(CommonSequence sequence, Stream sequenceStream, int precision)
    {
        string stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);

        string[] text = stringSequence.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

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

        BaseChain chain = new(elements);

        MatterRepository.CreateOrExtractExistingMatterForSequence(sequence);

        long[] alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.Notation, true);
        DataSequence dataSequence = new()
        {
            Alphabet = alphabet,
            Order = chain.Order,
            Notation = sequence.Notation,
            Description = sequence.Description,
            MatterId = sequence.MatterId,
            RemoteDb = sequence.RemoteDb,
            RemoteId = sequence.RemoteId
        };

        Db.DataSequences.Add(dataSequence);
        Db.SaveChanges();
        sequence.Id = dataSequence.Id;
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
    public void Create(CommonSequence sequence)
    {
        DataSequence dataSequence = new()
        {
            Alphabet = sequence.Alphabet,
            Order = sequence.Order,
            Notation = sequence.Notation,
            Description = sequence.Description,
            MatterId = sequence.MatterId,
            RemoteDb = sequence.RemoteDb,
            RemoteId = sequence.RemoteId
        };

        Db.DataSequences.Add(dataSequence);
        Db.SaveChanges();
        sequence.Id = dataSequence.Id;
    }
}
