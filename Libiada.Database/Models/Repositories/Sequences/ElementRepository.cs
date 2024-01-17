namespace Libiada.Database.Models.Repositories.Sequences;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;

/// <summary>
/// The element repository.
/// </summary>
public class ElementRepository : IElementRepository
{
    /// <summary>
    /// The db.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;

    /// <summary>
    /// The lazy cache.
    /// </summary>
    private Element[] lazyCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// The db.
    /// </param>
    public ElementRepository(LibiadaDatabaseEntities db)
    {
        this.db = db;
    }

    /// <summary>
    /// Gets the cached elements.
    /// </summary>
    private Element[] CachedElements => lazyCache ?? (lazyCache = db.Elements.Where(e => Aliases.StaticNotations.Contains(e.Notation)).ToArray());

    /// <summary>
    /// The dispose.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// The elements in db.
    /// </summary>
    /// <param name="alphabet">
    /// The alphabet.
    /// </param>
    /// <param name="notation">
    /// The notation id.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool ElementsInDb(Alphabet alphabet, Notation notation)
    {
        IEnumerable<string> elements = alphabet.Select(e => e.ToString());

        int existingElementsCount = db.Elements.Count(e => elements.Contains(e.Value) && e.Notation == notation);

        return alphabet.Cardinality == existingElementsCount;
    }

    /// <summary>
    /// The get or create notes in db.
    /// </summary>
    /// <param name="alphabet">
    /// The alphabet.
    /// </param>
    /// <returns>
    /// The <see cref="T:long[]"/>.
    /// </returns>
    public List<long> GetOrCreateNotesInDb(Alphabet alphabet)
    {
        var newNotes = new List<Note>();
        var result = new Note[alphabet.Cardinality];
        ValueNote[] notesAlphabet = alphabet.Cast<ValueNote>().ToArray();
        string[] stringNotes = notesAlphabet.Select(n => n.ToString()).ToArray();
        Dictionary<string, Note> existingNotes = db.Notes.Where(n => stringNotes.Contains(n.Value))
                                                        .ToDictionary(n => n.Value);
        for (int i = 0; i < notesAlphabet.Length; i++)
        {
            ValueNote note = notesAlphabet[i];
            int[] pitches = GetOrCreatePitchesInDb(note.Pitches);
            string localStringNote = stringNotes[i];

            if (existingNotes.ContainsKey(localStringNote))
            {
                result[i] = existingNotes[localStringNote];
                if (note.Triplet != result[i].Triplet
                 || note.Duration.Denominator != result[i].Denominator
                 || note.Duration.Numerator != result[i].Numerator
                 || note.Tie != result[i].Tie)
                {
                    throw new Exception("Found in db note is not equal to local note.");
                }
            }
            else
            {
                result[i] = new Note
                {
                    Value = localStringNote,
                    Triplet = note.Triplet,
                    Denominator = note.Duration.Denominator,
                    Numerator = note.Duration.Numerator,
                    Tie = note.Tie,
                    Pitches = db.Pitches.Where(p => pitches.Contains(p.Id)).ToList(),
                    Notation = Notation.Notes
                };
                newNotes.Add(result[i]);
            }
        }

        db.Notes.AddRange(newNotes);
        db.SaveChanges();
        return result.Select(n => n.Id).ToList();
    }

    /// <summary>
    /// The to db elements.
    /// </summary>
    /// <param name="alphabet">
    /// The alphabet.
    /// </param>
    /// <param name="notation">
    /// The notation id.
    /// </param>
    /// <param name="createElements">
    /// The create elements.
    /// </param>
    /// <returns>
    /// The <see cref="T:long[]"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if alphabet element is not found in db.
    /// </exception>
    public List<long> ToDbElements(Alphabet alphabet, Notation notation, bool createElements)
    {
        if (!ElementsInDb(alphabet, notation))
        {
            if (createElements)
            {
                CreateLackingElements(alphabet, notation);
            }
            else
            {
                throw new Exception("At least one element of alphabet is not found in database.");
            }
        }

        bool staticNotation = Aliases.StaticNotations.Contains(notation);

        string[] stringElements = alphabet.Select(element => element.ToString()).ToArray();

        Element[] elements = staticNotation ?
                        CachedElements.Where(e => e.Notation == notation && stringElements.Contains(e.Value)).ToArray() :
                        db.Elements.Where(e => e.Notation == notation && stringElements.Contains(e.Value)).ToArray();

        return (from stringElement in stringElements
                join element in elements
                on stringElement equals element.Value
                select element.Id).ToList();
    }

