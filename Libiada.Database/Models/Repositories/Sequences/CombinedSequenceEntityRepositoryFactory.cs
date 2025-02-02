using Microsoft.EntityFrameworkCore;

namespace Libiada.Database.Models.Repositories.Sequences;

public class CombinedSequenceEntityRepositoryFactory(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, IResearchObjectsCache cache) : ICombinedSequenceEntityRepositoryFactory
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory = dbFactory;
    private readonly IResearchObjectsCache cache = cache;

    public ICombinedSequenceEntityRepository Create() => new CombinedSequenceEntityRepository(dbFactory, cache);
}
