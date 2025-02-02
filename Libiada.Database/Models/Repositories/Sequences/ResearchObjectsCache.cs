namespace Libiada.Database.Models.Repositories.Sequences;

using Microsoft.EntityFrameworkCore;
using System.Threading;

/// <summary>
/// Custom cache for storing research objects table data.
/// </summary>
/// <remarks>
/// Initializes list of research objects.
/// </remarks>
public class ResearchObjectsCache(IDbContextFactory<LibiadaDatabaseEntities> dbFactory) : IResearchObjectsCache
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory = dbFactory;
    private List<ResearchObject>? researchObjects;
    private readonly Lock syncRoot = new();

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
