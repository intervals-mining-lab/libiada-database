using Microsoft.EntityFrameworkCore;

namespace Libiada.Database.Models.Repositories.Sequences;

public class CommonSequenceRepositoryFactory(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, Cache cache) : ICommonSequenceRepositoryFactory
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory = dbFactory;
    private readonly Cache cache = cache;

    public ICommonSequenceRepository Create()
    {
        return new CommonSequenceRepository(dbFactory, cache);
    }
}
