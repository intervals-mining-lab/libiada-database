namespace Libiada.Database.Models.Repositories.Sequences;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;

/// <summary>
/// The Fmotif repository.
/// </summary>
public class FmotifRepository : IFmotifRepository
{
    /// <summary>
    /// The db.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;

    /// <summary>
    /// Initializes a new instance of the <see cref="FmotifRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// The db.
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
    public List<long> GetOrCreateFmotifsInDb(Alphabet alphabet)
    {
        var result = new List<long>(alphabet.Cardinality);
        for (int i = 0; i < alphabet.Cardinality; i++)
        {
            result.Add(CreateFmotif((Fmotif)alphabet[i]));
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
        var notesChain = new BaseChain(fmotif.NoteList.ToList());
        List<long> notes = new ElementRepository(db).GetOrCreateNotesInDb(notesChain.Alphabet);

        var localFmotifHash = fmotif.GetHashCode().ToString();
        var dbFmotifs = db.Fmotifs.Where(f => f.Value == localFmotifHash).ToList();
        if (dbFmotifs.Count > 0)
        {
            foreach (var dbFmotif in dbFmotifs)
            {
                long[] dbAlphabet = dbFmotif.Alphabet.ToArray();
                if (notes.SequenceEqual(dbAlphabet))
                {
                    int[] dbOrder = dbFmotif.Order.ToArray();
                    if (notesChain.Order.SequenceEqual(dbOrder))
                    {
                        if (fmotif.Type != dbFmotif.FmotifType)
                        {
                            throw new Exception("Fmotif found in db is not equal to the local fmotif.");
                        }

                        return dbFmotif.Id;
                    }
                }
            }
        }

        var result = new Models.Fmotif
        {
            //Id = db.GetNewElementId(),
            Value = fmotif.GetHashCode().ToString(),
            Notation = Notation.FormalMotifs,
            FmotifType = fmotif.Type,
            Alphabet = notes,
            Order = notesChain.Order.ToList()
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
