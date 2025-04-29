namespace Libiada.Database.Extensions;

using Npgsql;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public static class LibiadaDatabaseEntitiesExtensions
{
    /// <summary>
    /// Gets new element id from database.
    /// </summary>
    /// <param name="db">
    /// Database connection.
    /// </param>
    /// <returns>
    /// The <see cref="long"/> value of new id.
    /// </returns>
    public static long GetNewElementId(this LibiadaDatabaseEntities db)
    {
        // using "Value" psudonim for ef core wrap
        return db.Database.SqlQuery<long>($"SELECT nextval('elements_id_seq') AS \"Value\"").Single();
    }

    /// <summary>
    /// The execute custom sql command with parameters.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    /// <param name="query">
    /// The query.
    /// </param>
    /// <param name="parameters">
    /// The parameters.
    /// </param>
    public static void ExecuteCommand(this LibiadaDatabaseEntities db, string query, params NpgsqlParameter[] parameters)
    {
        // TODO: check if this is the optimal way
        db.Database.ExecuteSqlRaw(query, parameters);
    }

    /// <summary>
    /// Gets the db name.
    /// </summary>
    public static string TryGetDbName(this LibiadaDatabaseEntities db)
    {
        try
        {
            var connection = db.Database.GetDbConnection();
            return $"{connection.DataSource}@{connection.Database}";

        }
        catch (Exception e)
        {
            return $"No connection to the database. Reason: {e.Message}";
        }
    }

    /// <summary>
    /// Gets a value indicating whether connection to db established or not.
    /// </summary>
    public static async Task<bool> IsConnectionSuccessful(this LibiadaDatabaseEntities db)
    {
        try
        {
            return await db.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}
