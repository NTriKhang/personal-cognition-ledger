namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public readonly record struct EvidenceFileUploadAttemptId(Guid Value)
{
    public static EvidenceFileUploadAttemptId New() => new(Guid.NewGuid());

    public static EvidenceFileUploadAttemptId From(Guid value) => new(value);
}
