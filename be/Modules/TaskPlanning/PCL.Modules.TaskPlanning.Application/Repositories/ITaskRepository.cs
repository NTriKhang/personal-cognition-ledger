using PCL.Modules.TaskPlanning.Domain.Tasks;
using PlanningTask = PCL.Modules.TaskPlanning.Domain.Tasks.Task;

namespace PCL.Modules.TaskPlanning.Application.Repositories
{
    public interface ITaskRepository
    {
        void Add(PlanningTask task);

        System.Threading.Tasks.Task<PlanningTask?> GetAsync(
            TaskId id,
            CancellationToken cancellationToken = default);
    }
}
