namespace Libiada.Database.Helpers;

using Bio.IO;
using Bio.IO.FastA;
using Bio.IO.GenBank;

using Libiada.Database.Models.NcbiSequencesData;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

/// <summary>
/// The ncbi helper.
/// </summary>
public class NcbiHelper : INcbiHelper
{
    /// <summary>
    /// The base url for all eutils.
    /// </summary>
    private readonly Uri BaseAddress = new(@"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/");

    /// <summary>
    /// Synchronization object.
    /// </summary>
    private static readonly object SyncRoot = new();

    private readonly string ApiKey;

    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// The last request date time.
    /// </summary>
    private DateTimeOffset lastRequestDateTime = DateTimeOffset.Now;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger;

    public NcbiHelper(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<NcbiHelper> logger)
    {
        ApiKey = config["NcbiApiKey"] ?? throw new Exception($"NcbiApiKey is not found in confiuguration.");
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <summary>
    /// Extracts features from genBank file downloaded from ncbi.
    /// </summary>
    /// <param name="accession">
    /// Accession id of the sequence in ncbi (remote id).
    /// </param>
    /// <returns>
    /// The <see cref="List{Bio.IO.GenBank.FeatureItem}"/>.
    /// </returns>
    public List<FeatureItem> GetFeatures(string accession)
    {
        GenBankMetadata metadata = GetMetadata(DownloadGenBankSequence(accession));
        return metadata.Features.All;
    }

    /// <summary>
    /// Extracts metadata from genbank file.
    /// </summary>
    /// <param name="sequence">
    /// Sequence extracted from genbank file.
    /// </param>
    /// <returns>
    /// The <see cref="GenBankMetadata"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if metadata is absent.
    /// </exception>
    public static GenBankMetadata GetMetadata(Bio.ISequence sequence)
    {
        return sequence.Metadata["GenBank"] as GenBankMetadata ?? throw new Exception("GenBank file metadata is empty.");
    }

    /// <summary>
    /// Downloads sequence as fasta file from ncbi.
    /// </summary>
    /// <param name="fastaFileStream">
    /// The fasta file stream.
    /// </param>
    /// <returns>
    /// The <see cref="ISequence"/>.
    /// </returns>
    public static Bio.ISequence GetFastaSequence(Stream fastaFileStream)
    {
        FastAParser fastaParser = new();
        Bio.ISequence result = fastaParser.ParseOne(fastaFileStream);
        fastaFileStream.Dispose();
        return result;
    }

    /// <summary>
    /// The get file.
    /// </summary>
    /// <param name="accession">
    /// Accession id of the sequence in ncbi (remote id).
    /// </param>
    /// <returns>
    /// The <see cref="Stream"/>.
    /// </returns>
    public Stream GetFastaFileStream(string accession)
    {
        string url = GetEfetchParamsString("fasta", accession);
        return GetResponseStream(url);
    }

    /// <summary>
    /// Extracts sequence from genbank file.
    /// </summary>
    /// <param name="accession">
    /// Accession id of the sequence in ncbi (remote id).
    /// </param>
    /// <returns>
    /// The <see cref="Stream"/>.
    /// </returns>
    public Bio.ISequence DownloadGenBankSequence(string accession)
    {
        ISequenceParser parser = new GenBankParser();
        string url = GetEfetchParamsString("gbwithparts", accession);
        using MemoryStream dataStream = GetResponseStream(url);
        return parser.ParseOne(dataStream);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="data">
    /// 
    /// </param>
    /// <param name="includePartial">
    /// 
    /// </param>
    /// <param name="minLength">
    /// 
    /// </param>
    /// <param name="maxLength">
    /// 
    /// </param>
    /// <returns></returns>
    public static string[] GetIdsFromNcbiSearchResults(
        string data,
        bool includePartial,
        int minLength = 1,
        int maxLength = int.MaxValue)
    {
        string[] searchResults = Regex.Split(data, @"^\r\n", RegexOptions.Multiline);
        List<string> accessions = [];

        foreach (string block in searchResults)
        {
            if (!string.IsNullOrEmpty(block))
            {
                string[] blockLines = block.Split('\n');
                string seqenceName = blockLines[0];
                string sequenceLength = blockLines[1];
                string accession = blockLines[2];

                if (includePartial || !seqenceName.Contains("partial"))
                {
                    int length = GetLengthFromString(sequenceLength);
                    if (length >= minLength && length <= maxLength)
                    {
                        string[] idStrings = accession.Split(' ');
                        accessions.Add(idStrings[0]);
                    }
                }
            }
        }

        return accessions.ToArray();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="searchTerm">
    /// 
    /// </param>
    /// <param name="includePartial">
    /// 
    /// </param>
    /// <returns>
    /// 
    /// </returns>
    public List<NuccoreObject> ExecuteESummaryRequest(string searchTerm, bool includePartial)
    {
        logger.LogInformation($"Executing ncbi ESummary request with search term: {searchTerm}", DateTime.UtcNow.ToLongTimeString());
        (string ncbiWebEnvironment, string queryKey) = ExecuteESearchRequest(searchTerm);
        return ExecuteESummaryRequest(ncbiWebEnvironment, queryKey, includePartial);
    }

    public List<NuccoreObject> ExecuteESummaryRequest(string ncbiWebEnvironment, string queryKey, bool includePartial)
    {
        List<NuccoreObject> nuccoreObjects = [];
        const short retmax = 500;
        int retstart = 0;
        List<ESummaryResult> eSummaryResults = [];

        do
        {
            string urlEsummary = $"esummary.fcgi?db=nuccore&retmode=json" +
                                  $"&WebEnv={ncbiWebEnvironment}" +
                                  $"&query_key={queryKey}" +
                                  $"&retmax={retmax}&retstart={retstart}";
            string esummaryResponse = GetResponceString(urlEsummary);
            retstart += retmax;

            JToken? esummaryResultJObject = JObject.Parse(esummaryResponse)["result"];
            if (esummaryResultJObject != null)
            {
                // removing array of uids because it breakes deserialization
                //eSummaryResults = esummaryResultJObject
                //                        .Children<JProperty>()
                //                        .Where(j => j.Name != "uids")
                //                        .Select(j => j.Value.ToObject<ESummaryResult>())
                //                        .ToList();

                // alternative implementation
                ((JObject)esummaryResultJObject).Remove("uids");
                string esummaryResultSring = esummaryResultJObject.ToString();
                Dictionary<string, ESummaryResult> eSummaryResultsDictionary = JsonConvert.DeserializeObject<Dictionary<string, ESummaryResult>>(esummaryResultSring)
                    ?? throw new Exception($"Invalid esummary responce: {esummaryResultSring}");
                eSummaryResults = eSummaryResultsDictionary.Values.ToList();

                foreach (ESummaryResult result in eSummaryResults)
                {
                    bool isPartial = result.Title.Contains("partial") || string.IsNullOrEmpty(result.Completeness);
                    if (includePartial || !isPartial)
                    {
                        NuccoreObject nuccoreObject = new()
                        {
                            Title = result.Title,
                            Organism = result.Organism,
                            AccessionVersion = result.AccessionVersion,
                            Completeness = result.Completeness,
                            UpdateDate = result.UpdateDate
                        };
                        nuccoreObjects.Add(nuccoreObject);
                    }
                }
            }
        } while (eSummaryResults.Count == retmax);

        return nuccoreObjects;
    }

    /// <summary>
    /// Requests web environment and query key from ncbi for given seqrch query.
    /// </summary>
    /// <param name="searchTerm">
    /// Search term of first part of search term.
    /// </param>
    /// <returns>
    /// Tuple of NcbiWebEnvironment and QueryKey <see cref="string"/>s.
    /// </returns>
    public (string, string) ExecuteESearchRequest(string searchTerm)
    {
        logger.LogInformation($"Executing ncbi ESearch request  with search term: {searchTerm}", DateTime.UtcNow.ToLongTimeString());
        string urlEsearch = $"esearch.fcgi?db=nuccore&term={searchTerm}&usehistory=y&retmode=json";
        string esearchResponseString = GetResponceString(urlEsearch);
        ESearchResponce eSeqrchReasponce = JsonConvert.DeserializeObject<ESearchResponce>(esearchResponseString)
            ?? throw new Exception($"Invalid esearch responce: {esearchResponseString}");
        ESearchResult eSearchResult = eSeqrchReasponce.ESearchResult;
        return (eSearchResult.NcbiWebEnvironment, eSearchResult.QueryKey);
    }

    /// <summary>
    /// Executes EPost request and returns it's result.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public (string, string) ExecuteEPostRequest(string ids)
    {
        string data = $"db=nuccore&id={ids}";

        // adding api key to request if there is any
        if (!string.IsNullOrEmpty(ApiKey))
        {
            data += $"&api_key={ApiKey}";
        }

        const string urlEPost = $"epost.fcgi";
        using HttpClient httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = BaseAddress;
        StringContent postData = new(data, Encoding.UTF8, "application/x-www-form-urlencoded");

        WaitForRequest();

        logger.LogInformation($"Executing ncbi post request {BaseAddress}{urlEPost} with data {data}", DateTime.UtcNow.ToLongTimeString());
        using HttpResponseMessage response = httpClient.PostAsync(urlEPost, postData)
            .ContinueWith((postTask) => postTask.Result.EnsureSuccessStatusCode()).Result;

        string requestResult = response.Content.ReadAsStringAsync().Result;

        XmlDocument xmlReader = new();
        xmlReader.LoadXml(requestResult);
        XmlNode? result = xmlReader.LastChild;
        (XmlElement? webEnv, XmlElement? queryKey) = (result?["WebEnv"], result?["QueryKey"]);
        
        if (webEnv?.FirstChild?.Value is null || queryKey?.FirstChild?.Value is null) throw new Exception("EPost request result is invalid");
        
        return (webEnv.FirstChild.Value, queryKey.FirstChild.Value);
    }

    /// <summary>
    /// Adds length linitations on sought sequences if any provided.
    /// </summary>
    /// <param name="searchTerm">
    /// Search term for all fields.
    /// </param>
    /// <param name="minLength">
    /// Minimal sequence length.
    /// </param>
    /// <param name="maxLength">
    /// Maximum sequence length.
    /// </param>
    /// <returns>
    /// Search term as is or formatet seqrch term with length limitations.
    /// </returns>
    public static string FormatNcbiSearchTerm(string searchTerm, int? minLength = null, int? maxLength = null)
    {
        if (minLength == null && maxLength == null)
        {
            if (searchTerm.Contains('[')) return searchTerm;

            return $"\"{searchTerm}\"[Organism]";
        }

        if (searchTerm.Contains('[')) return $"""{searchTerm} AND ("{minLength ?? 1}"[SLEN] : "{maxLength ?? int.MaxValue}"[SLEN])""";

        return $"\"{searchTerm}\"[Organism] AND (\"{minLength ?? 1}\"[SLEN] : \"{maxLength ?? int.MaxValue}\"[SLEN])";
    }

    /// <summary>
    /// Converts string formated in en-US culture into integer.
    /// </summary>
    /// <param name="integer">
    ///  String containing integer in american format.
    /// </param>
    /// <returns>
    /// Integer value.
    /// </returns>
    /// <example>
    /// (string)"1,111,520" => (int)1111520
    /// </example>
    private static int GetLengthFromString(string integer)
    {
        integer = integer.Split(' ')[0];
        IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
        return int.Parse(integer, NumberStyles.Integer | NumberStyles.AllowThousands, provider);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="url">
    /// 
    /// </param>
    /// <returns>
    /// 
    /// </returns>
    private string GetResponceString(string url)
    {
        logger.LogInformation($"Getting ncbi responce string {BaseAddress}{url}", DateTime.UtcNow.ToLongTimeString());
        using Stream response = GetResponseStream(url);
        using StreamReader reader = new(response);
        string responseText = reader.ReadToEnd();

        return responseText;
    }

    /// <summary>
    /// Creates efetch params string with given return type.
    /// </summary>
    /// <param name="retType">
    /// Response returned type.
    /// </param>
    /// <param name="accessions">
    /// Sequences acessions in genBank.
    /// </param>
    /// <returns>
    /// efetch part of url with params as <see cref="string"/>.
    /// </returns>
    private static string GetEfetchParamsString(string retType, string accessions)
    {
        return $"efetch.fcgi?db=nuccore&retmode=text&rettype={retType}&id={accessions}";
    }

    /// <summary>
    /// Downloads response from base url with given params.
    /// </summary>
    /// <param name="url">
    /// The params url (without base url).
    /// </param>
    /// <returns>
    /// The response as <see cref="MemoryStream"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if response stream is null.
    /// </exception>
    private MemoryStream GetResponseStream(string url)
    {
        // adding api key to request if there is any
        if (!string.IsNullOrEmpty(ApiKey))
        {
            url += $"&api_key={ApiKey}";
        }

        MemoryStream memoryStream = new();

        WaitForRequest();

        using HttpClient httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = BaseAddress;
        logger.LogInformation($"Executing ncbi request {BaseAddress}{url}", DateTime.UtcNow.ToLongTimeString());
        using Stream stream = httpClient.GetStreamAsync(url).Result ?? throw new Exception("Response stream was null.");
        stream.CopyTo(memoryStream);

        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// NCBI allows only 3 (10 for registered users) requests per second.
    /// So we wait between requests to be sure.
    /// </summary>
    private void WaitForRequest()
    {
        lock (SyncRoot)
        {
            int delay = string.IsNullOrEmpty(ApiKey) ? 334 : 100;

            // calculationg time to next request
            TimeSpan timeToRequest = (lastRequestDateTime + TimeSpan.FromMilliseconds(delay)) - DateTimeOffset.UtcNow;

            if (timeToRequest > TimeSpan.Zero)
            {
                Thread.Sleep(timeToRequest);
            }

            lastRequestDateTime = DateTimeOffset.UtcNow;
        }
    }
}
