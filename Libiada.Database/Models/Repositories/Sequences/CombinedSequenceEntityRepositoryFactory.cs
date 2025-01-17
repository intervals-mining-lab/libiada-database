using Microsoft.EntityFrameworkCore;

namespace Libiada.Database.Models.Repositories.Sequences;

public class CombinedSequenceEntityRepositoryFactory(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, Cache cache) : ICombinedSequenceEntityRepositoryFactory
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory = dbFactory;
    private readonly Cache cache = cache;

    public ICombinedSequenceEntityRepository Create() => new CombinedSequenceEntityRepository(dbFactory, cache);
}
