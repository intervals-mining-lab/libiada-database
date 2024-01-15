namespace Libiada.Database;

using Microsoft.EntityFrameworkCore;

public interface ILibiadaDatabaseEntitiesFactory : IDbContextFactory<LibiadaDatabaseEntities>
{
}
