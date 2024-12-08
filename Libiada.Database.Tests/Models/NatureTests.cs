namespace Libiada.Database.Tests.Models;

using Libiada.Core.Extensions;

/// <summary>
/// Nature enum tests.
/// </summary>
[TestFixture(TestOf = typeof(Nature))]
public class NatureTests
{
    /// <summary>
    /// The natures count.
    /// </summary>
    private const int NaturesCount = 5;

    /// <summary>
    /// Array of all natures.
    /// </summary>
    private readonly Nature[] natures = EnumExtensions.ToArray<Nature>();

    /// <summary>
    /// Tests count of natures.
    /// </summary>
    [Test]
    public void NatureCountTest() => Assert.That(natures, Has.Length.EqualTo(NaturesCount));

    /// <summary>
    /// Tests values of natures.
    /// </summary>
    [Test]
    public void NatureValuesTest()
    {
        for (int i = 1; i <= NaturesCount; i++)
        {
            Assert.That(natures, Contains.Item((Nature)i));
        }
    }

    /// <summary>
    /// Tests names of natures.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="name">
    /// The name.
    /// </param>
    [TestCase((Nature)1, "Genetic")]
    [TestCase((Nature)2, "Music")]
    [TestCase((Nature)3, "Literature")]
    [TestCase((Nature)4, "MeasurementData")]
    [TestCase((Nature)5, "Image")]
    public void NatureNamesTest(Nature nature, string name) => Assert.That(nature.GetName(), Is.EqualTo(name));

    /// <summary>
    /// Tests that all natures have display value.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    [Test]
    public void NatureHasDisplayValueTest([Values] Nature nature) => Assert.That(nature.GetDisplayValue(), Is.Not.Empty);

    /// <summary>
    /// Tests that all natures have description.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    [Test]
    public void NatureHasDescriptionTest([Values] Nature nature) => Assert.That(nature.GetDescription(), Is.Not.Empty);

    /// <summary>
    /// Tests that all natures values are unique.
    /// </summary>
    [Test]
    public void NatureValuesUniqueTest() => Assert.That(natures.Cast<byte>(), Is.Unique);
}
