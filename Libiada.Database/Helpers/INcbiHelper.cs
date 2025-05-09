﻿namespace Libiada.Database.Helpers;

using Bio.IO.GenBank;
using Libiada.Database.Models.NcbiSequencesData;

public interface INcbiHelper
{
    Bio.ISequence DownloadGenBankSequence(string accession);
    (string, string) ExecuteEPostRequest(string ids);
    (string, string) ExecuteESearchRequest(string searchTerm);
    List<NuccoreObject> ExecuteESummaryRequest(string searchTerm, bool includePartial);
    List<NuccoreObject> ExecuteESummaryRequest(string ncbiWebEnvironment, string queryKey, bool includePartial);
    Stream GetFastaFileStream(string accession);
    List<FeatureItem> GetFeatures(string accession);
}
