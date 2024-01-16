﻿namespace Libiada.Database.Tests.Models;

using Libiada.Core.Extensions;

/// <summary>
/// Language enum tests.
/// </summary>
[TestFixture(TestOf = typeof(Language))]
public class LanguageTests
{
    /// <summary>
    /// The languages count.
    /// </summary>
    private const int LanguagesCount = 3;

    /// <summary>
    /// Array of all languages.
    /// </summary>
    private readonly Language[] languages = EnumExtensions.ToArray<Language>();

    /// <summary>
    /// Tests count of languages.
    /// </summary>
    [Test]
    public void LanguageCountTest() => Assert.That(languages.Length, Is.EqualTo(LanguagesCount));

    /// <summary>
    /// Tests values of languages.
    /// </summary>
    [Test]
    public void LanguageValuesTest()
    {
        for (int i = 1; i <= LanguagesCount; i++)
        {
            Assert.That(languages, Has.Member((Language)i));
        }
    }

    /// <summary>
    /// Tests names of languages.
    /// </summary>
    /// <param name="language">
    /// The language.
    /// </param>
    /// <param name="name">
    /// The name.
    /// </param>
    [TestCase((Language)1, "Russian")]
    [TestCase((Language)2, "English")]
    [TestCase((Language)3, "German")]
    public void LanguageNamesTest(Language language, string name) => Assert.That(language.GetName(), Is.EqualTo(name));

    /// <summary>
    /// Tests that all languages have display value.
    /// </summary>
    /// <param name="language">
    /// The language.
    /// </param>
    [Test]
    public void LanguageHasDisplayValueTest([Values] Language language) => Assert.That(language.GetDisplayValue(), Is.Not.Null.And.Not.Empty);

    /// <summary>
    /// Tests that all languages have description.
    /// </summary>
    /// <param name="language">
    /// The language.
    /// </param>
    [Test]
    public void LanguageHasDescriptionTest([Values] Language language) => Assert.That(language.GetDescription(), Is.Not.Null.And.Not.Empty);

    /// <summary>
    /// Tests that all languages values are unique.
    /// </summary>
    [Test]
    public void LanguageValuesUniqueTest() => Assert.That(languages.Cast<byte>(), Is.Unique);
}
