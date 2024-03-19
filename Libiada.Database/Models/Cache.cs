namespace Libiada.Database.Models;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Custom cache for storing matter table data.
/// </summary>
public class Cache
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private List<Matter>? matters;
    private readonly object syncRoot = new();

    /// <summary>
    /// The list of matters.
    /// </summary>
    public List<Matter> Matters 
    {
        get
        {
            if(matters == null)
            {
                lock (syncRoot)
                {
                    if (matters == null)
                    {
                        using var db = dbFactory.CreateDbContext();
                        matters = db.Matters.ToList();
                    }
                }
            }
            
            return matters;
        }
    }

    /// <summary>
    /// Initializes list of matters.
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
            matters = null;
        }
    }
}
