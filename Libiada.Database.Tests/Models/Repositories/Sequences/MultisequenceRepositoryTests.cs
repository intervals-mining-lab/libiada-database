﻿namespace Libiada.Database.Tests.Models.Repositories.Sequences;

using Libiada.Database.Models.Repositories.Sequences;

[TestFixture]
public class MultisequenceRepositoryTests
{
    private static readonly string[][] testData =
    [
        [
            "Orchid fleck dichorhavirus | Orchid fleck virus genomic RNA, segment RNA 1 | NC_009608.1",
            "Yellowtail ascites virus segment A | NC_004168.1",
            "Influenza A virus (A/New York/392/2004(H3N2)) segment 6 | CY002066.1"
        ],
        [
            "Burkholderia mallei ATCC 23344 chromosome 1 | CP000010.1",
            "Brucella abortus biovar 1 str. 9-941 chromosome II | AE017224.1",
            "Lactobacillus acidophilus NCFM chromosome | NC_006814.3"
        ],
        [
            "Prosthecochloris aestuarii DSM 271 plasmid pPAES01 | CP001109.1",
            "Lactobacillus paracasei subsp. paracasei 8700:2 plasmid 1 | CP002392.1",
            "Nitrosococcus oceani ATCC 19707 plasmid A | CP000126.1"
        ]
    ];


    [TestCase(0, new[] { 1, 1, 6 })]
    [TestCase(1, new[] { 1, 2, 0 })]
    [TestCase(2, new[] { 1, 1, 1 })]
    public void GetSequenceNumberTest(int testDataIndex, int[] expectedResults)
    {
        string[] names = testData[testDataIndex];
        for (int i = 0; i < names.Length; i++)
        {
            int result = MultisequenceRepository.GetSequenceNumberByName(MultisequenceRepository.GetResearchObjectNameSplit(names[i]));
            Assert.That(expectedResults[i], Is.EqualTo(result));
        }
    }
}
