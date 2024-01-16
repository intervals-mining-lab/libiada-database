namespace Libiada.Web.Tests.Helpers;

using Libiada.Database.Helpers;
using Libiada.Database.Tests;

[TestFixture(TestOf = typeof(NcbiHelper))]
public class NcbiHelperTests
{

    [Test]
    public void GetIDFromFileTest()
    {
        var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result.txt");
        var textFromFile = txtReader.ReadToEnd();
        var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true);
        const int expectedSequencesCount = 2111;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void IncludePartialInGetIdFromFileTest()
    {
        var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
        var textFromFile = txtReader.ReadToEnd();
        var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false);
        int expectedSequencesCount = 1447;
        const int partialSequences = 823;
        expectedSequencesCount -= partialSequences;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void LengthInGetIdFromFileTest()
    {
        var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
        var textFromFile = txtReader.ReadToEnd();
        var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, 5000, 100000);
        const int expectedSequencesCount = 122;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void LengthPartialFalseInGetIdFromFileWithTest()
    {
        var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
        var textFromFile = txtReader.ReadToEnd();
        var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, 5000, 100000);
        const int expectedSequencesCount = 121;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void MaxLengthPartialFalseInGetIdFromFileTest()
    {
        var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
        var textFromFile = txtReader.ReadToEnd();
        var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, maxLength: 10000);
        const int expectedSequencesCount = 507;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void MaxLengthPartialTrueInGetIdFromFileTest()
    {
        var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
        var textFromFile = txtReader.ReadToEnd();
        var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, maxLength: 10000);
        const int expectedSequencesCount = 1330;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }

    [Test]
    public void MinLengthPartialTrueInGetIdFromFileTest()
    {
        var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
        var textFromFile = txtReader.ReadToEnd();
        var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, minLength: 30000);
        const int expectedSequencesCount = 115;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }
    [Test]
    public void MinLengthPartialFalseInGetIdFromFileTest()
    {
        var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
        var textFromFile = txtReader.ReadToEnd();
        var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, minLength: 1000);
        const int expectedSequencesCount = 415;
        Assert.That(result.Length, Is.EqualTo(expectedSequencesCount));
    }
}
