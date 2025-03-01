﻿namespace Libiada.Database.Models.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Extensions;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The subsequences characteristics calculator.
/// </summary>
public class SubsequencesCharacteristicsCalculator : ISubsequencesCharacteristicsCalculator
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;

    public SubsequencesCharacteristicsCalculator(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                                 IFullCharacteristicRepository characteristicTypeLinkRepository, 
                                                 ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory)
    {
        this.dbFactory = dbFactory;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
    }

    /// <summary>
    /// Calculates subsequences characteristics.
    /// </summary>
    /// <param name="parentId">
    /// The parent sequence id.
    /// </param>
    /// <param name="characteristicId">
    /// The id of characteristic type, arrangement type and link as <see cref="FullCharacteristicLink"/>.
    /// </param>
    /// <param name="features">
    /// The  features ids of subsequences to extract.
    /// </param>
    /// <param name="filters">
    /// Textual search filters for subsequences products.
    /// Null by default.
    /// </param>
    /// <returns></returns>
    public double[] CalculateSubsequencesCharacteristics(long parentId, short characteristicId, Feature[] features, string[] filters)
    {
        short[] characteristicsIds = [characteristicId];
        SubsequenceData[] subsequencesData = CalculateSubsequencesCharacteristics(characteristicsIds, features, parentId, filters);
        return subsequencesData.Select(s => s.CharacteristicsValues[0]).ToArray();
    }

    /// <summary>
    /// Calculates subsequences characteristics.
    /// </summary>
    /// <param name="characteristicIds">
    /// The ids of characteristic types, arrangement types and links as <see cref="FullCharacteristicLink"/>.
    /// </param>
    /// <param name="features">
    /// The  features ids of subsequences to extract.
    /// </param>
    /// <param name="parentSequenceId">
    /// The parent sequence id.
    /// </param>
    /// <param name="filters">
    /// Textual search filters for subsequences products.
    /// </param>
    /// <returns>
    /// The <see cref="T:SubsequenceData[]"/> .
    /// </returns>
    public SubsequenceData[] CalculateSubsequencesCharacteristics(
        short[] characteristicIds,
        Feature[] features,
        long parentSequenceId,
        string[]? filters = null)
    {
        Dictionary<long, ComposedSequence> sequences;
        long[] subsequenceIds;
        SubsequenceData[] subsequenceData;
        Dictionary<long, Dictionary<short, double>> characteristics;
        IFullCalculator[] calculators = new IFullCalculator[characteristicIds.Length];
        Link[] links = new Link[characteristicIds.Length];
        List<CharacteristicValue> newCharacteristics = [];

        // creating local context to avoid memory overflow due to possibly big cache of characteristics
        using var db = dbFactory.CreateDbContext();
        using var sequenceRepository = sequenceRepositoryFactory.Create();
        var subsequenceExtractor = new SubsequenceExtractor(db, sequenceRepository);

        Subsequence[] subsequences = filters.IsNullOrEmpty() ?
            subsequenceExtractor.GetSubsequences(parentSequenceId, features) :
            subsequenceExtractor.GetSubsequences(parentSequenceId, features, filters);

        subsequenceData = subsequences.Select(s => new SubsequenceData(s)).ToArray();

        // converting to libiada sequences
        subsequenceIds = subsequences.Select(s => s.Id).ToArray();
        characteristics = db.CharacteristicValues
                            .Where(c => characteristicIds.Contains(c.CharacteristicLinkId) && subsequenceIds.Contains(c.SequenceId))
                            .ToArray()
                            .GroupBy(c => c.SequenceId)
                            .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicLinkId, ct => ct.Value));
        if (characteristics.Count == subsequences.Length && characteristics.All(c => c.Value.Count == characteristicIds.Length))
        {
            sequences = [];
        }
        else
        {
            sequences = subsequenceExtractor.GetSubsequencesSequences(subsequences);
        }

        for (int k = 0; k < characteristicIds.Length; k++)
        {
            short characteristicLinkId = characteristicIds[k];
            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            calculators[k] = FullCalculatorsFactory.CreateCalculator(characteristic);
            links[k] = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
        }

        // cycle through subsequences
        for (int i = 0; i < subsequenceIds.Length; i++)
        {
            characteristics.TryGetValue(subsequenceIds[i], out Dictionary<short, double> sequenceDbCharacteristics);
            sequenceDbCharacteristics ??= [];
            double[] values = new double[calculators.Length];

            // cycle through characteristics and notations
            for (int j = 0; j < calculators.Length; j++)
            {
                short characteristicLinkId = characteristicIds[j];
                if (!sequenceDbCharacteristics.TryGetValue(characteristicLinkId, out values[j]))
                {
                    values[j] = calculators[j].Calculate(sequences[subsequenceIds[i]], links[j]);
                    CharacteristicValue currentCharacteristic = new()
                    {
                        SequenceId = subsequenceIds[i],
                        CharacteristicLinkId = characteristicLinkId,
                        Value = values[j]
                    };

                    newCharacteristics.Add(currentCharacteristic);
                }
            }

            subsequenceData[i].CharacteristicsValues = values;
        }

        // trying to save calculated characteristics to database
        CharacteristicRepository characteristicRepository = new(db);
        characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

        return subsequenceData;
    }
}
