﻿namespace Libiada.Database.Tests.Models;

using Libiada.Core.Extensions;

using Libiada.Database.Extensions;

using EnumExtensions = Core.Extensions.EnumExtensions;

/// <summary>
/// Sequence type enum tests.
/// </summary>
[TestFixture(TestOf = typeof(SequenceType))]
public class SequenceTypeTests
{
    /// <summary>
    /// The sequence types count.
    /// </summary>
    private const int SequenceTypesCount = 14;

    /// <summary>
    /// Array of all sequence types.
    /// </summary>
    private readonly SequenceType[] sequenceTypes = EnumExtensions.ToArray<SequenceType>();

    /// <summary>
    /// Array of all natures.
    /// </summary>
    private readonly Nature[] natures = EnumExtensions.ToArray<Nature>();

    /// <summary>
    /// Tests count of sequence types.
    /// </summary>
    [Test]
    public void SequenceTypeCountTest() => Assert.That(sequenceTypes, Has.Length.EqualTo(SequenceTypesCount));

    /// <summary>
    /// Tests values of sequence types.
    /// </summary>
    [Test]
    public void SequenceTypeValuesTest()
    {
        for (int i = 1; i <= SequenceTypesCount; i++)
        {
            Assert.That(sequenceTypes, Contains.Item((SequenceType)i));
        }
    }

    /// <summary>
    /// Tests names of sequence types.
    /// </summary>
    /// <param name="sequenceType">
    /// The sequence Type.
    /// </param>
    /// <param name="name">
    /// The name.
    /// </param>
    [TestCase((SequenceType)1, "CompleteGenome")]
    [TestCase((SequenceType)2, "CompleteMusicalComposition")]
    [TestCase((SequenceType)3, "CompleteText")]
    [TestCase((SequenceType)4, "CompleteNumericSequence")]
    [TestCase((SequenceType)5, "Plasmid")]
    [TestCase((SequenceType)6, "MitochondrialGenome")]
    [TestCase((SequenceType)7, "ChloroplastGenome")]
    [TestCase((SequenceType)8, "RRNA16S")]
    [TestCase((SequenceType)9, "RRNA18S")]
    [TestCase((SequenceType)10, "Mitochondrion16SRRNA")]
    [TestCase((SequenceType)11, "Plastid")]
    [TestCase((SequenceType)12, "MitochondrialPlasmid")]
    [TestCase((SequenceType)13, "CompleteImage")]
    [TestCase((SequenceType)14, "CompletePoem")]
    public void SequenceTypeNamesTest(SequenceType sequenceType, string name) => Assert.That(sequenceType.GetName(), Is.EqualTo(name));

    /// <summary>
    /// Tests that all sequence types have display value.
    /// </summary>
    /// <param name="sequenceType">
    /// The sequence type.
    /// </param>
    [Test]
    public void SequenceTypeHasDisplayValueTest([Values] SequenceType sequenceType) => Assert.That(sequenceType.GetDisplayValue(), Is.Not.Empty);

    /// <summary>
    /// Tests that all sequence types have description.
    /// </summary>
    /// <param name="sequenceType">
    /// The sequence Type.
    /// </param>
    [Test]
    public void SequenceTypeHasDescriptionTest([Values] SequenceType sequenceType) => Assert.That(sequenceType.GetDescription(), Is.Not.Empty);

    /// <summary>
    /// Tests that all sequence types have valid nature attribute.
    /// </summary>
    /// <param name="sequenceType">
    /// The sequence Type.
    /// </param>
    [Test]
    public void SequenceTypeHasNatureTest([Values] SequenceType sequenceType) => Assert.That(natures, Contains.Item(sequenceType.GetNature()));

    /// <summary>
    /// Tests that all sequence types values are unique.
    /// </summary>
    [Test]
    public void SequenceTypeValuesUniqueTest() => Assert.That(sequenceTypes.Cast<byte>(), Is.Unique);
}
