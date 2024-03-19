namespace Libiada.Database.Models.Calculators;

using Bio;
using Bio.Extensions;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Core.Core.SimpleTypes;
using Libiada.Core.Extensions;

using Libiada.Database.Models.Repositories.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The sequences characteristics calculator.
/// </summary>
public class SequencesCharacteristicsCalculator : ISequencesCharacteristicsCalculator
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;

    public SequencesCharacteristicsCalculator(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                              ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory,
                                              IFullCharacteristicRepository characteristicTypeLinkRepository)
    {
        this.dbFactory = dbFactory;
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
    }

    /// <summary>
    /// Calculation method.
    /// </summary>
    /// <param name="chainIds">
    /// The chains ids.
    /// </param>
    /// <param name="characteristicLinkIds">
    /// The characteristicLink ids.
    /// </param>
    /// <returns>
    /// The <see cref="T:double[][]"/>.
    /// </returns>
    public double[][] Calculate(long[][] chainIds, short[] characteristicLinkIds)
    {
        Dictionary<long, short[]> chainCharacteristicsIds = ToSequenceCharacteristicsIdsDictionary(chainIds, characteristicLinkIds);
        Dictionary<long, Dictionary<short, double>> result = Calculate(chainCharacteristicsIds);
        return ExtractCharacteristicsValues(result, chainIds, characteristicLinkIds);
    }

    /// <summary>
    /// Calculation method.
    /// </summary>
    /// <param name="chainCharacteristicsIds">
    /// Dictionary with chain ids as a key
    /// and characteristicLink ids array as value.
    /// </param>
    /// <returns>
    /// The <see cref="T:double[][]"/>.
    /// </returns>
    public Dictionary<long, Dictionary<short, double>> Calculate(Dictionary<long, short[]> chainCharacteristicsIds)
    {
        List<CharacteristicValue> newCharacteristics = [];
        Dictionary<long, Dictionary<short, double>> allCharacteristics = [];

        var characteristicLinkIds = chainCharacteristicsIds.SelectMany(c => c.Value).Distinct();
        var calculators = new Dictionary<short, LinkedFullCalculator>();
        foreach (short characteristicLinkId in characteristicLinkIds)
        {
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            calculators.Add(characteristicLinkId, new LinkedFullCalculator(characteristic, link));
        }
        using var db = dbFactory.CreateDbContext();
        var sequenceIds = chainCharacteristicsIds.Keys;
        using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
        foreach (long sequenceId in sequenceIds)
        {
            short[] sequenceCharacteristicLinkIds = chainCharacteristicsIds[sequenceId];
            Dictionary<short, double> characteristics = db.CharacteristicValues
                                                          .Where(c => sequenceId == c.SequenceId && sequenceCharacteristicLinkIds.Contains(c.CharacteristicLinkId))
                                                          .ToDictionary(ct => ct.CharacteristicLinkId, ct => ct.Value);

            allCharacteristics.Add(sequenceId, characteristics);

            if (characteristics.Count < sequenceCharacteristicLinkIds.Length)
            {
                Chain sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);

                foreach (short sequenceCharacteristicLinkId in sequenceCharacteristicLinkIds)
                {
                    if (!characteristics.ContainsKey(sequenceCharacteristicLinkId))
                    {
                        LinkedFullCalculator calculator = calculators[sequenceCharacteristicLinkId];
                        double characteristicValue = calculator.Calculate(sequence);
                        CharacteristicValue characteristic = new()
                        {
                            SequenceId = sequenceId,
                            CharacteristicLinkId = sequenceCharacteristicLinkId,
                            Value = characteristicValue
                        };

                        characteristics.Add(sequenceCharacteristicLinkId, characteristicValue);
                        newCharacteristics.Add(characteristic);
                    }
                }
            }
        }

        CharacteristicRepository characteristicRepository = new(db);
        characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

        return allCharacteristics;
    }

    /// <summary>
    /// Calculation method.
    /// </summary>
    /// <param name="chainIds">
    /// The chains ids.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristicLink id.
    /// </param>
    /// <returns>
    /// The <see cref="T:double[]"/>.
    /// </returns>
    public double[] Calculate(long[] chainIds, short characteristicLinkId)
    {
        Dictionary<long, short[]> chainCharacteristicsIds = chainIds.ToDictionary(c => c, c => new[] { characteristicLinkId });

        Dictionary<long, Dictionary<short, double>> dictionaryResult = Calculate(chainCharacteristicsIds);

        double[] result = new double[chainIds.Length];
        for (int i = 0; i < dictionaryResult.Count; i++)
        {
            result[i] = dictionaryResult[chainIds[i]][characteristicLinkId];
        }

        return result;
    }

    /// <summary>
    /// Calculation method.
    /// </summary>
    /// <param name="chainIds">
    /// The chains ids.
    /// </param>
    /// <param name="characteristicLinkIds">
    /// The characteristic type link ids.
    /// </param>
    /// <param name="rotate">
    /// The rotate flag.
    /// </param>
    /// <param name="complementary">
    /// The complementary flag.
    /// </param>
    /// <param name="rotationLength">
    /// The rotation length.
    /// </param>
    /// <returns>
    /// The <see cref="T:double[][]"/>.
    /// </returns>
    public double[][] Calculate(long[][] chainIds, short[] characteristicLinkIds, bool rotate, bool complementary, uint? rotationLength)
    {
        Link[] links = new Link[characteristicLinkIds.Length];
        IFullCalculator[] calculators = new IFullCalculator[characteristicLinkIds.Length];
        double[][] characteristics = new double[chainIds.Length][];
        IEnumerable<long> sequenceIds = chainIds.SelectMany(c => c).Distinct();
        Dictionary<long, Chain> sequences = [];
        using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
        foreach (long sequenceId in sequenceIds)
        {
            sequences.Add(sequenceId, commonSequenceRepository.GetLibiadaChain(sequenceId));
        }

        for (int k = 0; k < characteristicLinkIds.Length; k++)
        {
            links[k] = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkIds[k]);
            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkIds[k]);
            calculators[k] = FullCalculatorsFactory.CreateCalculator(characteristic);
        }

        for (int i = 0; i < chainIds.Length; i++)
        {
            characteristics[i] = new double[calculators.Length];

            for (int j = 0; j < calculators.Length; j++)
            {
                long sequenceId = chainIds[i][j];
                Chain sequence = (Chain)sequences[sequenceId].Clone();
                if (complementary)
                {
                    Sequence sourceSequence = new(Alphabets.DNA, sequence.ToString());
                    ISequence complementarySequence = sourceSequence.GetReverseComplementedSequence();
                    sequence = new Chain(complementarySequence.ConvertToString());
                }

                if (rotate)
                {
                    int[] order = sequence.Order.Rotate(rotationLength ?? 0);
                    List<IBaseObject> newSequence = order.Select(t => new ValueInt(t)).ToList<IBaseObject>();
                    sequence = new Chain(newSequence);
                }

                characteristics[i][j] = calculators[j].Calculate(sequence, links[j]);
            }
        }

        return characteristics;
    }

    /// <summary>
    /// Converts data into dictionary with sequences ids as keys
    /// and characteristicLinks ids as values.
    /// </summary>
    /// <param name="sequenceIds">
    /// The sequence ids.
    /// </param>
    /// <param name="characteristicLinkIds">
    /// The characteristic link ids.
    /// </param>
    /// <returns>
    /// The <see cref="T:Dictionary{long,short[]}"/>.
    /// </returns>
    private Dictionary<long, short[]> ToSequenceCharacteristicsIdsDictionary(long[][] sequenceIds, short[] characteristicLinkIds)
    {
        Dictionary<long, List<short>> result = sequenceIds.SelectMany(s => s)
                                                          .Distinct()
                                                          .ToDictionary(s => s, s => new List<short>());
        foreach (long[] sequenceId in sequenceIds)
        {
            for (int j = 0; j < sequenceId.Length; j++)
            {
                result[sequenceId[j]].Add(characteristicLinkIds[j]);
            }
        }

        return result.ToDictionary(c => c.Key, c => c.Value.ToArray());
    }

    /// <summary>
    /// Extracts two-dimensional array of characteristics values.
    /// </summary>
    /// <param name="characteristics">
    /// The characteristics.
    /// </param>
    /// <param name="sequenceIds">
    /// The sequence ids.
    /// </param>
    /// <param name="characteristicIds">
    /// The characteristic ids.
    /// </param>
    /// <returns>
    /// The <see cref="T:double[][]"/>.
    /// </returns>
    private double[][] ExtractCharacteristicsValues(Dictionary<long, Dictionary<short, double>> characteristics, long[][] sequenceIds, short[] characteristicIds)
    {
        double[][] result = new double[sequenceIds.Length][];
        for (int i = 0; i < sequenceIds.Length; i++)
        {
            result[i] = new double[sequenceIds[i].Length];
            for (int j = 0; j < characteristicIds.Length; j++)
            {
                result[i][j] = characteristics[sequenceIds[i][j]][characteristicIds[j]];
            }
        }

        return result;
    }
}
