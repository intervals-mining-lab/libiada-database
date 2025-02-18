namespace Libiada.Database.Models.Repositories.Sequences;

using System.Text.RegularExpressions;

/// <summary>
/// 
/// </summary>
public class MultisequenceRepository
{
    /// <summary>
    /// Array of sequence types used in multisequence grouping.
    /// </summary>
    public static readonly SequenceType[] SequenceTypesFilter =
    [
            SequenceType.CompleteGenome,
            SequenceType.MitochondrialGenome,
            SequenceType.ChloroplastGenome,
            SequenceType.Plasmid,
            SequenceType.Plastid,
            SequenceType.MitochondrialPlasmid
    ];

    private static readonly Dictionary<int, string> RomanDigits = new()
    {
        { 1000, "M" },
        { 900, "CM" },
        { 500, "D" },
        { 400, "CD" },
        { 100, "C" },
        { 90 , "XC" },
        { 50 , "L" },
        { 40 , "XL" },
        { 10 , "X" },
        { 9  , "IX" },
        { 5  , "V" },
        { 4  , "IV" },
        { 1  , "I" }
    };

    public static readonly Func<ResearchObject, bool> ResearchObjectsFilter = new(m => SequenceTypesFilter.Contains(m.SequenceType) && m.MultisequenceId == null);

    /// <summary>
    /// Converts roman numbers to arabic.
    /// </summary>
    /// <param name="number">
    /// Number to convert.
    /// </param>
    /// <returns>
    /// Returns arabic number.
    /// </returns>
    public static int ToArabic(string number) => number.Length == 0 ? 0 :
        RomanDigits
            .Where(d => number.StartsWith(d.Value))
            .Select(d => d.Key + ToArabic(number.Substring(d.Value.Length)))
            .FirstOrDefault();

    /// <summary>
    /// Discards excess parts.
    /// </summary>
    /// <param name="researchObjectName">
    /// The research object name.
    /// </param>
    /// <returns>
    /// Returns research object name without excess parts.
    /// </returns>
    public static string GetResearchObjectNameSplit(string researchObjectName)
    {
        string[] splitName = researchObjectName.Split('|');
        return splitName.Length > 2 ? splitName[1].Trim() : splitName[0].Trim();
    }

    /// <summary>
    /// Identifies sequence number.
    /// </summary>
    /// <param name="researchObjectName">
    /// The research object name.
    /// </param>
    /// <returns>
    /// Returns sequence number.
    /// </returns>
    public static int GetSequenceNumberByName(string researchObjectName)
    {
        List<string> splitName = researchObjectName.Split(' ').ToList();
        if (researchObjectName.Contains("chromosome"))
        {
            int chromosomeWordIndex = splitName.IndexOf("chromosome");
            if (splitName.IndexOf("chromosome") < splitName.Count - 1)
            {
                if (splitName[chromosomeWordIndex + 1].Replace(".", string.Empty).All(char.IsDigit))
                {
                    return Convert.ToInt32(splitName[chromosomeWordIndex + 1].Replace(".", string.Empty));
                }
                else
                {
                    return ToArabic(splitName[chromosomeWordIndex + 1].Replace(".", string.Empty));
                }
            }
            else
            {
                return 0;
            }
        }
        else if (researchObjectName.Contains("segment"))
        {
            int segmentWordIndex = splitName.IndexOf("segment");
            if (splitName[segmentWordIndex + 1].Contains("RNA"))
            {
                return Convert.ToInt32(splitName[splitName.IndexOf("RNA") + 1].Replace(".", string.Empty));
            }
            else if (splitName[segmentWordIndex + 1].All(char.IsDigit))
            {
                return Convert.ToInt32(splitName[segmentWordIndex + 1]);
            }
            else
            {
                return Convert.ToChar(splitName[segmentWordIndex + 1]) - 64;
            }
        }
        else if (researchObjectName.Contains("plasmid"))
        {
            int plasmidWordIndex = splitName.IndexOf("plasmid");
            if (splitName[plasmidWordIndex + 1].Length > 1 && !splitName[plasmidWordIndex + 1].All(char.IsDigit))
            {
                bool check = false;
                foreach (char ch in splitName[plasmidWordIndex + 1])
                {
                    if (char.IsNumber(ch))
                    {
                        check = true;
                    }
                }
                if (check)
                {
                    return Convert.ToInt32(Regex.Replace(splitName[plasmidWordIndex + 1], @"[^\d]+", ""));
                }
                else
                {
                    return splitName[plasmidWordIndex + 1].ToCharArray()[0] - 64;
                }
            }
            else if (splitName[plasmidWordIndex + 1].All(char.IsDigit))
            {
                return Convert.ToInt32(splitName[plasmidWordIndex + 1]);
            }
            else
            {
                return splitName[plasmidWordIndex + 1].ToCharArray()[0] - 64;
            }
        }

        return 0;
    }

    /// <summary>
    /// Sets multisequence numbers to research object array.
    /// </summary>
    /// <param name="researchObjects">
    /// The array of research objects.
    /// </param>
    public static void SetSequenceNumbers(ResearchObject[] researchObjects)
    {
        short counter = 1;

        foreach (ResearchObject researchObject in researchObjects)
        {
            if (SequenceTypesFilter.Contains(researchObject.SequenceType))
            {
                researchObject.MultisequenceNumber = counter++;
            }
        }
    }
}
