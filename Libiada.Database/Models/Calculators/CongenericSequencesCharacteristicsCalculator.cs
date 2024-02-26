﻿namespace Libiada.Database.Models.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.CongenericCalculators;

using Libiada.Database.Models.Repositories.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;

public class CongenericSequencesCharacteristicsCalculator : ICongenericSequencesCharacteristicsCalculator
{
    private readonly ILibiadaDatabaseEntitiesFactory dbFactory;
    private readonly ICommonSequenceRepository commonSequenceRepository;
    private readonly ICongenericCharacteristicRepository characteristicTypeLinkRepository;

    public CongenericSequencesCharacteristicsCalculator(ILibiadaDatabaseEntitiesFactory dbFactory,
                                              ICommonSequenceRepository commonSequenceRepository,
                                              ICongenericCharacteristicRepository characteristicTypeLinkRepository)
    {
        this.dbFactory = dbFactory;
        this.commonSequenceRepository = commonSequenceRepository;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
    }

    public Dictionary<long, Dictionary<(short, long), double>> Calculate(long[][] sequenceIds, short[] characteristicLinkIds)
    {
        Dictionary<long, short[]> chainCharacteristicsIds = ToSequenceCharacteristicsIdsDictionary(sequenceIds, characteristicLinkIds);
        return Calculate(chainCharacteristicsIds);
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
    public Dictionary<long, Dictionary<(short, long), double>> Calculate(Dictionary<long, short[]> chainCharacteristicsIds)
    {
        var newCharacteristics = new List<CongenericCharacteristicValue>();
        var allCharacteristics = new Dictionary<long, Dictionary<(short, long), double>>();

        var characteristicLinkIds = chainCharacteristicsIds.SelectMany(c => c.Value).Distinct();
        var calculators = new Dictionary<short, LinkedCongenericCalculator>();
        foreach (short characteristicLinkId in characteristicLinkIds)
        {
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
            CongenericCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            calculators.Add(characteristicLinkId, new LinkedCongenericCalculator(characteristic, link));
        }

        long[] sequenceIds = chainCharacteristicsIds.Keys.ToArray();
        using var db = dbFactory.CreateDbContext();
        var dbAlphabets = db.CommonSequences.Where(cs => sequenceIds.Contains(cs.Id)).ToDictionary(cs => cs.Id, cs => cs.Alphabet);

        foreach (long sequenceId in sequenceIds)
        {
            var dbAlphabet = dbAlphabets[sequenceId];
            short[] sequenceCharacteristicLinkIds = chainCharacteristicsIds[sequenceId];
            Dictionary<(short, long), double> characteristics = db.CongenericCharacteristicValues
                                                          .Where(c => sequenceId == c.SequenceId && sequenceCharacteristicLinkIds.Contains(c.CharacteristicLinkId))
                                                          .ToDictionary(ct => (ct.CharacteristicLinkId, ct.ElementId), ct => ct.Value);

            allCharacteristics.Add(sequenceId, characteristics);

            if (characteristics.Count < sequenceCharacteristicLinkIds.Length * dbAlphabet.Count)
            {
                Chain sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);
                // TODO: add ids to IBaseObject to avoid duplicate enumeration
                var alphabet = sequence.Alphabet;

                foreach (short sequenceCharacteristicLinkId in sequenceCharacteristicLinkIds)
                {
                    LinkedCongenericCalculator calculator = calculators[sequenceCharacteristicLinkId];

                    for (int i = 0; i < alphabet.Cardinality; i++)
                    {
                        var elementId = dbAlphabet[i];
                        if (!characteristics.ContainsKey((sequenceCharacteristicLinkId, elementId)))
                        {

                            double characteristicValue = calculator.Calculate(sequence.CongenericChain(i));
                            var characteristic = new CongenericCharacteristicValue
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

        var characteristicRepository = new CharacteristicRepository(db);
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
