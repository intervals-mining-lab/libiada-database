namespace Libiada.Database.Models.Repositories.Sequences;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The sequence importer.
/// </summary>
public abstract class SequenceImporter : IDisposable
{
    /// <summary>
    /// Database context.
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
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    protected SequenceImporter(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, Cache cache)
    {
        Db = dbFactory.CreateDbContext();
        MatterRepository = new MatterRepository(Db, cache);
        ElementRepository = new ElementRepository(Db);
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}
