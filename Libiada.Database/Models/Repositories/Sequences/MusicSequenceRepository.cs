namespace Libiada.Database.Models.Repositories.Sequences;

using System.Collections.Generic;
using System.Xml;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;
using Libiada.Core.Extensions;
using Libiada.Core.Music;
using Libiada.Core.Music.MusicXml;

using Libiada.Database.Helpers;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The music sequence repository.
/// </summary>
public class MusicSequenceRepository : SequenceImporter, IMusicSequenceRepository
{
    /// <summary>
    /// The Fmotifs repository.
    /// </summary>
    protected readonly FmotifRepository FmotifRepository;

    /// <summary>
    /// The measures repository.
    /// </summary>
    protected readonly MeasureRepository MeasureRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicSequenceRepository"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public MusicSequenceRepository(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, Cache cache) : base(dbFactory, cache)
    {
        FmotifRepository = new FmotifRepository(Db);
        MeasureRepository = new MeasureRepository(Db);
    }

    /// <summary>
    /// The create.
    /// </summary>
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    /// <param name="sequenceStream">
    /// The sequence stream.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown if congeneric tracks count not equals 1 (track is not monophonic).
    /// </exception>
    public void Create(CommonSequence sequence, Stream sequenceStream)
    {
        string stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);
        XmlDocument doc = new();
        doc.LoadXml(stringSequence);

        MusicXmlParser parser = new();
        parser.Execute(doc);
        ScoreTrack tempTrack = parser.ScoreModel;

        if (tempTrack.CongenericScoreTracks.Count != 1)
        {
            throw new Exception("Track contains more then one or zero congeneric score tracks (parts).");
        }

        MatterRepository.CreateOrExtractExistingMatterForSequence(sequence);

        BaseChain notesSequence = ConvertCongenericScoreTrackToNotesBaseChain(tempTrack.CongenericScoreTracks[0]);
        long[] notesAlphabet = ElementRepository.GetOrCreateNotesInDb(notesSequence.Alphabet);

        BaseChain measuresSequence = ConvertCongenericScoreTrackToMeasuresBaseChain(tempTrack.CongenericScoreTracks[0]);
        long[] measuresAlphabet = MeasureRepository.GetOrCreateMeasuresInDb(measuresSequence.Alphabet);

        var pauseTreatments = EnumExtensions.ToArray<PauseTreatment>().Where(pt => pt != PauseTreatment.NotApplicable).ToArray();
        List<BaseChain> fmotifsSequences = new(pauseTreatments.Length);
        List<long[]> fmotifsAlphabets = new(pauseTreatments.Length);
        List<BaseChain> fmotifsSequencesWithSequentialTransfer = new(pauseTreatments.Length);
        List<long[]> fmotifsAlphabetsWithSequentialTransfer = new(pauseTreatments.Length);

        for (int i = 0; i < pauseTreatments.Length; i++)
        {
            PauseTreatment pauseTreatment = pauseTreatments[i];

            fmotifsSequences.Add(ConvertCongenericScoreTrackToFormalMotifsBaseChain(tempTrack.CongenericScoreTracks[0], pauseTreatment, false));
            fmotifsAlphabets[i] = FmotifRepository.GetOrCreateFmotifsInDb(fmotifsSequences[i].Alphabet);

            fmotifsSequencesWithSequentialTransfer.Add(ConvertCongenericScoreTrackToFormalMotifsBaseChain(tempTrack.CongenericScoreTracks[0], pauseTreatment, true));
            fmotifsAlphabetsWithSequentialTransfer.Add(FmotifRepository.GetOrCreateFmotifsInDb(fmotifsSequencesWithSequentialTransfer[i].Alphabet));
        }


        MusicSequence musicSequence = new()
        {
            MatterId = sequence.MatterId,
            RemoteDb = sequence.RemoteDb,
            RemoteId = sequence.RemoteId,
            Description = sequence.Description,
            Order = notesSequence.Order,
            Alphabet = notesAlphabet,
            Notation = Notation.Notes,
            PauseTreatment = PauseTreatment.NotApplicable,
            SequentialTransfer = false
        };
        Db.MusicSequences.Add(musicSequence);


