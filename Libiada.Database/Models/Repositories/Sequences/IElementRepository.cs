using Libiada.Core.Core;

namespace Libiada.Database.Models.Repositories.Sequences
{
    public interface IElementRepository
    {
        bool ElementsInDb(Alphabet alphabet, Notation notation);
        Element[] GetElements(long[] elementIds);
        long[] GetOrCreateNotesInDb(Alphabet alphabet);
        long[] ToDbElements(Alphabet alphabet, Notation notation, bool createElements);
        Alphabet ToLibiadaAlphabet(long[] elementIds);
    }
}