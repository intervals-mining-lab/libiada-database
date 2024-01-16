namespace Libiada.Database.Models;

using Microsoft.EntityFrameworkCore;

public interface ILibiadaDatabaseEntitiesFactory : IDbContextFactory<LibiadaDatabaseEntities>
{
}
