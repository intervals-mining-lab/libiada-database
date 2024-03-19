namespace Libiada.Database.Models.Repositories.Calculators;

/// <summary>
/// The characteristic repository.
/// </summary>
public class CharacteristicRepository : ICharacteristicRepository
{
    /// <summary>
    /// Database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacteristicRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    public CharacteristicRepository(LibiadaDatabaseEntities db)
    {
        this.db = db;
    }

    /// <summary>
    /// Tries to save characteristics to the database.
    /// </summary>
    /// <param name="characteristics">
    /// The characteristics.
    /// </param>
    public void TrySaveCharacteristicsToDatabase(List<CharacteristicValue> characteristics)
    {
        if (characteristics.Count > 0)
        {
            try
            {
                db.CharacteristicValues.AddRange(characteristics);
                db.SaveChanges();
            }
            catch (Exception)
            {
                // todo: refactor and optimize all this
                long[] characteristicsSequences = characteristics.Select(c => c.SequenceId).Distinct().ToArray();
                short[] characteristicsTypes = characteristics.Select(c => c.CharacteristicLinkId).Distinct().ToArray();
                var characteristicsFilter = characteristics.Select(c => new { c.SequenceId, c.CharacteristicLinkId }).ToArray();
                var wasteCharacteristics = db.CharacteristicValues.Where(c => characteristicsSequences.Contains(c.SequenceId) && characteristicsTypes.Contains(c.CharacteristicLinkId))
                        .ToArray().Where(c => characteristicsFilter.Contains(new { c.SequenceId, c.CharacteristicLinkId })).Select(c => new { c.SequenceId, c.CharacteristicLinkId });
                var wasteNewCharacteristics = characteristics.Where(c => wasteCharacteristics.Contains(new { c.SequenceId, c.CharacteristicLinkId }));

                db.CharacteristicValues.RemoveRange(wasteNewCharacteristics);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    /// <summary>
    /// Tries to save characteristics to the database.
    /// </summary>
    /// <param name="characteristics">
    /// The characteristics.
    /// </param>
    public void TrySaveCharacteristicsToDatabase(List<CongenericCharacteristicValue> characteristics)
    {
        if (characteristics.Count > 0)
        {
            try
            {
                db.CongenericCharacteristicValues.AddRange(characteristics);
                db.SaveChanges();
            }
            catch (Exception)
            {
                // todo: refactor and optimize all this
                long[] characteristicsSequences = characteristics.Select(c => c.SequenceId).Distinct().ToArray();
                short[] characteristicsTypes = characteristics.Select(c => c.CharacteristicLinkId).Distinct().ToArray();
                long[] characteristicsElements = characteristics.Select(c => c.ElementId).Distinct().ToArray();
                var characteristicsFilter = characteristics.Select(c => new { c.SequenceId, c.CharacteristicLinkId, c.ElementId }).ToArray();
                var wasteCharacteristics = db.CongenericCharacteristicValues.Where(c => characteristicsSequences.Contains(c.SequenceId) 
                                                                                     && characteristicsTypes.Contains(c.CharacteristicLinkId)
                                                                                     && characteristicsElements.Contains(c.ElementId))
                        .ToArray().Where(c => characteristicsFilter.Contains(new { c.SequenceId, c.CharacteristicLinkId, c.ElementId })).Select(c => new { c.SequenceId, c.CharacteristicLinkId, c.ElementId });
                var wasteNewCharacteristics = characteristics.Where(c => wasteCharacteristics.Contains(new { c.SequenceId, c.CharacteristicLinkId, c.ElementId }));

                db.CongenericCharacteristicValues.RemoveRange(wasteNewCharacteristics);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    /// <summary>
    /// The dispose.
    /// </summary>
    public void Dispose()
    {
    }
}
