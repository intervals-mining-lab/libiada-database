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
    private readonly ICommonSequenceRepository commonSequenceRepository;

    public LocalCharacteristicsCalculator(
        LibiadaDatabaseEntities db,
        IFullCharacteristicRepository characteristicTypeLinkRepository,
        ICommonSequenceRepository commonSequenceRepository)
    {
        this.db = db;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.commonSequenceRepository = commonSequenceRepository;
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
        Chain chain;
        IFullCalculator calculator;
        Link link;

        FullCharacteristic characteristic =
            characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
        calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
        link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

        SubsequenceExtractor subsequenceExtractor = new(db, commonSequenceRepository);

        Subsequence subsequence = db.Subsequences.Single(s => s.Id == subsequenceId);
        chain = subsequenceExtractor.GetSubsequenceSequence(subsequence);


        CutRule cutRule = new SimpleCutRule(chain.Length, step, windowSize);

        CutRuleIterator iterator = cutRule.GetIterator();

        List<Chain> fragments = [];

        while (iterator.Next())
        {
            int start = iterator.GetStartPosition();
            int end = iterator.GetEndPosition();

            List<IBaseObject> fragment = [];
            for (int k = 0; start + k < end; k++)
            {
                fragment.Add(chain[start + k]);
            }

            fragments.Add(new Chain(fragment));
        }

        double[] characteristics = new double[fragments.Count];

        for (int k = 0; k < fragments.Count; k++)
        {
            characteristics[k] = calculator.Calculate(fragments[k], link);
        }

        return characteristics;
    }
}
