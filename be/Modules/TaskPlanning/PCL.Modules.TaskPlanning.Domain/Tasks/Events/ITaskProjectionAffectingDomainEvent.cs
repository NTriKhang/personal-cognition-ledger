using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events;

public interface ITaskProjectionAffectingDomainEvent : IDomainEvent
{
    TaskId TaskId { get; }
}
