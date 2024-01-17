namespace Libiada.Database.Models.Repositories.Sequences;

/// <summary>
/// The sequence importer.
/// </summary>
public abstract class SequenceImporter
{
    /// <summary>
    /// The db.
    /// </summary>
    protected readonly LibiadaDatabaseEntities Db;

    /// <summary>
    /// The matters repository.
    /// </summary>
    protected readonly MatterRepository MatterRepository;

    /// <summary>
    /// The elements repository.
    /// </summary>
    protected readonly ElementRepository ElementRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceImporter"/> class.
    /// </summary>
    /// <param name="db">
    /// The db.
    /// </param>
    protected SequenceImporter(ILibiadaDatabaseEntitiesFactory dbFactory, Cache cache)
    {
        Db = dbFactory.CreateDbContext();
        MatterRepository = new MatterRepository(Db, cache);
        ElementRepository = new ElementRepository(Db);
    }
}
