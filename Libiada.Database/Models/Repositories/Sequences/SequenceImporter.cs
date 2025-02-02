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
    /// The research objects repository.
    /// </summary>
    protected readonly ResearchObjectRepository ResearchObjectRepository;

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
    protected SequenceImporter(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, IResearchObjectsCache cache)
    {
        Db = dbFactory.CreateDbContext();
        ResearchObjectRepository = new ResearchObjectRepository(Db, cache);
        ElementRepository = new ElementRepository(Db);
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}
