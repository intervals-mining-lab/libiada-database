namespace Libiada.Database.Helpers;

using System.Text;

/// <summary>
/// The file helper.
/// </summary>
public static class FileHelper
{

    /// <summary>
    /// The read sequence from stream.
    /// </summary>
    /// <param name="stream">
    /// The stream.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string ReadSequenceFromStream(Stream stream)
    {
        byte[] input = new byte[stream.Length];
        stream.Read(input, 0, (int)stream.Length);
        stream.Dispose();
        return Encoding.UTF8.GetString(input);
    }
}
