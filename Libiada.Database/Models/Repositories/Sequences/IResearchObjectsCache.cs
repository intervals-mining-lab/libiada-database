namespace Libiada.Database.Models.Repositories.Sequences
{
    public interface IResearchObjectsCache
    {
        List<ResearchObject> ResearchObjects { get; }

        void Clear();
    }
}