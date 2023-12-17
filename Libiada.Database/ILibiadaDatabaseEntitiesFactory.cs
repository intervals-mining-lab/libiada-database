using Microsoft.EntityFrameworkCore;

namespace Libiada.Database
{

    public interface ILibiadaDatabaseEntitiesFactory : IDbContextFactory<LibiadaDatabaseEntities>
    {
    }
}
