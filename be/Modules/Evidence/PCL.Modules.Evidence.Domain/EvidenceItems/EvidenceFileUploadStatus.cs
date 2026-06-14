namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public enum EvidenceFileUploadStatus
{
    Pending = 1,
    Verifying = 2,
    Ready = 3,
    Failed = 4,
    Expired = 5,
    Cancelled = 6
}
