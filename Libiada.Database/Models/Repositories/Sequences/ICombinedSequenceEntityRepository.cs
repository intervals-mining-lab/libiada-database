namespace Libiada.Database.Models.Repositories.Sequences;

using Libiada.Core.Core;
using Libiada.Core.Music;

public interface ICombinedSequenceEntityRepository : IDisposable
{
    Element[] GetElements(long sequenceId);
    Sequence GetLibiadaSequence(long sequenceId);
    ComposedSequence GetLibiadaComposedSequence(long sequenceId);
    long[] GetSequenceIds(long[] matterIds, Notation notation, Language? language, Translator? translator, PauseTreatment? pauseTreatment, bool? sequentialTransfer, ImageOrderExtractor? imageOrderExtractor);
    long[][] GetSequenceIds(long[] matterIds, Notation[] notations, Language[] languages, Translator[] translators, PauseTreatment[] pauseTreatments, bool[] sequentialTransfers, ImageOrderExtractor[] imageOrderExtractors);
    string GetString(long sequenceId);
    long Create(CombinedSequenceEntity sequence);
}
