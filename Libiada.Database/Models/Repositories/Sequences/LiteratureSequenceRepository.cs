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
    /// <param name="commonSequence">
    /// The common sequence.
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
    public void Create(CommonSequence commonSequence, Stream sequenceStream, Language language, bool original, Translator translator, bool dropPunctuation = false)
    {
        string stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);
        BaseChain chain;
        if (commonSequence.Notation == Notation.Letters)
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
            string[] text = stringSequence.Split(new[] { '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            chain = new BaseChain(text.Select(e => (ValueString)e).ToList());
        }

        MatterRepository.CreateOrExtractExistingMatterForSequence(commonSequence);

        commonSequence.Alphabet = ElementRepository.ToDbElements(chain.Alphabet, commonSequence.Notation, true);
        commonSequence.Order = chain.Order;

        Create(commonSequence, original, language, translator);
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
    public void Create(CommonSequence sequence, bool original, Language language, Translator translator)
    {
        var literatureSequence = new LiteratureSequence
        {
            MatterId = sequence.MatterId,
            Alphabet = sequence.Alphabet,
            Order = sequence.Order,
            Notation = sequence.Notation,
            Description = sequence.Description,
            RemoteDb = sequence.RemoteDb,
            RemoteId = sequence.RemoteId,
            Language = language,
            Translator = translator,
            Original = original
        };

        Db.LiteratureSequences.Add(literatureSequence);
        Db.SaveChanges();
    }

    /// <summary>
    /// The dispose.
    /// </summary>
    public void Dispose()
    {
    }
}
