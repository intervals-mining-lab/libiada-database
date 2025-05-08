namespace Libiada.Web.Tests.Helpers;

using Libiada.Database.Helpers;
using Libiada.Database.Tests;
using System.Text;

[TestFixture(TestOf = typeof(FileHelper))]
public class FileHelperTests
{
    /// <summary>
    /// Reads the sequence from stream simple text test.
    /// </summary>
    [Test]
    public void ReadSequenceFromStreamSimpleTextTest()
    {
        string expected = "Hello World";
        byte[] bytes = Encoding.UTF8.GetBytes(expected);
        using MemoryStream stream = new(bytes);

        string result = FileHelper.ReadSequenceFromStream(stream);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(result.Length, Is.EqualTo(11));
            Assert.That(result, Is.TypeOf<string>());
        });
    }

    /// <summary>
    /// Reads the sequence from stream empty test.
    /// </summary>
    [Test]
    public void ReadSequenceFromStreamEmptyTest()
    {
        using MemoryStream stream = new();

        string result = FileHelper.ReadSequenceFromStream(stream);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(result.Length, Is.EqualTo(0));
            Assert.That(result, Is.TypeOf<string>());
        });
    }

    /// <summary>
    /// Reads the sequence from stream special characters test.
    /// </summary>
    [Test]
    public void ReadSequenceFromStreamSpecialCharactersTest()
    {
        string expected = "Hello\nWorld\t!@#$%^&*";
        byte[] bytes = Encoding.UTF8.GetBytes(expected);
        using MemoryStream stream = new(bytes);

        string result = FileHelper.ReadSequenceFromStream(stream);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(result.Length, Is.EqualTo(20));
            Assert.That(result, Is.TypeOf<string>());
        });
    }
}
