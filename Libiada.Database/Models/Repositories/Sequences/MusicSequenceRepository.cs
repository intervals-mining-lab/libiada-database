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
    public void Create(MusicSequence sequence, Stream sequenceStream)
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

        CombinedSequenceEntity dbSequence = sequence.ToCombinedSequence();

        ResearchObjectRepository.CreateOrExtractExistingResearchObjectForSequence(dbSequence);

        Sequence notesSequence = ConvertCongenericScoreTrackToNotesSequence(tempTrack.CongenericScoreTracks[0]);
        long[] notesAlphabet = ElementRepository.GetOrCreateNotesInDb(notesSequence.Alphabet);

        Sequence measuresSequence = ConvertCongenericScoreTrackToMeasuresSequence(tempTrack.CongenericScoreTracks[0]);
        long[] measuresAlphabet = MeasureRepository.GetOrCreateMeasuresInDb(measuresSequence.Alphabet);

        var pauseTreatments = EnumExtensions.ToArray<PauseTreatment>().Where(pt => pt != PauseTreatment.NotApplicable).ToArray();
        List<Sequence> fmotifsSequences = new(pauseTreatments.Length);
        List<long[]> fmotifsAlphabets = new(pauseTreatments.Length);
        List<Sequence> fmotifsSequencesWithSequentialTransfer = new(pauseTreatments.Length);
        List<long[]> fmotifsAlphabetsWithSequentialTransfer = new(pauseTreatments.Length);

        for (int i = 0; i < pauseTreatments.Length; i++)
        {
            PauseTreatment pauseTreatment = pauseTreatments[i];

            fmotifsSequences.Add(ConvertCongenericScoreTrackToFormalMotifsSequence(tempTrack.CongenericScoreTracks[0], pauseTreatment, false));
            fmotifsAlphabets[i] = FmotifRepository.GetOrCreateFmotifsInDb(fmotifsSequences[i].Alphabet);

            fmotifsSequencesWithSequentialTransfer.Add(ConvertCongenericScoreTrackToFormalMotifsSequence(tempTrack.CongenericScoreTracks[0], pauseTreatment, true));
            fmotifsAlphabetsWithSequentialTransfer.Add(FmotifRepository.GetOrCreateFmotifsInDb(fmotifsSequencesWithSequentialTransfer[i].Alphabet));
        }

        // creating notes and measures sequences
        List<CombinedSequenceEntity> CombinedSequenceEntityToCreate = new(8);

        CombinedSequenceEntity musicSequence = sequence.ToCombinedSequence();
        musicSequence.Order = notesSequence.Order;
        musicSequence.Alphabet = notesAlphabet;
        musicSequence.Notation = Notation.Notes;
        musicSequence.PauseTreatment = PauseTreatment.NotApplicable;
        musicSequence.SequentialTransfer = false;

        CombinedSequenceEntityToCreate.Add(musicSequence);

        musicSequence = sequence.ToCombinedSequence();
        musicSequence.Order = measuresSequence.Order;
        musicSequence.Alphabet = measuresAlphabet;
        musicSequence.Notation = Notation.Measures;
        musicSequence.PauseTreatment = PauseTreatment.NotApplicable;
        musicSequence.SequentialTransfer = false;

        CombinedSequenceEntityToCreate.Add(musicSequence);

        // creating fmotif sequences
        for (int i = 0; i < pauseTreatments.Length; i++)
        {
            PauseTreatment pauseTreatment = pauseTreatments[i];

            musicSequence = sequence.ToCombinedSequence();
            musicSequence.Order = fmotifsSequences[i].Order;
            musicSequence.Alphabet = fmotifsAlphabets[i];
            musicSequence.Notation = Notation.FormalMotifs;
            musicSequence.PauseTreatment = pauseTreatment;
            musicSequence.SequentialTransfer = false;

            CombinedSequenceEntityToCreate.Add(musicSequence);

            musicSequence = sequence.ToCombinedSequence();
            musicSequence.Order = fmotifsSequencesWithSequentialTransfer[i].Order;
            musicSequence.Alphabet = fmotifsAlphabetsWithSequentialTransfer[i];
            musicSequence.Notation = Notation.FormalMotifs;
            musicSequence.PauseTreatment = pauseTreatment;
            musicSequence.SequentialTransfer = true;

            CombinedSequenceEntityToCreate.Add(musicSequence);
        }

        Db.SaveChanges();
    }

    /// <summary>
    /// The insert.
    /// </summary>
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    /// <param name="alphabet">
    /// The alphabet.
    /// </param>
    /// <param name="order">
    /// The order.
    /// </param>
    public void Create(MusicSequence sequence)
    {
        CombinedSequenceEntity dbSequence = sequence.ToCombinedSequence();

        Db.CombinedSequenceEntities.Add(dbSequence);
        Db.SaveChanges();

        sequence.Id = dbSequence.Id;
    }

    /// <summary>
    /// Converts congeneric score track to sequence with notes as elements.
    /// </summary>
    /// <param name="scoreTrack">
    /// The score track.
    /// </param>
    /// <returns>
    /// The <see cref="Sequence"/>.
    /// </returns>
    private Sequence ConvertCongenericScoreTrackToNotesSequence(CongenericScoreTrack scoreTrack)
    {
        List<ValueNote> notes = scoreTrack.GetNotes();
        return new Sequence(((IEnumerable<IBaseObject>)notes).ToList());
    }

    /// <summary>
    /// Convert congeneric score track to measures sequence.
    /// </summary>
    /// <param name="scoreTrack">
    /// The score track.
    /// </param>
    /// <returns>
    /// The <see cref="Sequence"/>.
    /// </returns>
    private Sequence ConvertCongenericScoreTrackToMeasuresSequence(CongenericScoreTrack scoreTrack)
    {
        List<Measure> measures = scoreTrack.MeasureOrder();
        return new Sequence(((IEnumerable<IBaseObject>)measures).ToList());
    }

    /// <summary>
    /// Converts congeneric score track to formal motifs sequence.
    /// </summary>
    /// <param name="scoreTrack">
    /// The score track.
    /// </param>
    /// <returns>
    /// The <see cref="Sequence"/>.
    /// </returns>
    private Sequence ConvertCongenericScoreTrackToFormalMotifsSequence(CongenericScoreTrack scoreTrack, PauseTreatment pauseTreatment, bool sequentialTransfer)
    {
        BorodaDivider borodaDivider = new();
        FmotifSequence fmotifSequence = borodaDivider.Divide(scoreTrack, pauseTreatment, sequentialTransfer);
        return new Sequence(((IEnumerable<IBaseObject>)fmotifSequence.FmotifsList).ToList());
    }
}
