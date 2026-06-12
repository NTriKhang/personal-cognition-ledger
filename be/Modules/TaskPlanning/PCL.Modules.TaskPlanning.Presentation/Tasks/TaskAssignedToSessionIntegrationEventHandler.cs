using Common.Application.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;
using PCL.Modules.Session.Contracts.LSessions;
using PCL.Modules.TaskPlanning.Application.Tasks.RecordTaskAssignedToSession;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks;

internal sealed class TaskAssignedToSessionIntegrationEventHandler(
    ISender sender,
    ILogger<TaskAssignedToSessionIntegrationEventHandler> logger)
    : IntegrationEventHandler<TaskAssignedToSessionIntegrationEvent>
{
    public override async Task Handle(
        TaskAssignedToSessionIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new RecordTaskAssignedToSessionCommand(
            integrationEvent.TaskId,
            integrationEvent.OwnerId,
            integrationEvent.SessionId,
            integrationEvent.AssignedAt);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "TaskPlanning skipped task assignment event {IntegrationEventId}. Error: {ErrorCode}",
                integrationEvent.Id,
                result.Error.Code);
        }
    }
}
