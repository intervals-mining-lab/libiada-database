namespace Libiada.Database.Models.Repositories.Sequences;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;

using Libiada.Database.Helpers;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The literature sequence repository.
/// </summary>
public class LiteratureSequenceRepository : SequenceImporter, ILiteratureSequenceRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiteratureSequenceRepository"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public LiteratureSequenceRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, Cache cache) : base(dbFactory, cache)
    {
    }

    /// <summary>
    /// Creates literature sequence in database.
    /// </summary>
    /// <param name="sequence">
    /// The literature sequence to create in database.
    /// </param>
    /// <param name="sequenceStream">
    /// The sequence stream.
    /// </param>
    /// <param name="language">
    /// The language id.
    /// </param>
    /// <param name="original">
    /// The original.
    /// </param>
    /// <param name="translator">
    /// The translator id.
    /// </param>
    /// <param name="dropPunctuation">
    /// Flag indicating if punctuation should be removed from text.
    /// </param>
    public void Create(LiteratureSequence sequence, Stream sequenceStream, bool dropPunctuation = false)
    {
        string stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);
        BaseChain chain;
        if (sequence.Notation == Notation.Letters)
        {
            stringSequence = stringSequence.ToUpper();
            if (dropPunctuation)
            {
                stringSequence = new string(stringSequence.Where(c => !char.IsPunctuation(c)).ToArray());
            }
            chain = new BaseChain(stringSequence);
        }
        else
        {
            // file always contains empty string at the end
            // TODO: rewrite this, add empty string check at the end or write a normal trim
            string[] text = stringSequence.Split(['\n', '\r', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            chain = new BaseChain(text.Select(e => (ValueString)e).ToList());
        }

        CombinedSequenceEntity dbSequence = sequence.ToCombinedSequence();

        MatterRepository.CreateOrExtractExistingMatterForSequence(dbSequence);

        sequence.Alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.Notation, true);
        sequence.Order = chain.Order;

        Create(sequence);
    }

    /// <summary>
    /// The insert.
    /// </summary>
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    /// <param name="original">
    /// The original.
    /// </param>
    /// <param name="language">
    /// The language id.
    /// </param>
    /// <param name="translator">
    /// The translator id.
    /// </param>
    /// <param name="alphabet">
    /// The alphabet.
    /// </param>
    /// <param name="order">
    /// The order.
    /// </param>
    public void Create(LiteratureSequence sequence)
    {
        CombinedSequenceEntity dbSequence = sequence.ToCombinedSequence();

        Db.CombinedSequenceEntities.Add(dbSequence);
        Db.SaveChanges();
        sequence.Id = dbSequence.Id;
    }
}
