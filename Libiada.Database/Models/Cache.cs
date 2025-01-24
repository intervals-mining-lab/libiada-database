namespace Libiada.Database.Models;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Custom cache for storing research objects table data.
/// </summary>
public class Cache
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private List<ResearchObject>? researchObjects;
    private readonly object syncRoot = new();

    /// <summary>
    /// The list of research objects.
    /// </summary>
    public List<ResearchObject> ResearchObjects
    {
        get
        {
            if (researchObjects == null)
            {
                lock (syncRoot)
                {
                    if (researchObjects == null)
                    {
                        using var db = dbFactory.CreateDbContext();
                        researchObjects = db.ResearchObjects.ToList();
                    }
                }
            }

            return researchObjects;
        }
    }

    /// <summary>
    /// Initializes list of research objects.
    /// </summary>
    public Cache(IDbContextFactory<LibiadaDatabaseEntities> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    /// <summary>
    /// Clears the instance.
    /// </summary>
    public void Clear()
    {
        lock (syncRoot)
        {
            researchObjects = null;
        }
    }
}
