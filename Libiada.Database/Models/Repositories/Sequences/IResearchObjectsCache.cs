namespace Libiada.Database.Models.Repositories.Sequences
{
    public interface IResearchObjectsCache
    {
        List<ResearchObject> ResearchObjects { get; }
        List<long> ResearchObjectsWithSubsequencesIds { get; }
        void Clear();
    }
}