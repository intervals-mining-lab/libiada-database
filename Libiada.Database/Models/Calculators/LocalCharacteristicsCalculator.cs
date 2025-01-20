namespace Libiada.Database.Models.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Core.Iterators;

using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;

public class LocalCharacteristicsCalculator
{
    private readonly LibiadaDatabaseEntities db;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ICombinedSequenceEntityRepository sequenceRepository;

    public LocalCharacteristicsCalculator(
        LibiadaDatabaseEntities db,
        IFullCharacteristicRepository characteristicTypeLinkRepository,
        ICombinedSequenceEntityRepository sequenceRepository)
    {
        this.db = db;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.sequenceRepository = sequenceRepository;
    }

    /// <summary>
    /// The get subsequence characteristic.
    /// </summary>
    /// <param name="subsequenceId">
    /// The subsequence id.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
    /// </param>
    /// <param name="windowSize">
    /// The window size.
    /// </param>
    /// <param name="step">
    /// The step.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public double[] GetSubsequenceCharacteristic(
        long subsequenceId,
        short characteristicLinkId,
        int windowSize,
        int step)
    {
        FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
        IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
        Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

        SubsequenceExtractor subsequenceExtractor = new(db, sequenceRepository);

        Subsequence subsequence = db.Subsequences.Single(s => s.Id == subsequenceId);
        ComposedSequence sequence = subsequenceExtractor.GetSubsequenceSequence(subsequence);


        CutRule cutRule = new SimpleCutRule(sequence.Length, step, windowSize);

        CutRuleIterator iterator = cutRule.GetIterator();

        List<ComposedSequence> fragments = [];

        while (iterator.Next())
        {
            int start = iterator.GetStartPosition();
            int end = iterator.GetEndPosition();

            List<IBaseObject> fragment = [];
            for (int k = 0; start + k < end; k++)
            {
                fragment.Add(sequence[start + k]);
            }

            fragments.Add(new ComposedSequence(fragment));
        }

        double[] characteristics = new double[fragments.Count];

        for (int k = 0; k < fragments.Count; k++)
        {
            characteristics[k] = calculator.Calculate(fragments[k], link);
        }

        return characteristics;
    }
}
