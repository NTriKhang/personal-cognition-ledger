namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public static class EvidenceFilePolicy
{
    public const long MaximumFileSizeBytes = 25 * 1024 * 1024;
    public const string ChecksumAlgorithm = "SHA256";

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/webp",
            "text/plain"
        };

    public static bool IsAllowedContentType(string contentType) =>
        AllowedContentTypes.Contains(contentType);

    public static bool IsValidFileSize(long fileSizeBytes) =>
        fileSizeBytes > 0 && fileSizeBytes <= MaximumFileSizeBytes;

    public static bool IsValidChecksum(string algorithm, string value)
    {
        if (!string.Equals(algorithm, ChecksumAlgorithm, StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            return Convert.FromBase64String(value).Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
