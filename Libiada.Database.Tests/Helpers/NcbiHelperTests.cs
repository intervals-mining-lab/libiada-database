namespace Libiada.Web.Tests.Helpers;

using Libiada.Database.Helpers;
using Libiada.Database.Tests;

[TestFixture(TestOf = typeof(NcbiHelper))]
public class NcbiHelperTests
{

    private readonly string TestDataFolderPath = Path.Join(TestContext.CurrentContext.TestDirectory, "TestData");

    [Test]
    public void GetIDFromFileTest()
    {
        StreamReader txtReader = new(Path.Join(TestDataFolderPath, "nuccore_result.txt"));
        string textFromFile = txtReader.ReadToEnd();
        string[] result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true);
        const int expectedSequencesCount = 2111;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void IncludePartialInGetIdFromFileTest()
    {
        StreamReader txtReader = new(Path.Join(TestDataFolderPath, "nuccore_result2.txt"));
        string textFromFile = txtReader.ReadToEnd();
        string[] result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false);
        int expectedSequencesCount = 1447;
        const int partialSequences = 823;
        expectedSequencesCount -= partialSequences;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void LengthInGetIdFromFileTest()
    {
        StreamReader txtReader = new(Path.Join(TestDataFolderPath, "nuccore_result2.txt"));
        string textFromFile = txtReader.ReadToEnd();
        string[] result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, 5000, 100000);
        const int expectedSequencesCount = 122;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void LengthPartialFalseInGetIdFromFileWithTest()
    {
        StreamReader txtReader = new(Path.Join(TestDataFolderPath, "nuccore_result2.txt"));
        string textFromFile = txtReader.ReadToEnd();
        string[] result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, 5000, 100000);
        const int expectedSequencesCount = 121;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void MaxLengthPartialFalseInGetIdFromFileTest()
    {
        StreamReader txtReader = new(Path.Join(TestDataFolderPath, "nuccore_result2.txt"));
        string textFromFile = txtReader.ReadToEnd();
        string[] result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, maxLength: 10000);
        const int expectedSequencesCount = 507;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void MaxLengthPartialTrueInGetIdFromFileTest()
    {
        StreamReader txtReader = new(Path.Join(TestDataFolderPath, "nuccore_result2.txt"));
        string textFromFile = txtReader.ReadToEnd();
        string[] result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, maxLength: 10000);
        const int expectedSequencesCount = 1330;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void MinLengthPartialTrueInGetIdFromFileTest()
    {
        StreamReader txtReader = new(Path.Join(TestDataFolderPath, "nuccore_result2.txt"));
        string textFromFile = txtReader.ReadToEnd();
        string[] result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, minLength: 30000);
        const int expectedSequencesCount = 115;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }
    [Test]
    public void MinLengthPartialFalseInGetIdFromFileTest()
    {
        StreamReader txtReader = new(Path.Join(TestDataFolderPath, "nuccore_result2.txt"));
        string textFromFile = txtReader.ReadToEnd();
        string[] result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, minLength: 1000);
        const int expectedSequencesCount = 415;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }
}
