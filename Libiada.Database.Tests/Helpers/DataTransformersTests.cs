namespace Libiada.Web.Tests.Helpers;

using Libiada.Database.Helpers;
using Libiada.Database.Tests;

[TestFixture(TestOf = typeof(DataTransformers))]
public class DataTransformersTests
{
    /// <summary>
    /// Cleans the fasta file null character test.
    /// </summary>
    [Test]
    public void CleanFastaFileNullCharTest()
    {
        string input = "ACGT\0TGCA";
        string expected = "ACGTTGCA";
        string result = DataTransformers.CleanFastaFile(input);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(result.Length, Is.EqualTo(8));
            Assert.That(result, Is.TypeOf<string>());
        });
    }

    /// <summary>
    /// Cleans the fasta file tab test.
    /// </summary>
    [Test]
    public void CleanFastaFileTabTest()
    {
        string input = "ACGT\tTGCA";
        string expected = "ACGTTGCA";
        string result = DataTransformers.CleanFastaFile(input);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(result.Length, Is.EqualTo(8));
            Assert.That(result, Is.TypeOf<string>());
        });
    }

    /// <summary>
    /// Cleans the fasta file mixed (with both null and tab)  test.
    /// </summary>
    [Test]
    public void CleanFastaFileMixedTest()
    {
        string input = "AC\0GT\tTG\0CA";
        string expected = "ACGTTGCA";
        string result = DataTransformers.CleanFastaFile(input);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(result.Length, Is.EqualTo(8));
            Assert.That(result, Is.TypeOf<string>());
        });
    }

    /// <summary>
    /// Cleans the fasta file clean string test.
    /// </summary>
    [Test]
    public void CleanFastaFileCleanStringTest()
    {
        string input = "ACGTTGCA";
        string expected = "ACGTTGCA";
        string result = DataTransformers.CleanFastaFile(input);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(result.Length, Is.EqualTo(8));
            Assert.That(result, Is.TypeOf<string>());
            Assert.That(result, Is.EqualTo(input));
        });
    }
}
