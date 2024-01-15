namespace Libiada.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class LibiadaDatabaseEntitiesFactory : ILibiadaDatabaseEntitiesFactory
{
    private readonly IConfiguration configuration;

    public LibiadaDatabaseEntitiesFactory(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public LibiadaDatabaseEntities CreateDbContext()
    {
        DbContextOptions<LibiadaDatabaseEntities> options = new DbContextOptions<LibiadaDatabaseEntities>();
        return new LibiadaDatabaseEntities(options, configuration);
    }

}