        musicSequence = new MusicSequence
        {
            MatterId = sequence.MatterId,
            RemoteDb = sequence.RemoteDb,
            RemoteId = sequence.RemoteId,
            Description = sequence.Description,
            Order = measuresSequence.Order,
            Alphabet = measuresAlphabet,
            Notation = Notation.Measures,
            PauseTreatment = PauseTreatment.NotApplicable,
            SequentialTransfer = false
        };
        Db.MusicSequences.Add(musicSequence);

        for (int i = 0; i < pauseTreatments.Length; i++)
        {
            PauseTreatment pauseTreatment = pauseTreatments[i];

            musicSequence = new MusicSequence
            {
                MatterId = sequence.MatterId,
                Description = sequence.Description,
                RemoteDb = sequence.RemoteDb,
                RemoteId = sequence.RemoteId,
                Order = fmotifsSequences[i].Order,
                Alphabet = fmotifsAlphabets[i],
                Notation = Notation.FormalMotifs,
                PauseTreatment = pauseTreatment,
                SequentialTransfer = false
            };
            Db.MusicSequences.Add(musicSequence);

            musicSequence = new MusicSequence
            {
                MatterId = sequence.MatterId,
                Description = sequence.Description,
                RemoteDb = sequence.RemoteDb,
                RemoteId = sequence.RemoteId,
                Order = fmotifsSequencesWithSequentialTransfer[i].Order,
                Alphabet = fmotifsAlphabetsWithSequentialTransfer[i],
                Notation = Notation.FormalMotifs,
                PauseTreatment = pauseTreatment,
                SequentialTransfer = true
            };
            Db.MusicSequences.Add(musicSequence);
        }
    }

    /// <summary>
    /// The insert.
    /// </summary>
    /// <param name="commonSequence">
    /// The sequence.
    /// </param>
    /// <param name="alphabet">
    /// The alphabet.
    /// </param>
    /// <param name="order">
    /// The order.
    /// </param>
    public void Create(CommonSequence commonSequence, PauseTreatment pauseTreatment = PauseTreatment.NotApplicable, bool sequentialTransfer = false)
    {
        MusicSequence musicSequence = new()
        {
            MatterId = commonSequence.MatterId,
            Order = commonSequence.Order,
            Alphabet = commonSequence.Alphabet,
            Description = commonSequence.Description,
            Notation = commonSequence.Notation,
            RemoteDb = commonSequence.RemoteDb,
            RemoteId = commonSequence.RemoteId,
            PauseTreatment = pauseTreatment,
            SequentialTransfer = sequentialTransfer
        };

        Db.MusicSequences.Add(musicSequence);
        Db.SaveChanges();
    }

    /// <summary>
    /// Converts congeneric score track to base chain with notes as elements.
    /// </summary>
    /// <param name="scoreTrack">
    /// The score track.
    /// </param>
    /// <returns>
    /// The <see cref="BaseChain"/>.
    /// </returns>
    private BaseChain ConvertCongenericScoreTrackToNotesBaseChain(CongenericScoreTrack scoreTrack)
    {
        List<ValueNote> notes = scoreTrack.GetNotes();
        return new BaseChain(((IEnumerable<IBaseObject>)notes).ToList());
    }

    /// <summary>
    /// Convert congeneric score track to measures base chain.
    /// </summary>
    /// <param name="scoreTrack">
    /// The score track.
    /// </param>
    /// <returns>
    /// The <see cref="BaseChain"/>.
    /// </returns>
    private BaseChain ConvertCongenericScoreTrackToMeasuresBaseChain(CongenericScoreTrack scoreTrack)
    {
        List<Measure> measures = scoreTrack.MeasureOrder();
        return new BaseChain(((IEnumerable<IBaseObject>)measures).ToList());
    }

    /// <summary>
    /// Converts congeneric score track to formal motifs base chain.
    /// </summary>
    /// <param name="scoreTrack">
    /// The score track.
    /// </param>
    /// <returns>
    /// The <see cref="BaseChain"/>.
    /// </returns>
    private BaseChain ConvertCongenericScoreTrackToFormalMotifsBaseChain(CongenericScoreTrack scoreTrack, PauseTreatment pauseTreatment, bool sequentialTransfer)
    {
        BorodaDivider borodaDivider = new();
        FmotifChain fmotifChain = borodaDivider.Divide(scoreTrack, pauseTreatment, sequentialTransfer);
        return new BaseChain(((IEnumerable<IBaseObject>)fmotifChain.FmotifsList).ToList());
    }
}
