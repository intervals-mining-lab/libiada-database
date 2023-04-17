namespace Libiada.Database
{
    using System.Collections.Generic;
    using System.Linq;

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
            this.db = libiadaDatabaseEntitiesFactory.Create();
            Matters = db.Matter.ToList();
        }

        /// <summary>
        /// Clears the instance.
        /// </summary>
        public void Clear()
        {
            Matters = db.Matter.ToList();
        }
    }
}
