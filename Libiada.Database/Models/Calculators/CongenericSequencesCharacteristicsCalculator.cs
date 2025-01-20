namespace Libiada.Database.Models.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.CongenericCalculators;

using Libiada.Database.Models.Repositories.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;

using Microsoft.EntityFrameworkCore;

public class CongenericSequencesCharacteristicsCalculator : ICongenericSequencesCharacteristicsCalculator
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly ICongenericCharacteristicRepository characteristicTypeLinkRepository;

    public CongenericSequencesCharacteristicsCalculator(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                              ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                              ICongenericCharacteristicRepository characteristicTypeLinkRepository)
    {
        this.dbFactory = dbFactory;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
    }

    public Dictionary<long, Dictionary<(short, long), double>> Calculate(long[][] sequenceIds, short[] characteristicLinkIds)
    {
        Dictionary<long, short[]> sequenceCharacteristicsIds = ToSequenceCharacteristicsIdsDictionary(sequenceIds, characteristicLinkIds);
        return Calculate(sequenceCharacteristicsIds);
    }

    /// <summary>
    /// Calculation method.
    /// </summary>
    /// <param name="sequenceCharacteristicsIds">
    /// Dictionary with sequence ids as a key
    /// and characteristicLink ids array as value.
    /// </param>
    /// <returns>
    /// The <see cref="T:double[][]"/>.
    /// </returns>
    public Dictionary<long, Dictionary<(short, long), double>> Calculate(Dictionary<long, short[]> sequenceCharacteristicsIds)
    {
        List<CongenericCharacteristicValue> newCharacteristics = [];
        Dictionary<long, Dictionary<(short, long), double>> allCharacteristics = [];

        var characteristicLinkIds = sequenceCharacteristicsIds.SelectMany(c => c.Value).Distinct();
        var calculators = new Dictionary<short, LinkedCongenericCalculator>();
        foreach (short characteristicLinkId in characteristicLinkIds)
        {
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
            CongenericCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            calculators.Add(characteristicLinkId, new LinkedCongenericCalculator(characteristic, link));
        }

        long[] sequenceIds = sequenceCharacteristicsIds.Keys.ToArray();
        using var db = dbFactory.CreateDbContext();
        var dbAlphabets = db.CombinedSequenceEntities.Where(cs => sequenceIds.Contains(cs.Id)).ToDictionary(cs => cs.Id, cs => cs.Alphabet);
        using var sequenceRepository = sequenceRepositoryFactory.Create();
        foreach (long sequenceId in sequenceIds)
        {
            long[] dbAlphabet = dbAlphabets[sequenceId];
            short[] sequenceCharacteristicLinkIds = sequenceCharacteristicsIds[sequenceId];
            Dictionary<(short, long), double> characteristics = db.CongenericCharacteristicValues
                                                          .Where(c => sequenceId == c.SequenceId && sequenceCharacteristicLinkIds.Contains(c.CharacteristicLinkId))
                                                          .ToDictionary(ct => (ct.CharacteristicLinkId, ct.ElementId), ct => ct.Value);

            allCharacteristics.Add(sequenceId, characteristics);

            if (characteristics.Count < sequenceCharacteristicLinkIds.Length * dbAlphabet.Length)
            {
                ComposedSequence sequence = sequenceRepository.GetLibiadaComposedSequence(sequenceId);
                // TODO: add ids to IBaseObject to avoid duplicate enumeration
                Alphabet alphabet = sequence.Alphabet;

                foreach (short sequenceCharacteristicLinkId in sequenceCharacteristicLinkIds)
                {
                    LinkedCongenericCalculator calculator = calculators[sequenceCharacteristicLinkId];

                    for (int i = 0; i < alphabet.Cardinality; i++)
                    {
                        long elementId = dbAlphabet[i];
                        if (!characteristics.ContainsKey((sequenceCharacteristicLinkId, elementId)))
                        {

                            double characteristicValue = calculator.Calculate(sequence.CongenericSequence(i));
                            CongenericCharacteristicValue characteristic = new()
                            {
                                SequenceId = sequenceId,
                                CharacteristicLinkId = sequenceCharacteristicLinkId,
                                ElementId = elementId,
                                Value = characteristicValue
                            };

                            characteristics.Add((sequenceCharacteristicLinkId, elementId), characteristicValue);
                            newCharacteristics.Add(characteristic);
                        }

                    }
                }
            }
        }

        CharacteristicRepository characteristicRepository = new(db);
        characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

        return allCharacteristics;
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
    private static Dictionary<long, short[]> ToSequenceCharacteristicsIdsDictionary(long[][] sequenceIds, short[] characteristicLinkIds)
    {
        Dictionary<long, List<short>> result = sequenceIds.SelectMany(s => s)
                                                          .Distinct()
                                                          .ToDictionary(s => s, s => new List<short>());
        for (int i = 0; i < sequenceIds.Length; i++)
        {
            for (int j = 0; j < sequenceIds[i].Length; j++)
            {
                result[sequenceIds[i][j]].Add(characteristicLinkIds[j]);
            }
        }

        return result.ToDictionary(c => c.Key, c => c.Value.ToArray());
    }
}