    /// <summary>
    /// The to libiada alphabet.
    /// </summary>
    /// <param name="elementIds">
    /// The element ids.
    /// </param>
    /// <returns>
    /// The <see cref="Alphabet"/>.
    /// </returns>
    public Alphabet ToLibiadaAlphabet(long[] elementIds)
    {
        var alphabet = new Alphabet { NullValue.Instance() };
        List<Element> elements = GetElements(elementIds);
        foreach (long elementId in elementIds)
        {
            Element el = elements.Single(e => e.Id == elementId);
            alphabet.Add(new ValueString(el.Value));
        }

        return alphabet;
    }

    /// <summary>
    /// The get elements.
    /// </summary>
    /// <param name="elementIds">
    /// The element ids.
    /// </param>
    /// <returns>
    /// The <see cref="IReadOnlyList{Element}"/>.
    /// </returns>
    public List<Element> GetElements(long[] elementIds) => db.Elements
                                                             .Where(e => elementIds.Contains(e.Id))
                                                             .ToList()
                                                             .OrderBy(e => Array.IndexOf(elementIds, e.Id))
                                                             .ToList();

    /// <summary>
    /// The get or create pitches in db.
    /// </summary>
    /// <param name="pitches">
    /// The pitches.
    /// </param>
    /// <returns>
    /// The <see cref="T:int[]"/>.
    /// </returns>
    private int[] GetOrCreatePitchesInDb(List<Pitch> pitches)
    {
        var newPitches = new List<Models.Pitch>();
        var result = new Models.Pitch[pitches.Count];
        int[] midiNumbers = pitches.Select(p => p.MidiNumber).ToArray();
        Dictionary<int, Models.Pitch> existingPitches = db.Pitches.Where(p => midiNumbers.Contains(p.Midinumber))
                                                                    .ToDictionary(p => p.Midinumber);
        for (int i = 0; i < pitches.Count; i++)
        {
            Pitch pitch = pitches[i];

            if (existingPitches.ContainsKey(pitch.MidiNumber))
            {
                result[i] = existingPitches[pitch.MidiNumber];

                if (pitch.Alter != result[i].Accidental
                 || pitch.Step != result[i].NoteSymbol
                 || pitch.Octave != result[i].Octave)
                {
                    throw new Exception("Found in db pitch is not equal to the local pitch.");
                }
            }
            else
            {
                result[i] = new Database.Models.Pitch
                {
                    Accidental = pitch.Alter,
                    NoteSymbol = pitch.Step,
                    Octave = pitch.Octave,
                    Midinumber = pitch.MidiNumber
                };
                newPitches.Add(result[i]);
            }
        }

        db.Pitches.AddRange(newPitches);
        db.SaveChanges();
        return result.Select(p => p.Id).ToArray();
    }

    /// <summary>
    /// Saves lacking elements to db.
    /// </summary>
    /// <param name="libiadaAlphabet">
    /// The libiada alphabet.
    /// </param>
    /// <param name="notation">
    /// The notation id.
    /// </param>
    private void CreateLackingElements(Alphabet libiadaAlphabet, Notation notation)
    {
        string[] elements = libiadaAlphabet.Select(e => e.ToString()).ToArray();

        List<string> existingElements = db.Elements
                                          .Where(e => elements.Contains(e.Value) && e.Notation == notation)
                                          .Select(e => e.Value)
                                          .ToList();

        List<string> newElements = elements.Where(e => !existingElements.Contains(e)).ToList();
        db.Elements.AddRange(newElements.ConvertAll(e => new Element
                                                            {
                                                                Value = e,
                                                                Name = e,
                                                                Notation = notation
                                                            }));
        db.SaveChanges();
    }
}
