namespace Libiada.Database.Tests.Models;

using Libiada.Core.Music;

[TestFixture]
public class MusicSequenceTests
{
    [Test]
    public void FromCombinedSequenceTest()
    {
        CombinedSequenceEntity combinedSequence = new()
        {
            Id = 1,
            Alphabet = [2, 3],
            Order = [1, 0],
            Notation = Notation.Notes,
            ResearchObjectId = 4,
            RemoteDb = RemoteDb.GenBank,
            RemoteId = "remote-id",
            ResearchObject = new ResearchObject(),
            CreatorId = 5,
            ModifierId = 6,
            SequentialTransfer = false,
            PauseTreatment = PauseTreatment.Ignore
        };

        MusicSequence musicSequence = MusicSequence.FromCombinedSequence(combinedSequence);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(musicSequence.Id, Is.EqualTo(combinedSequence.Id));
            Assert.That(musicSequence.Alphabet, Is.SameAs(combinedSequence.Alphabet));
            Assert.That(musicSequence.Order, Is.SameAs(combinedSequence.Order));
            Assert.That(musicSequence.Notation, Is.EqualTo(combinedSequence.Notation));
            Assert.That(musicSequence.ResearchObjectId, Is.EqualTo(combinedSequence.ResearchObjectId));
            Assert.That(musicSequence.RemoteDb, Is.EqualTo(combinedSequence.RemoteDb));
            Assert.That(musicSequence.RemoteId, Is.EqualTo(combinedSequence.RemoteId));
            Assert.That(musicSequence.ResearchObject, Is.SameAs(combinedSequence.ResearchObject));
            Assert.That(musicSequence.CreatorId, Is.EqualTo(combinedSequence.CreatorId));
            Assert.That(musicSequence.ModifierId, Is.EqualTo(combinedSequence.ModifierId));
            Assert.That(musicSequence.SequentialTransfer, Is.EqualTo(combinedSequence.SequentialTransfer));
            Assert.That(musicSequence.PauseTreatment, Is.EqualTo(combinedSequence.PauseTreatment));
        }
    }

    [Test]
    public void FromCombinedSequenceRequiresMusicPropertiesTest()
    {
        Assert.Throws<Exception>(() => MusicSequence.FromCombinedSequence(new CombinedSequenceEntity() { SequentialTransfer = null }));
        Assert.Throws<Exception>(() => MusicSequence.FromCombinedSequence(new CombinedSequenceEntity() { PauseTreatment = null }));
    }
}
