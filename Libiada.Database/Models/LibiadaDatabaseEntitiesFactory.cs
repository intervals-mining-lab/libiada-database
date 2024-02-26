namespace Libiada.Database.Models;

using Microsoft.EntityFrameworkCore;

public class LibiadaDatabaseEntitiesFactory : ILibiadaDatabaseEntitiesFactory
{
    private readonly DbContextOptions<LibiadaDatabaseEntities> options;

    public LibiadaDatabaseEntitiesFactory(DbContextOptions<LibiadaDatabaseEntities> options)
    {
        this.options = options;
    }

    public LibiadaDatabaseEntities CreateDbContext() => new LibiadaDatabaseEntities(options);

}
