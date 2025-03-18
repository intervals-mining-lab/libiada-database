namespace Libiada.Database.Models.Repositories.Sequences;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;

/// <summary>
/// The Fmotif repository.
/// </summary>
public class FmotifRepository : IFmotifRepository
{
    /// <summary>
    /// Database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;

    /// <summary>
    /// Initializes a new instance of the <see cref="FmotifRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    public FmotifRepository(LibiadaDatabaseEntities db)
    {
        this.db = db;
    }

    /// <summary>
    /// The get or create Fmotifs in db.
    /// </summary>
    /// <param name="alphabet">
    /// The alphabet.
    /// </param>
    /// <returns>
    /// The <see cref="T:long[]"/>.
    /// </returns>
    public long[] GetOrCreateFmotifsInDb(Alphabet alphabet)
    {
        long[] result = new long[alphabet.Cardinality];
        for (int i = 0; i < alphabet.Cardinality; i++)
        {
            result[i] = CreateFmotif((Fmotif)alphabet[i]);
        }

        return result;
    }

    /// <summary>
    /// Saves Fmotifs to db.
    /// </summary>
    /// <param name="fmotif">
    /// The Fmotif.
    /// </param>
    /// <returns>
    /// The <see cref="long"/>.
    /// </returns>
    public long CreateFmotif(Fmotif fmotif)
    {
        Sequence notesSequence = new(fmotif.NoteList.ToList());
        long[] notes = new ElementRepository(db).GetOrCreateNotesInDb(notesSequence.Alphabet);

        string localFmotifHash = fmotif.GetHashCode().ToString();
        List<Models.Fmotif> dbFmotifs = db.Fmotifs.Where(f => f.Value == localFmotifHash).ToList();
        if (dbFmotifs.Count > 0)
        {
            foreach (var dbFmotif in dbFmotifs)
            {
                long[] dbAlphabet = dbFmotif.Alphabet;
                if (notes.SequenceEqual(dbAlphabet))
                {
                    int[] dbOrder = dbFmotif.Order;
                    if (notesSequence.Order.SequenceEqual(dbOrder))
                    {
                        if (fmotif.Type != dbFmotif.FmotifType)
                        {
                            throw new Exception("Fmotif found in database is not equal to the local fmotif.");
                        }

                        return dbFmotif.Id;
                    }
                }
            }
        }

        Models.Fmotif result = new()
        {
            Value = fmotif.GetHashCode().ToString(),
            FmotifType = fmotif.Type,
            Alphabet = notes,
            Notation = Notation.FormalMotifs,
            Order = notesSequence.Order
        };

        db.Fmotifs.Add(result);
        db.SaveChanges();
        return fmotif.Id;
    }

    /// <summary>
    /// The dispose.
    /// </summary>
    public void Dispose()
    {
    }
}
