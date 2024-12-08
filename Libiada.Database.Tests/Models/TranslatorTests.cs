namespace Libiada.Database.Tests.Models;

using Libiada.Core.Extensions;

/// <summary>
/// Translator enum tests.
/// </summary>
[TestFixture(TestOf = typeof(Translator))]
public class TranslatorTests
{
    /// <summary>
    /// The translators count.
    /// </summary>
    private const int TranslatorsCount = 4;

    /// <summary>
    /// Array of all automatic translators.
    /// </summary>
    private readonly Translator[] translators = EnumExtensions.ToArray<Translator>();

    /// <summary>
    /// Tests count of translators.
    /// </summary>
    [Test]
    public void TranslatorCountTest() => Assert.That(translators, Has.Length.EqualTo(TranslatorsCount));

    /// <summary>
    /// Tests values of translators.
    /// </summary>
    [Test]
    public void TranslatorValuesTest()
    {
        for (int i = 0; i < TranslatorsCount; i++)
        {
            Assert.That(translators, Contains.Item((Translator)i));
        }
    }

    /// <summary>
    /// Tests names of translators.
    /// </summary>
    /// <param name="translator">
    /// The translator.
    /// </param>
    /// <param name="name">
    /// The name.
    /// </param>
    [TestCase((Translator)0, "NoneOrManual")]
    [TestCase((Translator)1, "GoogleTranslate")]
    [TestCase((Translator)2, "Promt")]
    [TestCase((Translator)3, "InterTran")]
    public void TranslatorNamesTest(Translator translator, string name) => Assert.That(translator.GetName(), Is.EqualTo(name));

    /// <summary>
    /// Tests that all translators have display value.
    /// </summary>
    /// <param name="translator">
    /// The translator.
    /// </param>
    [Test]
    public void TranslatorHasDisplayValueTest([Values] Translator translator) => Assert.That(translator.GetDisplayValue(), Is.Not.Empty);

    /// <summary>
    /// Tests that all translators have description.
    /// </summary>
    /// <param name="translator">
    /// The translator.
    /// </param>
    [Test]
    public void TranslatorHasDescriptionTest([Values] Translator translator) => Assert.That(translator.GetDescription(), Is.Not.Empty);

    /// <summary>
    /// Tests that all translators values are unique.
    /// </summary>
    [Test]
    public void TranslatorValuesUniqueTest() => Assert.That(translators.Cast<byte>(), Is.Unique);
}
