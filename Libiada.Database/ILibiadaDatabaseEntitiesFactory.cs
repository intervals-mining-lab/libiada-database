namespace Libiada.Database
{
    using System.Data.Entity.Infrastructure;

    public interface ILibiadaDatabaseEntitiesFactory : IDbContextFactory<LibiadaDatabaseEntities>
    {
    }
}
