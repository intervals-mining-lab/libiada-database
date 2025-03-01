﻿namespace Libiada.Database.Tests.Models;

using Libiada.Core.Extensions;

using Libiada.Database.Extensions;

using EnumExtensions = Core.Extensions.EnumExtensions;

/// <summary>
/// Notation enum tests.
/// </summary>
[TestFixture(TestOf = typeof(Notation))]
public class NotationTests
{
    /// <summary>
    /// The notations count.
    /// </summary>
    private const int NotationsCount = 13;

    /// <summary>
    /// Array of all notations.
    /// </summary>
    private readonly Notation[] notations = EnumExtensions.ToArray<Notation>();

    /// <summary>
    /// Array of all natures.
    /// </summary>
    private readonly Nature[] natures = EnumExtensions.ToArray<Nature>();

    /// <summary>
    /// Tests count of notations.
    /// </summary>
    [Test]
    public void NotationCountTest() => Assert.That(notations, Has.Length.EqualTo(NotationsCount));

    /// <summary>
    /// Tests values of notations.
    /// </summary>
    [Test]
    public void NotationValuesTest()
    {
        for (int i = 1; i <= NotationsCount; i++)
        {
            Assert.That(notations, Contains.Item((Notation)i));
        }
    }

    /// <summary>
    /// Tests names of notations.
    /// </summary>
    /// <param name="notation">
    /// The notation.
    /// </param>
    /// <param name="name">
    /// The name.
    /// </param>
    [TestCase((Notation)1, "Nucleotides")]
    [TestCase((Notation)2, "Triplets")]
    [TestCase((Notation)3, "AminoAcids")]
    [TestCase((Notation)4, "GeneticWords")]
    [TestCase((Notation)5, "NormalizedWords")]
    [TestCase((Notation)6, "FormalMotifs")]
    [TestCase((Notation)7, "Measures")]
    [TestCase((Notation)8, "Notes")]
    [TestCase((Notation)9, "Letters")]
    [TestCase((Notation)10, "IntegerValues")]
    [TestCase((Notation)11, "Consonance")]
    [TestCase((Notation)12, "Phonemes")]
    [TestCase((Notation)13, "Pixels")]
    public void NotationNamesTest(Notation notation, string name) => Assert.That(notation.GetName(), Is.EqualTo(name));

    /// <summary>
    /// Tests that all notations have display value.
    /// </summary>
    /// <param name="notation">
    /// The notation.
    /// </param>
    [Test]
    public void NotationHasDisplayValueTest([Values] Notation notation) => Assert.That(notation.GetDisplayValue(), Is.Not.Empty);

    /// <summary>
    /// Tests that all notations have description.
    /// </summary>
    /// <param name="notation">
    /// The notation.
    /// </param>
    [Test]
    public void NotationHasDescriptionTest([Values] Notation notation) => Assert.That(notation.GetDescription(), Is.Not.Empty);

    /// <summary>
    /// Tests that all notations have valid nature attribute.
    /// </summary>
    /// <param name="notation">
    /// The notation.
    /// </param>
    [Test]
    public void NotationHasNatureTest([Values] Notation notation) => Assert.That(natures, Contains.Item(notation.GetNature()));

    /// <summary>
    /// Tests that all notations values are unique.
    /// </summary>
    [Test]
    public void NotationValuesUniqueTest() => Assert.That(notations.Cast<byte>(), Is.Unique);
}
