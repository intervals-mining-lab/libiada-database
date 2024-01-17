namespace Libiada.Database.Models.Repositories.Sequences;

using System.Xml;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;
using Libiada.Core.Music;
using Libiada.Core.Music.MusicXml;

using Libiada.Database.Extensions;
using Libiada.Database.Helpers;

using Npgsql;
using NpgsqlTypes;

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
    /// <param name="db">
    /// The db.
    /// </param>
    public MusicSequenceRepository(ILibiadaDatabaseEntitiesFactory dbFactory, Cache cache) : base(dbFactory, cache)
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
        var doc = new XmlDocument();
        doc.LoadXml(stringSequence);

        var parser = new MusicXmlParser();
        parser.Execute(doc);
        ScoreTrack tempTrack = parser.ScoreModel;

        if (tempTrack.CongenericScoreTracks.Count != 1)
        {
            throw new Exception("Track contains more then one or zero congeneric score tracks (parts).");
        }

        MatterRepository.CreateOrExtractExistingMatterForSequence(sequence);

        BaseChain notesSequence = ConvertCongenericScoreTrackToNotesBaseChain(tempTrack.CongenericScoreTracks[0]);
        long[] notesAlphabet = ElementRepository.GetOrCreateNotesInDb(notesSequence.Alphabet);
        sequence.Notation = Notation.Notes;
        Create(sequence, notesAlphabet, notesSequence.Order);

        BaseChain measuresSequence = ConvertCongenericScoreTrackToMeasuresBaseChain(tempTrack.CongenericScoreTracks[0]);
        long[] measuresAlphabet = MeasureRepository.GetOrCreateMeasuresInDb(measuresSequence.Alphabet);
        sequence.Notation = Notation.Measures;
        sequence.Id = default;
        Create(sequence, measuresAlphabet, measuresSequence.Order);

        sequence.Notation = Notation.FormalMotifs;
        var pauseTreatments = Libiada.Core.Extensions.EnumExtensions.ToArray<PauseTreatment>().Where(pt => pt != PauseTreatment.NotApplicable);
        foreach (PauseTreatment pauseTreatment in pauseTreatments)
        {
            BaseChain fmotifsSequence = ConvertCongenericScoreTrackToFormalMotifsBaseChain(tempTrack.CongenericScoreTracks[0], pauseTreatment, false);
            long[] fmotifsAlphabet = FmotifRepository.GetOrCreateFmotifsInDb(fmotifsSequence.Alphabet);
            sequence.Id = default;
            Create(sequence, fmotifsAlphabet, fmotifsSequence.Order, pauseTreatment, false);

            fmotifsSequence = ConvertCongenericScoreTrackToFormalMotifsBaseChain(tempTrack.CongenericScoreTracks[0], pauseTreatment, true);
            fmotifsAlphabet = FmotifRepository.GetOrCreateFmotifsInDb(fmotifsSequence.Alphabet);
            sequence.Id = default;
            Create(sequence, fmotifsAlphabet, fmotifsSequence.Order, pauseTreatment, true);
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
    public void Create(CommonSequence commonSequence, long[] alphabet, int[] order, PauseTreatment pauseTreatment = PauseTreatment.NotApplicable, bool sequentialTransfer = false)
    {
        List<NpgsqlParameter> parameters = FillParams(commonSequence, alphabet, order, pauseTreatment, sequentialTransfer);

        const string Query = @"INSERT INTO music_chain (
                                        id,
                                        notation,
                                        matter_id,
                                        alphabet,
                                        building,
                                        remote_id,
                                        remote_db,
                                        pause_treatment,
                                        sequential_transfer
                                    ) VALUES (
                                        @id,
                                        @notation,
                                        @matter_id,
                                        @alphabet,
                                        @building,
                                        @remote_id,
                                        @remote_db,
                                        @pause_treatment,
                                        @sequential_transfer
                                    );";
        Db.ExecuteCommand(Query, parameters.ToArray());
    }

    /// <summary>
    /// The fill parameters.
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
    /// <returns>
    /// The <see cref="List{Object}"/>.
    /// </returns>
    private List<NpgsqlParameter> FillParams(CommonSequence commonSequence, long[] alphabet, int[] order, PauseTreatment pauseTreatment, bool sequentialTransfer)
    {
        var parameters = FillParams(commonSequence, alphabet, order);

        parameters.Add(new NpgsqlParameter<byte>("pause_treatment", NpgsqlDbType.Smallint) {  TypedValue = (byte)pauseTreatment });
        parameters.Add(new NpgsqlParameter<bool>("sequential_transfer", NpgsqlDbType.Boolean) { TypedValue = sequentialTransfer });

        return parameters;
    }

    /// <summary>
    /// The dispose.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// The convert congeneric score track to base chain.
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
        var borodaDivider = new BorodaDivider();
        FmotifChain fmotifChain = borodaDivider.Divide(scoreTrack, pauseTreatment, sequentialTransfer);
        return new BaseChain(((IEnumerable<IBaseObject>)fmotifChain.FmotifsList).ToList());
    }
}
