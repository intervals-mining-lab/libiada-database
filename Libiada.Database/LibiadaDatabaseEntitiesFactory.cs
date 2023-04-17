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

        public LibiadaDatabaseEntities Create() => new LibiadaDatabaseEntities("metadata=res://*/LibiadaWeb.csdl|res://*/LibiadaWeb.ssdl|res://*/LibiadaWeb.msl;provider=Npgsql;provider connection string=&quot;Server=localhost;Port=5434;Database=test;UserId=postgres;CommandTimeout=10000000;enlist=true;&quot;");
    }
}
