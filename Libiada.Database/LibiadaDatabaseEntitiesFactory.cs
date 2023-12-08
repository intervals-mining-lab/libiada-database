namespace Libiada.Database
{
    using Microsoft.Extensions.Configuration;

    public class LibiadaDatabaseEntitiesFactory : ILibiadaDatabaseEntitiesFactory
    {
        private readonly IConfiguration configuration;

        public LibiadaDatabaseEntitiesFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public LibiadaDatabaseEntities Create() => new LibiadaDatabaseEntities(configuration.GetConnectionString("LibiadaDatabaseEntities"));
    }
}
