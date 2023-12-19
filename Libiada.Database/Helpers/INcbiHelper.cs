using Bio;
using Bio.IO.GenBank;
using Libiada.Database.Models.NcbiSequencesData;
using System.Collections.Generic;
using System.IO;

namespace Libiada.Database.Helpers
{
    public interface INcbiHelper
    {
        ISequence DownloadGenBankSequence(string accession);
        (string, string) ExecuteEPostRequest(string ids);
        (string, string) ExecuteESearchRequest(string searchTerm);
        List<NuccoreObject> ExecuteESummaryRequest(string searchTerm, bool includePartial);
        List<NuccoreObject> ExecuteESummaryRequest(string ncbiWebEnvironment, string queryKey, bool includePartial);
        Stream GetFastaFileStream(string accession);
        List<FeatureItem> GetFeatures(string accession);
    }
}