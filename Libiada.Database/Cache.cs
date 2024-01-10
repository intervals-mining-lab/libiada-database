namespace Libiada.Database
{
    /// <summary>
    /// Custom cache for storing matter table data.
    /// </summary>
    public class Cache
    {
        private readonly LibiadaDatabaseEntities db;

        /// <summary>
        /// The list of matters.
        /// </summary>
        public List<Matter> Matters { get; set; }

        /// <summary>
        /// Initializes list of matters.
        /// </summary>
        public Cache(ILibiadaDatabaseEntitiesFactory libiadaDatabaseEntitiesFactory)
        {
            this.db = libiadaDatabaseEntitiesFactory.CreateDbContext();
            Matters = db.Matters.ToList();
        }

        /// <summary>
        /// Clears the instance.
        /// </summary>
        public void Clear()
        {
            Matters = db.Matters.ToList();
        }
    }
}
