namespace Libiada.Database.Models.Repositories.Sequences;

using SixLabors.ImageSharp;

using System.Text;

using Libiada.Core.Core;
using Libiada.Core.Extensions;
using Libiada.Core.Music;
using Libiada.Core.Images;
using Libiada.Core.Core.SimpleTypes;

using Libiada.Database.Attributes;
using Libiada.Database.Extensions;

using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.PixelFormats;


/// <summary>
/// The sequence repository.
/// </summary>
public class CommonSequenceRepository : SequenceImporter, ICommonSequenceRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSequenceRepository"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public CommonSequenceRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, Cache cache) : base(dbFactory, cache)
    {
    }

    /// <summary>
    /// The get elements.
    /// </summary>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <returns>
    /// The <see cref="element[]"/>.
    /// </returns>
    public Element[] GetElements(long sequenceId)
    {
        long[] elementIds = Db.CommonSequences.Single(cs => cs.Id == sequenceId).Alphabet;
        return ElementRepository.GetElements(elementIds);
    }

    /// <summary>
    /// Loads sequence by id from db and converts it to <see cref="BaseChain"/>.
    /// </summary>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <returns>
    /// The sequence as <see cref="BaseChain"/>.
    /// </returns>
    public BaseChain GetLibiadaBaseChain(long sequenceId)
    {
        int[] order = Db.CommonSequences.Single(cs => cs.Id == sequenceId).Order;
        return new BaseChain(order, GetAlphabet(sequenceId), sequenceId);
    }

    /// <summary>
    /// Loads sequence by id from db and converts it to <see cref="Chain"/>.
    /// </summary>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <returns>
    /// The sequence as <see cref="Chain"/>.
    /// </returns>
    public Chain GetLibiadaChain(long sequenceId)
    {

        if (Db.CommonSequences.Any(s => s.Id == sequenceId))
        {
            var DBSequence = Db.CommonSequences.Include(s => s.Matter).Single(s => s.Id == sequenceId);
            var matter = DBSequence.Matter;
            return new Chain(DBSequence.Order.ToArray(), GetAlphabet(sequenceId), sequenceId);
        }

        // if it is not "real" sequence , then it must be image "sequence" 
        var imageSequence = Db.ImageSequences.Include(s => s.Matter).Single(s => s.Id == sequenceId);
        if (imageSequence.Matter.Nature != Nature.Image)
        {
            throw new Exception("Cannot find sequence to return");
        }

        var image = Image.Load<Rgba32>(imageSequence.Matter.Source);
        var orderExtractor = imageSequence.OrderExtractor.GetAttribute<ImageOrderExtractor, ImageOrderExtractorAttribute>().Value;
        var sequence = ImageProcessor.ProcessImage(image, [], [], (IImageOrderExtractor)Activator.CreateInstance(orderExtractor));
        Alphabet alphabet = [NullValue.Instance()];
        var incompleteAlphabet = sequence.Alphabet;
        for (int j = 0; j < incompleteAlphabet.Cardinality; j++)
        {
            alphabet.Add(incompleteAlphabet[j]);
        }

        return new Chain(sequence.Order, alphabet);
    }

    /// <summary>
    /// Loads sequence by id from db and converts it to <see cref="string"/>.
    /// </summary>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <returns>
    /// The sequence as <see cref="string"/>.
    /// </returns>
    public string GetString(long sequenceId)
    {
        int[] order = Db.CommonSequences.Single(cs => cs.Id == sequenceId).Order;
        Alphabet alphabet = GetAlphabet(sequenceId);
        var stringBuilder = new StringBuilder(order.Length);
        foreach (int element in order)
        {
            stringBuilder.Append(alphabet[element]);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Extracts sequences ids from database.
    /// </summary>
    /// <param name="matterIds">
    /// The matters ids.
    /// </param>
    /// <param name="notations">
    /// The notations ids.
    /// </param>
    /// <param name="languages">
    /// The languages ids.
    /// </param>
    /// <param name="translators">
    /// The translators ids.
    /// </param>
    /// <param name="pauseTreatments">
    /// Pause treatment parameters of music sequences.
    /// </param>
    /// <param name="sequentialTransfers">
    /// Sequential transfer flag used in music sequences.
    /// </param>
    /// <returns>
    /// The sequences ids as <see cref="T:long[][]"/>.
    /// </returns>
    public long[][] GetSequenceIds(
        long[] matterIds,
        Notation[] notations,
        Language[] languages,
        Translator[] translators,
        PauseTreatment[] pauseTreatments,
        bool[] sequentialTransfers,
        ImageOrderExtractor[] imageOrderExtractors)
    {


        var sequenceIds = new long[matterIds.Length][];
        CreateMissingImageSequences(matterIds, notations, imageOrderExtractors);

        for (int i = 0; i < matterIds.Length; i++)
        {
            sequenceIds[i] = new long[notations.Length];
        }
        for (int j = 0; j < notations.Length; j++)
        {
            var sequenceIdsForOneNotation = GetSequenceIds(matterIds,
                                                           notations[j],
                                                           languages.IsNullOrEmpty() ? null : languages[j],
                                                           translators.IsNullOrEmpty() ? null : translators[j],
                                                           pauseTreatments.IsNullOrEmpty() ? null : pauseTreatments[j],
                                                           sequentialTransfers.IsNullOrEmpty() ? null : sequentialTransfers[j],
                                                           imageOrderExtractors.IsNullOrEmpty() ? null : imageOrderExtractors[j]);
            for (int i = 0; i < matterIds.Length; i++)
            {
                sequenceIds[i][j] = sequenceIdsForOneNotation[i];

            }
        }

        return sequenceIds;
    }

    public long[] GetSequenceIds(long[] matterIds,
        Notation notation,
        Language? language,
        Translator? translator,
        PauseTreatment? pauseTreatment,
        bool? sequentialTransfer,
        ImageOrderExtractor? imageOrderExtractor)
    {
        switch (notation.GetNature())
        {
            case Nature.Literature:
                return Db.LiteratureSequences
                         .Where(l => matterIds.Contains(l.MatterId)
                                  && l.Notation == notation
                                  && l.Language == language
                                  && l.Translator == translator)
                         .OrderBy(s => Array.IndexOf(matterIds, s.MatterId))
                         .Select(s => s.Id)
                         .ToArray();
            case Nature.Music:
                return Db.MusicSequences
                         .Where(m => matterIds.Contains(m.MatterId)
                                  && m.Notation == notation
                                  && m.PauseTreatment == pauseTreatment
                                  && m.SequentialTransfer == sequentialTransfer)
                         .OrderBy(s => Array.IndexOf(matterIds, s.MatterId))
                         .Select(s => s.Id)
                         .ToArray();
            case Nature.Image:
                return Db.ImageSequences
                         .Where(c => matterIds.Contains(c.MatterId)
                                  && c.Notation == notation
                                  && c.OrderExtractor == imageOrderExtractor)
                         .OrderBy(s =>Array.IndexOf(matterIds, s.MatterId))
                         .Select(s => s.Id)
                         .ToArray();
            default:
                return Db.CommonSequences
                         .Where(c => matterIds.Contains(c.MatterId) && c.Notation == notation)
                         .OrderBy(s => Array.IndexOf(matterIds, s.MatterId))
                         .Select(s => s.Id)
                         .ToArray();
        }
    }

    /// <summary>
    /// The get alphabet.
    /// </summary>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <returns>
    /// The <see cref="Alphabet"/>.
    /// </returns>
    private Alphabet GetAlphabet(long sequenceId)
    {
        long[] elements = Db.CommonSequences.Single(cs => cs.Id == sequenceId).Alphabet;
        return ElementRepository.ToLibiadaAlphabet(elements);
    }

    /// <summary>
    /// Creates image sequences for given order reading trajectories
    /// if they are missing in database.
    /// </summary>
    /// <param name="matterIds">
    /// Matters ids.
    /// </param>
    /// <param name="notations">
    /// notations of images.
    /// </param>
    /// <param name="imageOrderExtractors">
    /// Reading trajectories.
    /// </param>
    private void CreateMissingImageSequences(long[] matterIds, Notation[] notations, ImageOrderExtractor[] imageOrderExtractors)
    {
        if (notations[0].GetNature() == Nature.Image)
        {
            var existingSequences = Db.ImageSequences
                .Where(s => matterIds.Contains(s.MatterId)
                         && notations.Contains(s.Notation)
                         && imageOrderExtractors.Contains(s.OrderExtractor))
                .ToList();

            for (int i = 0; i < matterIds.Length; i++)
            {
                for (int j = 0; j < notations.Length; j++)
                {
                    if (!existingSequences.Any(s => s.MatterId == matterIds[i]
                                                 && s.Notation == notations[j]
                                                 && s.OrderExtractor == imageOrderExtractors[j]))
                    {
                        var newImageSequence = new ImageSequence
                        {
                            MatterId = matterIds[i],
                            Notation = notations[j],
                            OrderExtractor = imageOrderExtractors.IsNullOrEmpty() ? ImageOrderExtractor.LineLeftToRightTopToBottom : imageOrderExtractors[j]
                        };

                        Db.ImageSequences.Add(newImageSequence);
                        existingSequences.Add(newImageSequence);
                    }
                }
            }

            Db.SaveChanges();
        }
    }
}
