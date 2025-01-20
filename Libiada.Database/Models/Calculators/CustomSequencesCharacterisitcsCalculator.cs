namespace Libiada.Database.Models.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Database.Models.Repositories.Catalogs;

public class CustomSequencesCharacterisitcsCalculator
{
    private readonly LinkedFullCalculator[] calculators;

    public CustomSequencesCharacterisitcsCalculator(IFullCharacteristicRepository characteristicTypeLinkRepository, short[] characteristicLinkIds)
    {
        calculators = new LinkedFullCalculator[characteristicLinkIds.Length];
        for (int i = 0; i < characteristicLinkIds.Length; i++)
        {
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkIds[i]);
            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkIds[i]);
            calculators[i] = new LinkedFullCalculator(characteristic, link);
        }
    }

    public IEnumerable<double[]> Calculate(IEnumerable<ComposedSequence> sequences)
    {
        List<double[]> result = [];
        foreach(ComposedSequence sequence in sequences)
        {
            result.Add(Calculate(sequence));
        }

        return result;
    }

    public double[] Calculate(ComposedSequence sequence)
    {
        double[] characteristics = new double[calculators.Length];

        for(int i = 0; i < calculators.Length; i++)
        {
            characteristics[i] = calculators[i].Calculate(sequence);
        }

        return characteristics;
    }
}
