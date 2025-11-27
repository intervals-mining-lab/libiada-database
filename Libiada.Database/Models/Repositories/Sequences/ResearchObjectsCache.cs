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
    private List<long>? researchObjectsWithSubsequencesIds;
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

    public List<long> ResearchObjectsWithSubsequencesIds
    {
        get
        {
            if (researchObjectsWithSubsequencesIds == null)
            {
                lock (syncRoot)
                {
                    if (researchObjectsWithSubsequencesIds == null)
                    {
                        using var db = dbFactory.CreateDbContext();
                        var sequenceIds = db.Subsequences.Select(s => s.SequenceId).Distinct();
                        researchObjectsWithSubsequencesIds = db.CombinedSequenceEntities
                                                               .Where(c => sequenceIds.Contains(c.Id))
                                                               .Select(c => c.ResearchObjectId)
                                                               .ToList();
                    }
                }
            }

            return researchObjectsWithSubsequencesIds;
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
