namespace Libiada.Database.Models.NcbiSequencesData;

using Newtonsoft.Json;

public record class ESearchResult
{
    [JsonProperty(PropertyName = "webenv")]
    public required string NcbiWebEnvironment { get; init; }

    public required string QueryKey { get; init; }

    public required int Count { get; init; }
}
