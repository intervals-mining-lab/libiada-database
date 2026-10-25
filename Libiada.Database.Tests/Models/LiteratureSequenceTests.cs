namespace Libiada.Database.Tests.Models;

[TestFixture]
public class LiteratureSequenceTests
{

    [Test]
    public void FromCombinedSequenceTest()
    {
        CombinedSequenceEntity combinedSequence = new()
        {
            Id = 1,
            Alphabet = [3,2,1],
            Order = [1,2,2,2,2,2,1,2,1,2,2,1,2,1,2],
            Notation = Notation.NormalizedWords,
            ResearchObjectId = 2,
            RemoteDb = RemoteDb.GenBank,
            RemoteId = "remote-id",
            ResearchObject = new ResearchObject(),
            CreatorId = 89,
            ModifierId = 1,
            Original = true,
            Language = Language.English,
            Translator = Translator.NoneOrManual
        };

        LiteratureSequence literatureSequence = LiteratureSequence.FromCombinedSequence(combinedSequence);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(literatureSequence.Id, Is.EqualTo(combinedSequence.Id));
            Assert.That(literatureSequence.Alphabet, Is.SameAs(combinedSequence.Alphabet));
            Assert.That(literatureSequence.Order, Is.SameAs(combinedSequence.Order));
            Assert.That(literatureSequence.Notation, Is.EqualTo(combinedSequence.Notation));
            Assert.That(literatureSequence.ResearchObjectId, Is.EqualTo(combinedSequence.ResearchObjectId));
            Assert.That(literatureSequence.RemoteDb, Is.EqualTo(combinedSequence.RemoteDb));
            Assert.That(literatureSequence.RemoteId, Is.EqualTo(combinedSequence.RemoteId));
            Assert.That(literatureSequence.ResearchObject, Is.SameAs(combinedSequence.ResearchObject));
            Assert.That(literatureSequence.CreatorId, Is.EqualTo(combinedSequence.CreatorId));
            Assert.That(literatureSequence.ModifierId, Is.EqualTo(combinedSequence.ModifierId));
            Assert.That(literatureSequence.Original, Is.EqualTo(combinedSequence.Original));
            Assert.That(literatureSequence.Language, Is.EqualTo(combinedSequence.Language));
            Assert.That(literatureSequence.Translator, Is.EqualTo(combinedSequence.Translator));
        }
    }

    [Test]
    public void FromCombinedSequenceRequiresLiteraturePropertiesTest()
    {
        Assert.Throws<Exception>(() => LiteratureSequence.FromCombinedSequence(new CombinedSequenceEntity() { Original = null }));
        Assert.Throws<Exception>(() => LiteratureSequence.FromCombinedSequence(new CombinedSequenceEntity() { Language = null }));
        Assert.Throws<Exception>(() => LiteratureSequence.FromCombinedSequence(new CombinedSequenceEntity() { Translator = null }));
    }
}
