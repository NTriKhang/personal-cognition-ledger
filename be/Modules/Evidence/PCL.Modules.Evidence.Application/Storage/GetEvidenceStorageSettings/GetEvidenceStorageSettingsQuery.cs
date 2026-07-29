using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.Storage.GetEvidenceStorageSettings;

public sealed record GetEvidenceStorageSettingsQuery
    : IQuery<EvidenceStorageSettingsReadModel>;
