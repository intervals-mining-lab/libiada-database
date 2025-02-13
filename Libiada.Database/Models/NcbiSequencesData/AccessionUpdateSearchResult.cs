namespace Libiada.Database.Models.NcbiSequencesData;

public record class AccessionUpdateSearchResult
{
    public required string LocalAccession { get; init; }
    public string? RemoteName { get; set; }
    public required string Name { get; init; }
    public string? RemoteOrganism { get; set; }
    public required byte LocalVersion { get; init; }
    public byte RemoteVersion { get; set; }
    public required string LocalUpdateDate { get; init; }
    public required DateTimeOffset LocalUpdateDateTime { get; init; }
    public string? RemoteUpdateDate { get; set; }
    public bool Updated { get; set; }
    public bool NameUpdated { get; set; }
}