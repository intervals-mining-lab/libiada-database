namespace Libiada.Database.Models;

/// <summary>
/// Custom cache for storing matter table data.
/// </summary>
public class Cache
{
    private readonly ILibiadaDatabaseEntitiesFactory dbFactory;
    private List<Matter>? matters;
    private readonly object syncRoot = new object();

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
    public Cache(ILibiadaDatabaseEntitiesFactory dbFactory)
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
