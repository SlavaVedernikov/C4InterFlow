using C4InterFlow.Commons.Extensions;

namespace C4InterFlow.Structures;

public record StructureIdentity
{
    private string Key { get; }
    private string? Instance { get; }
    public string Value => Instance is null ? Key : $"{Key}.{Instance.GenerateSlug(".")}";


    public StructureIdentity(string key, string? instance = null) => (Key, Instance) = (key, instance);
}