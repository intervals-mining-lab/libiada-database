namespace Libiada.Database.Models.Repositories.Sequences
{
    using LibiadaCore.Core;
    using LibiadaCore.Music;
    using System;
    using System.Collections.Generic;

    public interface ICommonSequenceRepository : IDisposable
    {
        void Create(CommonSequence sequence, long[] alphabet, int[] building);
        void Dispose();
        List<Element> GetElements(long sequenceId);
        BaseChain GetLibiadaBaseChain(long sequenceId);
        Chain GetLibiadaChain(long sequenceId);
        long[] GetSequenceIds(long[] matterIds, Notation notation, Language? language, Translator? translator, PauseTreatment? pauseTreatment, bool? sequentialTransfer, ImageOrderExtractor? imageOrderExtractor);
        long[][] GetSequenceIds(long[] matterIds, Notation[] notations, Language?[] languages, Translator?[] translators, PauseTreatment?[] pauseTreatments, bool?[] sequentialTransfers, ImageOrderExtractor?[] imageOrderExtractors);
        string GetString(long sequenceId);
    }
}
