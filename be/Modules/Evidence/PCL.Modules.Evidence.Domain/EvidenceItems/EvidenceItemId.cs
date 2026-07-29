namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public readonly record struct EvidenceItemId(Guid Value)
{
    public static EvidenceItemId New() => new(Guid.NewGuid());

    public static EvidenceItemId From(Guid value) => new(value);
}
