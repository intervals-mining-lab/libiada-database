namespace Libiada.Database.Models.Repositories.Sequences;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;

/// <summary>
/// The measure repository.
/// </summary>
public class MeasureRepository : IMeasureRepsitory
{
    /// <summary>
    /// Database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;

    /// <summary>
    /// Initializes a new instance of the <see cref="MeasureRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    public MeasureRepository(LibiadaDatabaseEntities db)
    {
        this.db = db;
    }

    /// <summary>
    /// Gets (or creates if there are none) measure from database.
    /// </summary>
    /// <param name="alphabet">
    /// The alphabet.
    /// </param>
    /// <returns>
    /// Measures id in datadabe as <see cref="long[]"/>
    /// </returns>
    public long[] GetOrCreateMeasuresInDb(Alphabet alphabet)
    {
        var result = new long[alphabet.Cardinality];
        for (int i = 0; i < alphabet.Cardinality; i++)
        {
            result[i] = CreateMeasure((Measure)alphabet[i]);
        }

        db.SaveChanges();
        return result;
    }

    /// <summary>
    /// Saves measures to database.
    /// </summary>
    /// <param name="measure">
    /// The measure.
    /// </param>
    /// <returns>
    /// The <see cref="long"/>.
    /// </returns>
    public long CreateMeasure(Measure measure)
    {
        var measureChain = new BaseChain(measure.NoteList.Cast<IBaseObject>().ToList());
        long[] notes = new ElementRepository(db).GetOrCreateNotesInDb(measureChain.Alphabet);

        string localMeasureHash = measure.GetHashCode().ToString();
        var dbMeasures = db.Measures.Where(m => m.Value == localMeasureHash).ToList();
        if (dbMeasures.Count > 0)
        {
            foreach (var dbMeasure in dbMeasures)
            {
                long[] dbAlphabet = dbMeasure.Alphabet;
                if (notes.SequenceEqual(dbAlphabet))
                {
                    int[] dbOrder = dbMeasure.Order;
                    if (measureChain.Order.SequenceEqual(dbOrder))
                    {
                        if (measure.Attributes.Key.Fifths != dbMeasure.Fifths
                            || measure.Attributes.Size.BeatBase != dbMeasure.Beatbase
                            || measure.Attributes.Size.Beats != dbMeasure.Beats)
                        {
                            throw new Exception("Found in db measure is not equal to local measure.");
                        }

                        return dbMeasure.Id;
                    }
                }
            }
        }

        var mode = measure.Attributes.Key.Mode;
        var result = new Models.Measure
        {
            //Id = db.GetNewElementId(),
            Alphabet = notes,
            Order = measureChain.Order,
            Value = measure.GetHashCode().ToString(),
            Beats = measure.Attributes.Size.Beats,
            Beatbase = measure.Attributes.Size.BeatBase,
            Fifths = measure.Attributes.Key.Fifths,
            Major = mode.Equals("major") || mode.Equals(null)
        };

        db.Measures.Add(result);
        db.SaveChanges();
        return measure.Id;
    }

    /// <summary>
    /// The dispose.
    /// </summary>
    public void Dispose()
    {
    }
}
