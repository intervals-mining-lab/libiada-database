namespace Libiada.Database.Tests.Models;

[TestFixture]
public class GeneticSequenceTests
{
    [Test]
    public void FromCombinedSequenceTest()
    {
        CombinedSequenceEntity combinedSequence = new()
        {
            Id = 1,
            Alphabet = [3, 76],
            Order = [1, 1, 2, 2, 2, 2, 2, 1, 2, 1, 2, 1, 2, 1, 2],
            Notation = Notation.Nucleotides,
            ResearchObjectId = 1,
            RemoteDb = RemoteDb.GenBank,
            RemoteId = "1",
            ResearchObject = new ResearchObject(),
            CreatorId = 77,
            ModifierId = 77,
            Partial = false
        };

        GeneticSequence geneticSequence = GeneticSequence.FromCombinedSequence(combinedSequence);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(geneticSequence.Id, Is.EqualTo(combinedSequence.Id));
            Assert.That(geneticSequence.Alphabet, Is.SameAs(combinedSequence.Alphabet));
            Assert.That(geneticSequence.Order, Is.SameAs(combinedSequence.Order));
            Assert.That(geneticSequence.Notation, Is.EqualTo(combinedSequence.Notation));
            Assert.That(geneticSequence.ResearchObjectId, Is.EqualTo(combinedSequence.ResearchObjectId));
            Assert.That(geneticSequence.RemoteDb, Is.EqualTo(combinedSequence.RemoteDb));
            Assert.That(geneticSequence.RemoteId, Is.EqualTo(combinedSequence.RemoteId));
            Assert.That(geneticSequence.ResearchObject, Is.SameAs(combinedSequence.ResearchObject));
            Assert.That(geneticSequence.CreatorId, Is.EqualTo(combinedSequence.CreatorId));
            Assert.That(geneticSequence.ModifierId, Is.EqualTo(combinedSequence.ModifierId));
            Assert.That(geneticSequence.Partial, Is.EqualTo(combinedSequence.Partial));
        }
    }

    [Test]
    public void FromCombinedSequenceRequiresPartialTest() =>
        Assert.Throws<Exception>(() => GeneticSequence.FromCombinedSequence(new CombinedSequenceEntity() { Partial = null }));
}