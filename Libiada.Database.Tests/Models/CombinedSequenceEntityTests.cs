namespace Libiada.Database.Tests.Models;

using Libiada.Core.Music;

[TestFixture]
public class CombinedSequenceEntityTests
{
    [Test]
    public void FromMusicSequenceTest()
    {
        MusicSequence musicSequence = new()
        {
            Id = 1,
            Alphabet = [1, 2],
            Order = [1, 2],
            Notation = Notation.Notes,
            ResearchObjectId = 4,
            RemoteDb = RemoteDb.GenBank,
            RemoteId = "music-remote-id",
            ResearchObject = new ResearchObject(),
            CreatorId = 5,
            ModifierId = 6,
            SequentialTransfer = true,
            PauseTreatment = PauseTreatment.Ignore
        };

        CombinedSequenceEntity combinedSequence = CombinedSequenceEntity.FromMusicSequence(musicSequence);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(combinedSequence.Id, Is.EqualTo(musicSequence.Id));
            Assert.That(combinedSequence.Alphabet, Is.SameAs(musicSequence.Alphabet));
            Assert.That(combinedSequence.Order, Is.SameAs(musicSequence.Order));
            Assert.That(combinedSequence.Nature, Is.EqualTo(Nature.Music));
            Assert.That(combinedSequence.Notation, Is.EqualTo(musicSequence.Notation));
            Assert.That(combinedSequence.ResearchObjectId, Is.EqualTo(musicSequence.ResearchObjectId));
            Assert.That(combinedSequence.RemoteDb, Is.EqualTo(musicSequence.RemoteDb));
            Assert.That(combinedSequence.RemoteId, Is.EqualTo(musicSequence.RemoteId));
            Assert.That(combinedSequence.ResearchObject, Is.SameAs(musicSequence.ResearchObject));
            Assert.That(combinedSequence.CreatorId, Is.EqualTo(musicSequence.CreatorId));
            Assert.That(combinedSequence.ModifierId, Is.EqualTo(musicSequence.ModifierId));
            Assert.That(combinedSequence.SequentialTransfer, Is.EqualTo(musicSequence.SequentialTransfer));
            Assert.That(combinedSequence.PauseTreatment, Is.EqualTo(musicSequence.PauseTreatment));
        }
    }

    [Test]
    public void FromGeneticSequenceTest()
    {
        GeneticSequence geneticSequence = new()
        {
            Id = 1,
            Alphabet = [3, 54, 2],
            Order = [1, 1, 1, 2, 2, 2, 1, 2, 1, 2, 3],
            Notation = Notation.Nucleotides,
            ResearchObjectId = 4,
            RemoteDb = RemoteDb.GenBank,
            RemoteId = "genetic-remote-id",
            ResearchObject = new ResearchObject(),
            CreatorId = 5,
            ModifierId = 2,
            Partial = true
        };

        CombinedSequenceEntity combinedSequence = CombinedSequenceEntity.FromGeneticSequence(geneticSequence);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(combinedSequence.Id, Is.EqualTo(geneticSequence.Id));
            Assert.That(combinedSequence.Alphabet, Is.SameAs(geneticSequence.Alphabet));
            Assert.That(combinedSequence.Order, Is.SameAs(geneticSequence.Order));
            Assert.That(combinedSequence.Nature, Is.EqualTo(Nature.Genetic));
            Assert.That(combinedSequence.Notation, Is.EqualTo(geneticSequence.Notation));
            Assert.That(combinedSequence.ResearchObjectId, Is.EqualTo(geneticSequence.ResearchObjectId));
            Assert.That(combinedSequence.RemoteDb, Is.EqualTo(geneticSequence.RemoteDb));
            Assert.That(combinedSequence.RemoteId, Is.EqualTo(geneticSequence.RemoteId));
            Assert.That(combinedSequence.ResearchObject, Is.SameAs(geneticSequence.ResearchObject));
            Assert.That(combinedSequence.CreatorId, Is.EqualTo(geneticSequence.CreatorId));
            Assert.That(combinedSequence.ModifierId, Is.EqualTo(geneticSequence.ModifierId));
            Assert.That(combinedSequence.Partial, Is.EqualTo(geneticSequence.Partial));
        }
    }

    [Test]
    public void FromLiteratureSequenceTest()
    {
        LiteratureSequence literatureSequence = new()
        {
            Id = 1,
            Alphabet = [2, 3, 5, 4],
            Order = [1, 2, 3, 1, 2, 3, 1, 4, 3, 2, 4, 2, 3, 4, 2, 4, 2, 4, 1],
            Notation = Notation.NormalizedWords,
            ResearchObjectId = 4,
            RemoteDb = RemoteDb.GenBank,
            RemoteId = "literature-remote-id",
            ResearchObject = new ResearchObject(),
            CreatorId = 1,
            ModifierId = 1,
            Original = true,
            Language = Language.English,
            Translator = Translator.GoogleTranslate
        };

        CombinedSequenceEntity combinedSequence = CombinedSequenceEntity.FromLiteratureSequence(literatureSequence);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(combinedSequence.Id, Is.EqualTo(literatureSequence.Id));
            Assert.That(combinedSequence.Alphabet, Is.SameAs(literatureSequence.Alphabet));
            Assert.That(combinedSequence.Order, Is.SameAs(literatureSequence.Order));
            Assert.That(combinedSequence.Nature, Is.EqualTo(Nature.Literature));
            Assert.That(combinedSequence.Notation, Is.EqualTo(literatureSequence.Notation));
            Assert.That(combinedSequence.ResearchObjectId, Is.EqualTo(literatureSequence.ResearchObjectId));
            Assert.That(combinedSequence.RemoteDb, Is.EqualTo(literatureSequence.RemoteDb));
            Assert.That(combinedSequence.RemoteId, Is.EqualTo(literatureSequence.RemoteId));
            Assert.That(combinedSequence.ResearchObject, Is.SameAs(literatureSequence.ResearchObject));
            Assert.That(combinedSequence.CreatorId, Is.EqualTo(literatureSequence.CreatorId));
            Assert.That(combinedSequence.ModifierId, Is.EqualTo(literatureSequence.ModifierId));
            Assert.That(combinedSequence.Original, Is.EqualTo(literatureSequence.Original));
            Assert.That(combinedSequence.Language, Is.EqualTo(literatureSequence.Language));
            Assert.That(combinedSequence.Translator, Is.EqualTo(literatureSequence.Translator));
        }
    }
}
